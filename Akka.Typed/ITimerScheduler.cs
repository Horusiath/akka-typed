#region copyright
// -----------------------------------------------------------------------
// <copyright file="ITimerScheduler.cs" company="Bartosz Sypytkowski">
//     Copyright (C) 2018-2018 Bartosz Sypytkowski <b.sypytkowski@gmail.com>
// </copyright>
// -----------------------------------------------------------------------
#endregion

using System;

namespace Akka.Typed
{
    /// <summary>
    /// Support for scheduled `self` messages in an actor.
    /// It is used with `Behaviors.withTimers`.
    /// Timers are bound to the lifecycle of the actor that owns it,
    /// and thus are cancelled automatically when it is restarted or stopped.
    /// 
    /// `TimerScheduler` is not thread-safe, i.e. it must only be used within
    /// the actor that owns it.
    /// </summary>
    public interface ITimerScheduler<T>
    {
        /// <summary>
        /// Start a periodic timer that will send `msg` to the `self` actor at
        /// a fixed `interval`.
        /// 
        /// Each timer has a key and if a new timer with same key is started
        /// the previous is cancelled and it's guaranteed that a message from the
        /// previous timer is not received, even though it might already be enqueued
        /// in the mailbox when the new timer is started.
        /// </summary>
        void StartPeriodicTimer(object key, T message, TimeSpan interval);

        /// <summary>
        /// Start a timer that will send `msg` once to the `self` actor after
        /// the given `timeout`.
        /// 
        /// Each timer has a key and if a new timer with same key is started
        /// the previous is cancelled and it's guaranteed that a message from the
        /// previous timer is not received, even though it might already be enqueued
        /// in the mailbox when the new timer is started.
        /// </summary>
        void StartSingleTimer(object key, T message, TimeSpan timeout);

        /// <summary>
        /// Check if a timer with a given `key` is active.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool IsTimerActive(object key);

        /// <summary>
        /// Cancel a timer with a given `key`.
        /// If canceling a timer that was already canceled, or key never was used to start a timer
        /// this operation will do nothing.
        /// 
        /// It is guaranteed that a message from a canceled timer, including its previous incarnation
        /// for the same key, will not be received by the actor, even though the message might already
        /// be enqueued in the mailbox when cancel is called.
        /// </summary>
        void Cancel(object key);

        /// <summary>
        /// Cancel all timers.
        /// </summary>
        void CancelAll();
    }
}