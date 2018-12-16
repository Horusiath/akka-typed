#region copyright
// -----------------------------------------------------------------------
//  <copyright file="Interceptor.cs" company="Akka.NET Project">
//      Copyright (C) 2009-2018 Lightbend Inc. <http://www.lightbend.com>
//      Copyright (C) 2013-2018 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------
#endregion

using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Akka.Typed.Internal
{
    public sealed class Interceptor<TOut, TIn> : ExtensibleBehavior<TOut> 
        where TOut : class 
        where TIn : class
    {
        #region targets

        private readonly struct PreStartTarget : IPreStartTarget<TIn>
        {
            private readonly Behavior<TIn> _behavior;

            public PreStartTarget(Behavior<TIn> behavior)
            {
                _behavior = behavior;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ValueTask<Behavior<TIn>> Start<T2>(IActorContext<T2> context) where T2 : class => 
                Behaviors.Unwrap(_behavior, (IActorContext<TIn>)context);
        }

        private readonly struct ReceiveTarget : IReceiveTarget<TIn>
        {
            private readonly Behavior<TIn> _behavior;

            public ReceiveTarget(Behavior<TIn> behavior)
            {
                _behavior = behavior;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ValueTask<Behavior<TIn>> Apply<T2>(IActorContext<T2> context, TIn message) where T2 : class => 
                Behaviors.Interpret(_behavior, (IActorContext<TIn>)context, message);

            public async ValueTask SignalRestart<T2>(IActorContext<T2> context) where T2 : class =>
                await Behaviors.Interpret(_behavior, (IActorContext<TIn>)context, PreRestart.Instance);
        }

        private readonly struct SignalTarget : ISignalTarget<TIn>
        {
            private readonly Behavior<TIn> _behavior;

            public SignalTarget(Behavior<TIn> behavior)
            {
                _behavior = behavior;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ValueTask<Behavior<TIn>> Apply<T2>(IActorContext<T2> context, ISignal signal) where T2 : class =>
                Behaviors.Interpret(_behavior, (IActorContext<TIn>) context, signal);
        }

        #endregion

        private readonly IBehaviorInterceptor<TOut, TIn> _interceptor;
        private readonly Behavior<TIn> _behavior;

        public Interceptor(IBehaviorInterceptor<TOut, TIn> interceptor, Behavior<TIn> behavior)
        {
            _interceptor = interceptor;
            _behavior = behavior;
        }

        public override Behavior<TNarrowed> Narrow<TNarrowed>()
        {
            throw new System.NotImplementedException();
        }

        public override async ValueTask<Behavior<TOut>> Receive(IActorContext<TOut> context, TOut message)
        {
            var interceptedResult = await _interceptor.AroundReceive(context, message, new ReceiveTarget(_behavior));
            return Deduplicate(interceptedResult, context);
        }

        public override async ValueTask<Behavior<TOut>> ReceiveSignal(IActorContext<TOut> context, ISignal signal)
        {
            var interceptedResult = await _interceptor.AroundSignal(context, signal, new SignalTarget(_behavior));
            return Deduplicate(interceptedResult, context);
        }

        public async ValueTask<Behavior<TOut>> PreStart(IActorContext<TOut> context)
        {
            var started = await _interceptor.AroundStart(context, new PreStartTarget(_behavior));
            return Deduplicate(started, context);
        }

        private Behavior<TOut> Deduplicate(Behavior<TIn> interceptedResult, IActorContext<TOut> context)
        {
            throw new System.NotImplementedException();
        }
    }
}