#region copyright
// -----------------------------------------------------------------------
// <copyright file="SupervisionStrategy.cs" company="Bartosz Sypytkowski">
//     Copyright (C) 2018-2018 Bartosz Sypytkowski <b.sypytkowski@gmail.com>
// </copyright>
// -----------------------------------------------------------------------
#endregion

using System;
using System.Runtime.CompilerServices;

namespace Akka.Typed
{
    public abstract class SupervisionStrategy
    {
        #region statics

        /// <summary>
        /// Resume means keeping the same state as before the exception was
        /// thrown and is thus less safe than `restart`.
        /// 
        /// If the actor behavior is deferred and throws an exception on startup the actor is stopped
        /// (restarting would be dangerous as it could lead to an infinite restart-loop)
        /// </summary>
        public static readonly SupervisionStrategy Resume = new Resume(isLoggingEnabled: true);

        /// <summary>
        /// Restart immediately without any limit on number of restart retries.
        /// 
        /// If the actor behavior is deferred and throws an exception on startup the actor is stopped
        /// (restarting would be dangerous as it could lead to an infinite restart-loop)
        /// </summary>
        public static readonly SupervisionStrategy Restart = new Restart(-1, TimeSpan.Zero, isLoggingEnabled: true);

        /// <summary>
        /// Stop the actor.
        /// </summary>
        public static readonly SupervisionStrategy Stop = new Stop(isLoggingEnabled: true);

        /// <summary>
        /// Restart with a limit of number of restart retries.
        /// The number of restarts are limited to a number of restart attempts (`maxNrOfRetries`)
        /// within a time range (`withinTimeRange`). When the time window has elapsed without reaching
        /// `maxNrOfRetries` the restart count is reset.
        /// 
        /// The strategy is applied also if the actor behavior is deferred and throws an exception during
        /// startup.
        /// </summary>
        /// <param name="maxRetries">the number of times a child actor is allowed to be restarted,
        ///   if the limit is exceeded the child actor is stopped</param>
        /// <param name="within">duration of the time window for maxRetries</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SupervisionStrategy RestartWithLimit(int maxRetries, TimeSpan within) =>
            new Restart(maxRetries, within, isLoggingEnabled: true);

        /// <summary>
        /// It supports exponential back-off between the given `minBackoff` and
        /// `maxBackoff` durations. For example, if `minBackoff` is 3 seconds and
        /// `maxBackoff` 30 seconds the start attempts will be delayed with
        /// 3, 6, 12, 24, 30, 30 seconds. The exponential back-off counter is reset
        /// if the actor is not terminated within the `minBackoff` duration.
        /// 
        /// In addition to the calculated exponential back-off an additional
        /// random delay based the given `randomFactor` is added, e.g. 0.2 adds up to 20%
        /// delay. The reason for adding a random delay is to avoid that all failing
        /// actors hit the backend resource at the same time.
        /// 
        /// During the back-off incoming messages are dropped.
        /// 
        /// If no new exception occurs within the `minBackoff` duration the exponentially
        /// increased back-off timeout is reset.
        /// 
        /// The strategy is applied also if the actor behavior is deferred and throws an exception during
        /// startup.
        /// 
        /// A maximum number of restarts can be specified with [[Backoff#withMaxRestarts]]
        /// </summary>
        /// <param name="minBackoff">minimum (initial) duration until the child actor will
        ///   started again, if it is terminated</param>
        /// <param name="maxBackoff">the exponential back-off is capped to this duration</param>
        /// <param name="randomFactor">after calculation of the exponential back-off an additional
        ///   random delay based on this factor is added, e.g. `0.2` adds up to `20%` delay.
        ///   In order to skip this additional delay pass in `0`.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BackoffSupervisorStrategy RestartWithBackoff(TimeSpan minBackoff, TimeSpan maxBackoff, double randomFactor) =>
            new Backoff(minBackoff, maxBackoff, randomFactor, resetBackoffAfter: minBackoff, isLoggingEnabled: true, maxRestart: -1);

        #endregion

        public bool IsLoggingEnabled { get; }

        protected SupervisionStrategy(bool isLoggingEnabled)
        {
            IsLoggingEnabled = isLoggingEnabled;
        }

        public abstract SupervisionStrategy WithLoggingEnabled(bool enabled);
    }

    public abstract class BackoffSupervisorStrategy : SupervisionStrategy
    {
        public TimeSpan ResetBackoffAfter { get; }

        /// <summary>
        /// The back-off algorithm is reset if the actor does not crash within the
        /// specified `resetBackoffAfter`. By default, the `resetBackoffAfter` has
        /// the same value as `minBackoff`.
        /// </summary>
        public abstract BackoffSupervisorStrategy WithResetBackoffAfter(TimeSpan timeout);

        /// <summary>
        /// Allow at most this number of failed restarts in a row. Zero or negative disables
        /// the upper limit on restarts (and is the default)
        /// </summary>
        public abstract BackoffSupervisorStrategy WithMaxRestarts(int maxRestarts);

        protected BackoffSupervisorStrategy(bool isLoggingEnabled, TimeSpan resetBackoffAfter) : base(isLoggingEnabled)
        {
            ResetBackoffAfter = resetBackoffAfter;
        }
    }

    internal sealed class Resume : SupervisionStrategy
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Resume(bool isLoggingEnabled) : base(isLoggingEnabled)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override SupervisionStrategy WithLoggingEnabled(bool enabled) =>
            new Resume(enabled);
    }

    internal sealed class Stop : SupervisionStrategy
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Stop(bool isLoggingEnabled) : base(isLoggingEnabled)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override SupervisionStrategy WithLoggingEnabled(bool enabled) =>
            new Stop(enabled);
    }

    internal sealed class Restart : SupervisionStrategy
    {
        public int MaxRetries { get; }
        public TimeSpan Within { get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Restart(int maxRetries, TimeSpan within, bool isLoggingEnabled) : base(isLoggingEnabled)
        {
            MaxRetries = maxRetries;
            Within = within;
        }

        public bool HasUnlimitedRestarts => MaxRetries == -1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override SupervisionStrategy WithLoggingEnabled(bool enabled) =>
            new Restart(MaxRetries, Within, enabled);
    }

    internal sealed class Backoff : BackoffSupervisorStrategy
    {
        public TimeSpan MinBackoff { get; }
        public TimeSpan MaxBackoff { get; }
        public double RandomFactor { get; }
        public int MaxRestart { get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Backoff(TimeSpan minBackoff, TimeSpan maxBackoff, double randomFactor, TimeSpan resetBackoffAfter, bool isLoggingEnabled, int maxRestart) 
            : base(isLoggingEnabled, resetBackoffAfter)
        {
            MinBackoff = minBackoff;
            MaxBackoff = maxBackoff;
            RandomFactor = randomFactor;
            MaxRestart = maxRestart;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override SupervisionStrategy WithLoggingEnabled(bool enabled) =>
            new Backoff(MinBackoff, MaxBackoff, RandomFactor, ResetBackoffAfter, enabled, MaxRestart);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override BackoffSupervisorStrategy WithResetBackoffAfter(TimeSpan timeout) =>
            new Backoff(MinBackoff, MaxBackoff, RandomFactor, timeout, IsLoggingEnabled, MaxRestart);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override BackoffSupervisorStrategy WithMaxRestarts(int maxRestarts) =>
            new Backoff(MinBackoff, MaxBackoff, RandomFactor, ResetBackoffAfter, IsLoggingEnabled, maxRestarts);
    }
}