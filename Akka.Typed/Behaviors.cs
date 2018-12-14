#region copyright
// -----------------------------------------------------------------------
// <copyright file="Behaviors.cs" company="Bartosz Sypytkowski">
//     Copyright (C) 2018-2018 Bartosz Sypytkowski <b.sypytkowski@gmail.com>
// </copyright>
// -----------------------------------------------------------------------
#endregion

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Akka.Typed
{
    public delegate ValueTask<Behavior<T>> Receive<T>([NotNull]IActorContext<T> context, [NotNull]T message) where T : class;
    public delegate ValueTask<Behavior<T>> ReceiveMessage<T>([NotNull]T message) where T : class;
    public delegate ValueTask<Behavior<T>> ReceiveSignal<T>([NotNull]IActorContext<T> context, [NotNull]ISignal signal) where T : class;
    public delegate ValueTask<Behavior<T>> Defer<T>([NotNull]IActorContext<T> context) where T : class;

    /// <summary>
    /// Factories for <see cref="Behavior{T}"/>.
    /// </summary>
    public static class Behaviors
    {
        /// <summary>
        /// <see cref="Setup{T}"/> is a factory for a behavior. Creation of the behavior instance is deferred until
        /// the actor is started, as opposed to <see cref="Receive{T}"/> that creates the behavior instance
        /// immediately before the actor is running. The `factory` function pass the `ActorContext`
        /// as parameter and that can for example be used for spawning child actors.
        /// 
        /// <see cref="Setup{T}"/> is typically used as the outer most behavior when spawning an actor, but it
        /// can also be returned as the next behavior when processing a message or signal. In that
        /// case it will be started immediately after it is returned, i.e. next message will be
        /// processed by the started behavior.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Behavior<T> Setup<T>(Defer<T> setup) where T : class => new DeferredBehavior<T>(setup);

        /// <summary>
        /// Return this behavior from message processing in order to advise the
        /// system to reuse the previous behavior. This is provided in order to
        /// avoid the allocation overhead of recreating the current behavior where
        /// that is not necessary.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Behavior<T> Same<T>() where T : class => SameBehavior<T>.Instance;

        /// <summary>
        /// Return this behavior from message processing in order to advise the
        /// system to reuse the previous behavior, including the hint that the
        /// message has not been handled. This hint may be used by composite
        /// behaviors that delegate (partial) handling to other behaviors.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Behavior<T> Unhandled<T>() where T : class => UnhandledBehavior<T>.Instance;

        /// <summary>
        /// Return this behavior from message processing to signal that this actor
        /// shall terminate voluntarily. If this actor has created child actors then
        /// these will be stopped as part of the shutdown procedure.
        /// 
        /// The PostStop signal that results from stopping this actor will be passed to the
        /// current behavior. All other messages and signals will effectively be
        /// ignored.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Behavior<T> Stopped<T>() where T : class => StoppedBehavior<T>.Default;

        /// <summary>
        /// Return this behavior from message processing to signal that this actor
        /// shall terminate voluntarily. If this actor has created child actors then
        /// these will be stopped as part of the shutdown procedure.
        /// 
        /// The PostStop signal that results from stopping this actor will be passed to the
        /// current behavior. All other messages and signals will effectively be
        /// ignored.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Behavior<T> Stopped<T>(Behavior<T> postStop) where T : class => new StoppedBehavior<T>(postStop);

        /// <summary>
        /// A behavior that treats every incoming message as unhandled.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Behavior<T> Empty<T>() where T : class => EmptyBehavior<T>.Instance;

        /// <summary>
        /// A behavior that ignores every incoming message and returns “same”.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Behavior<T> Ignore<T>() where T : class => IgnoreBehavior<T>.Instance;
        
    }

}