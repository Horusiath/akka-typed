#region copyright
// -----------------------------------------------------------------------
// <copyright file="Behavior.cs" company="Bartosz Sypytkowski">
//     Copyright (C) 2018-2018 Bartosz Sypytkowski <b.sypytkowski@gmail.com>
// </copyright>
// -----------------------------------------------------------------------
#endregion

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Akka.Typed.Internal;
using JetBrains.Annotations;

namespace Akka.Typed
{
    public abstract class Behavior<TMessage> where TMessage: class
    {
        #region statics
        
        internal static readonly ReceiveSignal<TMessage> UnhandledSignal = (context, signal) => 
            new ValueTask<Behavior<TMessage>>(UnhandledBehavior<TMessage>.Instance);

        #endregion
        
        /// <summary>
        /// Narrow the type of this Behavior, which is always a safe operation. This
        /// method is necessary to implement the contravariant nature of Behavior
        /// (which cannot be expressed directly due to type inference problems).
        /// </summary>
        public abstract Behavior<TNarrowed> Narrow<TNarrowed>() where TNarrowed : class, TMessage;

        /// <summary>
        /// Composes this `Behavior with a fallback `Behavior` which
        /// is used when this `Behavior` doesn't handle the message or signal, i.e.
        /// when `unhandled` is returned.
        /// </summary>
        /// <param name="other">Fallback behavior, executed when this behavior will return <see cref="Behaviors.Unhandled{T}"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Behavior<TMessage> OrElse(Behavior<TMessage> other) => new OrElseBehavior<TMessage>(this, other);
    }

    /// <summary>
    /// Extension point for implementing custom behaviors in addition to the existing
    /// set of behaviors available through the DSLs in <see cref="Behaviors"/>.
    /// 
    /// Note that behaviors that keep an inner behavior, and intercepts messages for it should not be implemented as
    /// an extensible behavior but should instead use the [[BehaviorInterceptor]]
    /// </summary>
    public abstract class ExtensibleBehavior<TMessage> : Behavior<TMessage> where TMessage : class
    {
        /// <summary>
        /// Process an incoming message and return the next behavior.
        /// 
        /// The returned behavior can in addition to normal behaviors be one of the
        /// canned special objects:
        /// 
        ///  * returning `stopped` will terminate this Behavior
        ///  * returning `same` designates to reuse the current Behavior
        ///  * returning `unhandled` keeps the same Behavior and signals that the message was not yet handled
        /// 
        /// Code calling this method should use [[Behavior$]] `canonicalize` to replace
        /// the special objects with real Behaviors.
        /// </summary>
        public abstract ValueTask<Behavior<TMessage>> Receive(IActorContext<TMessage> context, TMessage message);

        /// <summary>
        /// Process an incoming <see cref="ISignal"/> and return the next behavior. This means
        /// that all lifecycle hooks, ReceiveTimeout, Terminated and Failed messages
        /// can initiate a behavior change.
        /// 
        /// The returned behavior can in addition to normal behaviors be one of the
        /// canned special objects:
        /// 
        ///  * returning `stopped` will terminate this Behavior
        ///  * returning `same` designates to reuse the current Behavior
        ///  * returning `unhandled` keeps the same Behavior and signals that the message was not yet handled
        /// 
        /// Code calling this method should use [[Behavior$]] `canonicalize` to replace
        /// the special objects with real Behaviors.
        /// </summary>
        public abstract ValueTask<Behavior<TMessage>> ReceiveSignal(IActorContext<TMessage> context, ISignal signal);
    }

    public static class BehaviorDecorators
    {
        /// <summary>
        /// Widen the wrapped Behavior by placing a funnel in front of it: the supplied
        /// PartialFunction decides which message to pull in (those that it is defined
        /// at) and may transform the incoming message to place them into the wrapped
        /// Behavior’s type hierarchy. Signals are not transformed.
        /// 
        /// Example:
        /// {{{
        /// receive[String] { (ctx, msg) => println(msg); same }.widen[Number] {
        ///   case b: BigDecimal => s"BigDecimal(&dollar;b)"
        ///   case i: BigInteger => s"BigInteger(&dollar;i)"
        ///   // all other kinds of Number will be `unhandled`
        /// }
        /// }}}
        /// 
        /// Scheduled messages via [[akka.actor.typed.scaladsl.TimerScheduler]] can currently
        /// not be used together with `widen`, see issue #25318.
        /// </summary>
        public static Behavior<TWiden> Widen<TNarrow, TWiden>(this Behavior<TNarrow> behavior) 
            where TWiden : class 
            where TNarrow : class =>
            throw new NotImplementedException();
    }

    internal sealed class EmptyBehavior<T> : Behavior<T> where T : class
    {
        public static readonly Behavior<T> Instance = new EmptyBehavior<T>();
        private EmptyBehavior() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override Behavior<TNarrowed> Narrow<TNarrowed>() => EmptyBehavior<TNarrowed>.Instance;
    }
    
    internal sealed class IgnoreBehavior<T> : Behavior<T> where T : class
    {
        public static readonly Behavior<T> Instance = new IgnoreBehavior<T>();
        private IgnoreBehavior() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override Behavior<TNarrowed> Narrow<TNarrowed>() => IgnoreBehavior<TNarrowed>.Instance;
    }

    internal sealed class UnhandledBehavior<T> : Behavior<T> where T : class
    {
        public static readonly Behavior<T> Instance = new UnhandledBehavior<T>();
        private UnhandledBehavior() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override Behavior<TNarrowed> Narrow<TNarrowed>() => UnhandledBehavior<TNarrowed>.Instance;
    }

    internal sealed class SameBehavior<T> : Behavior<T> where T : class
    {
        public static readonly Behavior<T> Instance = new SameBehavior<T>();
        private SameBehavior() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override Behavior<TNarrowed> Narrow<TNarrowed>() => SameBehavior<TNarrowed>.Instance;
    }

    internal sealed class DeferredBehavior<T> : Behavior<T> where T : class
    {
        internal readonly Defer<T> Deferred;
        internal DeferredBehavior(Defer<T> deferred)
        {
            Deferred = deferred;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override Behavior<TNarrowed> Narrow<TNarrowed>() =>
            throw new NotImplementedException();
    }

    internal sealed class StoppedBehavior<T> : Behavior<T> where T : class
    {
        public static readonly Behavior<T> Default = new StoppedBehavior<T>(null);

        [CanBeNull] internal readonly Behavior<T> postStop;

        public StoppedBehavior([CanBeNull] Behavior<T> postStop)
        {
            if (postStop is DeferredBehavior<T>)
                ThrowHelpers.ArgumentException("Behavior used as `postStop` behavior in Stopped(...) was a deferred one, which is not supported (it would never be evaluated).");

            this.postStop = postStop;
        }

        public override Behavior<TNarrowed> Narrow<TNarrowed>()
        {
            throw new NotImplementedException();
        }
    }
}