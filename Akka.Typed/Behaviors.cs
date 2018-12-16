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
using Akka.Dispatch.SysMsg;
using Akka.Typed.Internal;

namespace Akka.Typed
{
    public delegate ValueTask<Behavior<T>> Receive<T>(IActorContext<T> context, T message) where T : class;
    public delegate ValueTask<Behavior<T>> ReceiveMessage<T>(T message) where T : class;
    public delegate ValueTask<Behavior<T>> ReceiveSignal<T>(IActorContext<T> context, ISignal signal) where T : class;
    public delegate ValueTask<Behavior<T>> Defer<T>(IActorContext<T> context) where T : class;

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

        /// <summary>
        /// Construct an actor behavior that can react to both incoming messages and
        /// lifecycle signals. After spawning this actor from another actor (or as the
        /// guardian of an <see cref="ActorSystem{T}"/>) it will be executed within an
        /// <see cref="IActorContext{TMessage}"/> that allows access to the system, spawning and watching
        /// other actors, etc.
        /// 
        /// Compared to using <see cref="AbstractBehavior{T}"/> this factory is a more functional style
        /// of defining the <see cref="Behavior{TMessage}"/>. Processing the next message results in a new behavior
        /// that can potentially be different from this one. State is maintained by returning
        /// a new behavior that holds the new immutable state.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReceiveBehavior<T> Receive<T>(Receive<T> onMessage) where T : class =>
            new ReceiveBehavior<T>(onMessage);

        /// <summary>
        /// Simplified version of <see cref="Receive{T}"/> with only a single argument - the message
        /// to be handled. Useful for when the context is already accessible by other means,
        /// like being wrapped in an <see cref="Setup{T}"/> or similar.
        /// 
        /// Construct an actor behavior that can react to both incoming messages and
        /// lifecycle signals. After spawning this actor from another actor (or as the
        /// guardian of an <see cref="ActorSystem{T}"/>) it will be executed within an
        /// <see cref="IActorContext{TMessage}"/> that allows access to the system, spawning and watching
        /// other actors, etc.
        /// 
        /// Compared to using <see cref="AbstractBehavior{T}"/> this factory is a more functional style
        /// of defining the <see cref="Behavior{TMessage}"/>. Processing the next message results in a new behavior
        /// that can potentially be different from this one. State is maintained by returning
        /// a new behavior that holds the new immutable state.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReceiveMessageBehavior<T> ReceiveMessage<T>(ReceiveMessage<T> onMessage) where T : class =>
            new ReceiveMessageBehavior<T>(onMessage);

        /// <summary>
        /// Construct an actor <see cref="Behavior{TMessage}"/> that can react to lifecycle signals only.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="behavior"></param>
        /// <param name="onSignal"></param>
        /// <returns></returns>
        public static Behavior<T> ReceiveSignal<T>(ReceiveSignal<T> onSignal) where T : class =>
            new ReceiveMessageBehavior<T>(async _ => Same<T>()).ReceiveSignal(onSignal);

        /// <summary>
        /// Intercept messages and signals for a <see cref="Behavior{TMessage}"/> by first passing them to a <see cref="IBehaviorInterceptor{TOuter,TInner}"/>.
        /// 
        /// When a behavior returns a new behavior as a result of processing a signal or message and that behavior already contains
        /// the same interceptor, only the innermost interceptor is kept. This is to protect against stack overflow when recursively defining behaviors.
        /// </summary>
        public static Behavior<TOut> Intercept<TOut, TIn>(this Behavior<TIn> behavior, IBehaviorInterceptor<TOut, TIn> interceptor) where TIn : class where TOut : class
        {
            return new DeferredBehavior<TOut>(context =>
            {
                var i = new Interceptor<TOut, TIn>(interceptor, behavior);
                return i.PreStart(context);
            });
        }

        /// <summary>
        /// Behavior decorator that copies all received message to the designated
        /// monitor <see cref="IActorRef{T}"/> before invoking the wrapped behavior. The
        /// wrapped behavior can evolve (i.e. return different behavior) without needing to be
        /// wrapped in a `monitor` call again.
        /// </summary>
        public static Behavior<T> Monitor<T>(this Behavior<T> behavior, IActorRef<T> monitor) where T : class => throw new NotImplementedException();

        /// <summary>
        /// Wrap the given behavior with the given <see cref="SupervisorStrategy"/> for
        /// the given exception.
        /// Exceptions that are not subtypes of `Thr` will not be
        /// caught and thus lead to the termination of the actor.
        /// 
        /// It is possible to specify different supervisor strategies, such as restart,
        /// resume, backoff.
        /// 
        /// Example:
        /// <code>
        /// var dbConnector: Behavior[DbCommand] = ...
        /// 
        /// val dbRestarts =
        ///    Behaviors.supervise(dbConnector)
        ///      .onFailure(SupervisorStrategy.restart) // handle all NonFatal exceptions
        /// 
        /// val dbSpecificResumes =
        ///    Behaviors.supervise(dbConnector)
        ///      .onFailure[IndexOutOfBoundsException](SupervisorStrategy.resume) // resume for IndexOutOfBoundsException exceptions
        /// </code>
        /// </summary>
        public static Supervise<T> Supervise<T>(this Behavior<T> behavior) where T : class => new Supervise<T>(behavior);

        internal static async ValueTask<Behavior<T>> Unwrap<T>(Behavior<T> result, IActorContext<T> context) where T : class
        {
            while (result is DeferredBehavior<T> deferred)
            {
                result = await deferred.Deferred(context);
            }

            return result;
        }

        public static async ValueTask<Behavior<T>> Interpret<T>(Behavior<T> behavior, IActorContext<T> context, T message) 
            where T : class
        {
            if (behavior is ExtensibleBehavior<T> ext)
            {
                var result = await ext.Receive(context, message);
                return await Unwrap(result, context);
            }
            else if (behavior is StoppedBehavior<T> || behavior is FailedBehavior<T>) return behavior;
            else if (behavior is IgnoreBehavior<T>) return Same<T>();
            else if (behavior is EmptyBehavior<T>) return Unhandled<T>();
            else
            {
                Raises.InvalidArgument($"Cannot execute behavior of type [{behavior?.GetType()}] using interpreter");
                return default;
            }
        }

        public static async ValueTask<Behavior<T>> Interpret<T>(Behavior<T> behavior, IActorContext<T> context, ISignal signal)
            where T : class
        {
            if (behavior is ExtensibleBehavior<T> ext)
            {
                var result = await ext.ReceiveSignal(context, signal);

                if (signal is Terminated terminated && result == UnhandledBehavior<T>.Instance)
                {
                    Raises.DeathPact(terminated.ActorRef);
                    return result; // never reached
                }

                return await Unwrap(result, context);
            }
            else if (behavior is StoppedBehavior<T> || behavior is FailedBehavior<T>) return behavior;
            else if (behavior is IgnoreBehavior<T>) return Same<T>();
            else if (behavior is EmptyBehavior<T>) return Unhandled<T>();
            else
            {
                Raises.InvalidArgument($"Cannot execute behavior of type [{behavior?.GetType()}] using interpreter");
                return default;
            }
        }
    }

    public struct Supervise<T> where T : class 
    {
        private readonly Behavior<T> behavior;

        public Supervise(Behavior<T> behavior)
        {
            this.behavior = behavior;
        }

        public Behavior<T> OnFailure<TError>(SupervisorStrategy strategy) where TError: Exception
        {
        }
    }
}