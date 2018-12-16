#region copyright
// -----------------------------------------------------------------------
//  <copyright file="Supervision.cs" company="Akka.NET Project">
//      Copyright (C) 2009-2018 Lightbend Inc. <http://www.lightbend.com>
//      Copyright (C) 2013-2018 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------
#endregion

using System;
using System.Threading.Tasks;
using Akka.Util;
using Microsoft.Extensions.Logging;

namespace Akka.Typed.Internal
{
    internal delegate T Catcher<out T, in TException>(TException exception) where TException : Exception;

    internal static class Supervisor
    {
        public static Behavior<T> Supervise<T, TError>(this Behavior<T> behavior, SupervisorStrategy strategy) 
            where T : class
            where TError : Exception
        {
            switch (strategy)
            {
                case Resume resume: return behavior.Intercept(new ResumeSupervisor<T,TError>(resume, behavior));
                case Restart restart: return behavior.Intercept(new RestartSupervisor<T, TError>(restart, behavior));
                case Stop stop: return behavior.Intercept(new StopSupervisor<T, TError>(stop, behavior));
                case Backoff backoff: return behavior.Intercept(new BackoffSupervisor<T, TError>(backoff, behavior));
                default:
                    Raises.NotSupported($"Supervisor strategy [{strategy?.GetType()}] is not supported. Supported strategies are: Restart, Resume, Stop, Backoff.");
                    return default;
            }
        }
    }

    internal abstract class AbstractSupervisor<TOut, TIn, TException> : IBehaviorInterceptor<TOut, TIn> 
        where TIn : class 
        where TOut : class
        where TException : Exception
    {
        protected readonly SupervisorStrategy Strategy;

        protected AbstractSupervisor(SupervisorStrategy strategy)
        {
            Strategy = strategy;
        }

        public virtual async ValueTask<Behavior<TIn>> AroundSignal<TSignalTarget>(IActorContext<TOut> context, ISignal signal, TSignalTarget target) 
            where TSignalTarget : ISignalTarget<TIn>
        {
            try
            {
                return await target.Apply(context, signal);
            }
            catch (TException exception)
            {
                return await HandleSignalException(context, target);
            }
        }

        public async ValueTask<Behavior<TIn>> AroundStart<TPreStartTarget>(IActorContext<TOut> context, TPreStartTarget target) where TPreStartTarget : IPreStartTarget<TIn>
        {
            try
            {
                return await target.Start(context);
            }
            catch (TException exception)
            {
                return await HandleExceptionOnStart(context);
            }
        }

        protected void Log(IActorContext<TOut> context, TException exception)
        {
            if (Strategy.IsLoggingEnabled)
            {
                context.Log.LogError(exception, "Supervisor {0} saw failure: {1}", this, exception);
            }
        }

        public abstract ValueTask<Behavior<TIn>> AroundReceive<TReceiveTarget>(IActorContext<TOut> context, TOut message, TReceiveTarget target) where TReceiveTarget : IReceiveTarget<TIn>;
        protected abstract ValueTask<Behavior<TIn>> HandleExceptionOnStart(IActorContext<TOut> context);
        protected abstract ValueTask<Behavior<TIn>> HandleSignalException(IActorContext<TOut> context, ISignalTarget<TIn> target);
        protected abstract ValueTask<Behavior<TIn>> HandleReceiveException(IActorContext<TOut> context, IReceiveTarget<TIn> target);
    }

    internal abstract class SimpleSupervisor<T, TException> : AbstractSupervisor<T, T, TException>
        where T : class
        where TException : Exception
    {
        protected SimpleSupervisor(SupervisorStrategy strategy) : base(strategy) { }

        public override async ValueTask<Behavior<T>> AroundReceive<TReceiveTarget>(IActorContext<T> context, T message, TReceiveTarget target)
        {
            try
            {
                return await target.Apply(context, message);
            }
            catch (TException exception)
            {
                return await HandleReceiveException(context, target);
            }
        }

        protected override ValueTask<Behavior<T>> HandleExceptionOnStart(IActorContext<T> context)
        {
            throw new NotImplementedException();
        }

        protected override ValueTask<Behavior<T>> HandleSignalException(IActorContext<T> context, ISignalTarget<T> target)
        {
            throw new NotImplementedException();
        }

        protected override ValueTask<Behavior<T>> HandleReceiveException(IActorContext<T> context, IReceiveTarget<T> target)
        {
            throw new NotImplementedException();
        }
    }

    internal sealed class StopSupervisor<T, TException> : SimpleSupervisor<T, TException>
        where T : class
        where TException : Exception
    {
        public StopSupervisor(Stop stop, Behavior<T> behavior) : base(stop)
        {
            throw new NotImplementedException();
        }
    }

    internal sealed class ResumeSupervisor<T, TException> : SimpleSupervisor<T, TException>
        where T : class
        where TException : Exception
    {
        public ResumeSupervisor(Resume resume, Behavior<T> behavior) : base(resume)
        {
            throw new NotImplementedException();
        }
    }

    internal sealed class RestartSupervisor<T, TException> : SimpleSupervisor<T, TException>
        where T : class
        where TException : Exception
    {
        public RestartSupervisor(Restart restart, Behavior<T> behavior) : base(restart)
        {
            throw new NotImplementedException();
        }
    }

    internal sealed class BackoffSupervisor<T, TException> : AbstractSupervisor<object, T, TException>
        where T : class
        where TException : Exception
    {
        #region messages

        public sealed class ScheduledRestart
        {
            public static readonly ScheduledRestart Instance = new ScheduledRestart();
            private ScheduledRestart()
            {
            }
        }

        public sealed class ResetRestartCount
        {
            public int Current { get; }

            public ResetRestartCount(int current)
            {
                Current = current;
            }
        }

        #endregion

        /// <summary>
        /// Calculates an exponential back off delay.
        /// </summary>
        private static TimeSpan CalculateDelay(int restartCount, TimeSpan min, TimeSpan max, double randomFactor)
        {
            var random = 1.0 + ThreadLocalRandom.Current.NextDouble() * randomFactor;
            if (restartCount > 30)
            {
                return max;
            }
            else
            {
                var ticks = Math.Min(max.Ticks, min.Ticks * Math.Pow(2, restartCount)) * random;
                return new TimeSpan(Math.Min(max.Ticks, (long)ticks));
            }
        }

        private readonly Behavior<T> behavior;
        private bool blackhole = false;
        private int restartCount = 0;

        public BackoffSupervisor(Backoff backoff, Behavior<T> behavior) : base(backoff)
        {
            this.behavior = behavior;
        }

        public ValueTask<Behavior<T>> AroundSignal<TSignalTarget>(IActorContext<T> context, ISignal signal, TSignalTarget target)
        {
            if (blackhole)
            {
                context.System.EventStream.Publish(new Dropped(signal, context.Self.UnsafeCast<Nothing>()));
                return new ValueTask<Behavior<T>>(Behaviors.Same<T>());
            }
            else
            {
                return target.Apply(context, signal);
            }
        }

        public override async ValueTask<Behavior<T>> AroundReceive<TReceiveTarget>(IActorContext<object> context, object message, TReceiveTarget target)
        {
            try
            {
            }
            catch (TException exception)
            {
                
            }
        }

        protected override ValueTask<Behavior<T>> HandleExceptionOnStart(IActorContext<object> context)
        {
            throw new NotImplementedException();
        }

        protected override ValueTask<Behavior<T>> HandleSignalException(IActorContext<object> context, ISignalTarget<T> target)
        {
            throw new NotImplementedException();
        }

        protected override ValueTask<Behavior<T>> HandleReceiveException(IActorContext<object> context, IReceiveTarget<T> target)
        {
            throw new NotImplementedException();
        }
    }
}