#region copyright
// -----------------------------------------------------------------------
// <copyright file="TimerScheduler.cs" company="Bartosz Sypytkowski">
//     Copyright (C) 2018-2018 Bartosz Sypytkowski <b.sypytkowski@gmail.com>
// </copyright>
// -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Akka.Typed.Internal
{
    internal interface ITimerMessage
    {
        object Key { get; }
        int Generation { get; }
        object Owner { get; }
    }

    internal readonly struct Timer<T> where T: class
    {
        public Timer(object key, int generation, bool repeat, ICancelable task, T message)
        {
            Key = key;
            Generation = generation;
            Repeat = repeat;
            Task = task;
            Message = message;
        }

        public object Key { get; }
        public T Message { get; }
        public ICancelable Task { get; }
        public int Generation { get; }
        public bool Repeat { get; }
    }

    internal sealed class InfluenceReceiveTimeoutTimerMessage : ITimerMessage
    {
        public InfluenceReceiveTimeoutTimerMessage(object key, object owner, int generation)
        {
            Key = key;
            Owner = owner;
            Generation = generation;
        }

        public object Key { get; }
        public object Owner { get; }
        public int Generation { get; }
    }

    internal sealed class NotInfluenceReceiveTimeoutTimerMessage : ITimerMessage, Akka.Actor.INotInfluenceReceiveTimeout
    {
        public NotInfluenceReceiveTimeoutTimerMessage(object key, object owner, int generation)
        {
            Key = key;
            Owner = owner;
            Generation = generation;
        }

        public object Key { get; }
        public object Owner { get; }
        public int Generation { get; }
    }
    
    internal sealed class TimerScheduler<TMessage> : ITimerScheduler<TMessage>
        where TMessage : class
    {
        private readonly IActorContext<TMessage> _context;
        private ImmutableDictionary<object, Timer<TMessage>> _timers = ImmutableDictionary<object, Timer<TMessage>>.Empty;
        private int _timerGenerator = 1;

        public TimerScheduler(IActorContext<TMessage> context)
        {
            _context = context;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StartPeriodicTimer(object key, TMessage message, TimeSpan interval) =>
            StartTimer(key, message, interval, repeat: true);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StartSingleTimer(object key, TMessage message, TimeSpan timeout) => 
            StartTimer(key, message, timeout, repeat: false);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsTimerActive(object key) => _timers.ContainsKey(key);

        public void Cancel(object key)
        {
            if (_timers.TryGetValue(key, out var timer))
            {
                CancelTimer(timer);
            }
        }

        public void CancelAll()
        {
            _context.Log.LogDebug("Cancel all timers");
            foreach (var entry in _timers)
            {
                entry.Value.Task.Cancel();
            }
            _timers = ImmutableDictionary<object, Timer<TMessage>>.Empty;
        }

        private void StartTimer(object key, TMessage message, TimeSpan timeout, bool repeat)
        {
            Cancel(key);
            var next = _timerGenerator++;
            var timerMsg = message is Akka.Actor.INotInfluenceReceiveTimeout
                ? (ITimerMessage) new NotInfluenceReceiveTimeoutTimerMessage(key, this, next)
                : new InfluenceReceiveTimeoutTimerMessage(key, this, next);

            var task = repeat
                ? _context.System.Scheduler.ScheduleRepeatedlyCancelable(timeout, timeout,
                    () => _context.Self.Tell(message))
                : _context.System.Scheduler.ScheduleOnceCancelable(timeout,
                    () => _context.Self.Tell(message));
            var nextTimer = new Timer<TMessage>(key, next, repeat, task, message);
            _context.Log.LogDebug("Start timer {0} with generation {1}", key, next);
            _timers = _timers.SetItem(key, nextTimer);
        }

        private void CancelTimer(in Timer<TMessage> timer)
        {
            _context.Log.LogDebug("Cancel timer {0} with generation {1}", timer.Key, timer.Generation);
            timer.Task.Cancel();
            _timers = _timers.Remove(timer.Key);
        }

        [CanBeNull]
        public TMessage InterceptTimerMessage(IActorContext<ITimerMessage> context, ITimerMessage message)
        {
            var key = message.Key;
            if (_timers.TryGetValue(key, out var timer))
            {
                if (!ReferenceEquals(message.Owner, this))
                {
                    // after restart, it was from an old instance that was enqueued in mailbox before canceled
                    _context.Log.LogDebug("Received timer [{0}] from old restarted instance, discarding", key);
                    return default;
                }
                else if (message.Generation == timer.Generation)
                {
                    // valid timer
                    if (!timer.Repeat)
                        _timers = _timers.Remove(key);
                    return timer.Message;
                }
                else
                {
                    // it was from an old timer that was enqueued in mailbox before canceled
                    _context.Log.LogDebug("Received timer [{0}] from from old generation [{1}], expected generation [{2}], discarding", key, message.Generation, timer.Generation);
                    return default;
                }
            }
            else
            {
                // it was from canceled timer that was already enqueued in mailbox
                _context.Log.LogDebug("Received timer [{0}] that has been removed, discarding", key);
                return default;
            }
        }


    }

    internal sealed class TimerInterceptor<T> : IBehaviorInterceptor<object, T> where T : class
    {
        private readonly TimerScheduler<T> _timerScheduler;

        public TimerInterceptor(TimerScheduler<T> timerScheduler)
        {
            _timerScheduler = timerScheduler;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask<Behavior<T>> AroundStart<TPreStartTarget>(IActorContext<object> context, TPreStartTarget target) where TPreStartTarget : IPreStartTarget<T>
        {
            return target.Start(context);
        }

        public ValueTask<Behavior<T>> AroundReceive<TReceiveTarget>(IActorContext<object> context, object message, TReceiveTarget target) where TReceiveTarget : IReceiveTarget<T>
        {
            var intercepted = message is ITimerMessage timerMesage
                ? _timerScheduler.InterceptTimerMessage(context, timerMesage)
                : (T) message;

            return ReferenceEquals(intercepted, null) 
                ? new ValueTask<Behavior<T>>(Behaviors.Same<T>()) 
                : target.Apply(context, intercepted);
        }

        public ValueTask<Behavior<T>> AroundReceive<TSignalTarget>(IActorContext<object> context, ISignal signal, TSignalTarget target) where TSignalTarget : ISignalTarget<T>
        {
            if (signal is PreStart || signal is PostStop)
            {
                _timerScheduler.CancelAll();
            }

            return target.Apply(context, signal);
        }
    }
}