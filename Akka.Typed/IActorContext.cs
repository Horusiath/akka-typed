#region copyright
// -----------------------------------------------------------------------
// <copyright file="IActorContext.cs" company="Bartosz Sypytkowski">
//     Copyright (C) 2018-2018 Bartosz Sypytkowski <b.sypytkowski@gmail.com>
// </copyright>
// -----------------------------------------------------------------------
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Akka.Typed
{
    /// <summary>
    /// An Actor is given by the combination of a [[Behavior]] and a context in
    /// which this behavior is executed. As per the Actor Model an Actor can perform
    /// the following actions when processing a message:
    /// 
    ///  - send a finite number of messages to other Actors it knows
    ///  - create a finite number of Actors
    ///  - designate the behavior for the next message
    /// 
    /// In Akka the first capability is accessed by using the `!` or `tell` method
    /// on an [[ActorRef]], the second is provided by [[ActorContext#spawn]]
    /// and the third is implicit in the signature of [[Behavior]] in that the next
    /// behavior is always returned from the message processing logic.
    /// 
    /// An `ActorContext` in addition provides access to the Actor’s own identity (“`self`”),
    /// the [[ActorSystem]] it is part of, methods for querying the list of child Actors it
    /// created, access to [[Terminated]] and timed message scheduling.
    /// 
    /// Not for user extension.
    /// </summary>
    public interface IActorContext<in TMessage> where TMessage: class
    {
        /// <summary>
        /// The identity of this Actor, bound to the lifecycle of this Actor instance.
        /// An Actor with the same name that lives before or after this instance will
        /// have a different [[ActorRef]].
        /// 
        /// This field is thread-safe and can be called from other threads than the ordinary
        /// actor message processing thread, such as [[scala.concurrent.Future]] callbacks.
        /// </summary>
        IActorRef<TMessage> Self { get; }

        /// <summary>
        /// The [[ActorSystem]] to which this Actor belongs.
        /// 
        /// This field is thread-safe and can be called from other threads than the ordinary
        /// actor message processing thread, such as [[scala.concurrent.Future]] callbacks.
        /// </summary>
        ActorSystem<Nothing> System { get; }

        /// <summary>
        /// An actor specific logger
        /// 
        /// *Warning*: This method is not thread-safe and must not be accessed from threads other
        /// than the ordinary actor message processing thread, such as [[scala.concurrent.Future]] callbacks.
        /// </summary>
        ILogger Log { get; }

        /// <summary>
        /// The list of child Actors created by this Actor during its lifetime that
        /// are still alive, in no particular order.
        /// 
        /// *Warning*: This method is not thread-safe and must not be accessed from threads other
        /// than the ordinary actor message processing thread, such as [[scala.concurrent.Future]] callbacks.
        /// </summary>
        IEnumerable<IActorRef<Nothing>> Children { get; }

        /// <summary>
        /// The named child Actor if it is alive.
        /// 
        /// *Warning*: This method is not thread-safe and must not be accessed from threads other
        /// than the ordinary actor message processing thread, such as [[scala.concurrent.Future]] callbacks.
        /// </summary>
        bool TryGetChild<T2>(string name, out IActorRef<T2> childRef) where T2 : class;

        /// <summary>
        /// Create a child Actor from the given [[akka.actor.typed.Behavior]] under a randomly chosen name.
        /// It is good practice to name Actors wherever practical.
        /// 
        /// *Warning*: This method is not thread-safe and must not be accessed from threads other
        /// than the ordinary actor message processing thread, such as [[scala.concurrent.Future]] callbacks.
        /// </summary>
        IActorRef<T2> SpawnAnonymous<T2>(Behavior<T2> behavior, Props props = null) where T2 : class;

        /// <summary>
        /// Create a child Actor from the given [[akka.actor.typed.Behavior]] and with the given name.
        /// 
        /// *Warning*: This method is not thread-safe and must not be accessed from threads other
        /// than the ordinary actor message processing thread, such as [[scala.concurrent.Future]] callbacks.
        /// </summary>
        IActorRef<T2> Spawn<T2>(Behavior<T2> behavior, string name, Props props = null) where T2 : class;

        /// <summary>
        /// Force the child Actor under the given name to terminate after it finishes
        /// processing its current message. Nothing happens if the ActorRef is a child that is already stopped.
        /// 
        /// *Warning*: This method is not thread-safe and must not be accessed from threads other
        /// than the ordinary actor message processing thread, such as [[scala.concurrent.Future]] callbacks.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if the given actor ref is not a direct child of this actor.</exception>
        void Stop<T2>(IActorRef<T2> child) where T2 : class;

        /// <summary>
        /// Register for [[akka.actor.typed.Terminated]] notification once the Actor identified by the
        /// given [[ActorRef]] terminates. This message is also sent when the watched actor
        /// is on a node that has been removed from the cluster when using akka-cluster
        /// or has been marked unreachable when using akka-remote directly
        /// 
        /// `watch` is idempotent if it is not mixed with `watchWith`.
        /// 
        /// It will fail with an [[IllegalStateException]] if the same subject was watched before using `watchWith`.
        /// To clear the termination message, unwatch first.
        /// 
        /// *Warning*: This method is not thread-safe and must not be accessed from threads other
        /// than the ordinary actor message processing thread, such as [[scala.concurrent.Future]] callbacks.
        /// </summary>
        void Watch<T2>(IActorRef<T2> other) where T2 : class;

        /// <summary>
        /// Register for termination notification with a custom message once the Actor identified by the
        /// given [[ActorRef]] terminates. This message is also sent when the watched actor
        /// is on a node that has been removed from the cluster when using akka-cluster
        /// or has been marked unreachable when using akka-remote directly.
        /// 
        /// `watchWith` is idempotent if it is called with the same `msg` and not mixed with `watch`.
        /// 
        /// It will fail with an [[IllegalStateException]] if the same subject was watched before using `watch` or `watchWith` with
        /// another termination message. To change the termination message, unwatch first.
        /// 
        /// *Warning*: This method is not thread-safe and must not be accessed from threads other
        /// than the ordinary actor message processing thread, such as [[scala.concurrent.Future]] callbacks.
        /// </summary>
        void WatchWith<T2>(IActorRef<T2> other, TMessage message) where T2 : class;

        /// <summary>
        /// Revoke the registration established by `watch`. A [[Terminated]]
        /// notification will not subsequently be received for the referenced Actor.
        /// 
        /// *Warning*: This method is not thread-safe and must not be accessed from threads other
        /// than the ordinary actor message processing thread, such as [[scala.concurrent.Future]] callbacks.
        /// </summary>
        void Unwatch<T2>(IActorRef<T2> other) where T2 : class;

        /// <summary>
        /// Schedule the sending of a notification in case no other
        /// message is received during the given period of time. The timeout starts anew
        /// with each received message. Use `cancelReceiveTimeout` to switch off this
        /// mechanism.
        /// 
        /// *Warning*: This method is not thread-safe and must not be accessed from threads other
        /// than the ordinary actor message processing thread, such as [[scala.concurrent.Future]] callbacks.
        /// </summary>
        void SetReceiveTimeout(TimeSpan timeout, TMessage message);

        /// <summary>
        /// Cancel the sending of receive timeout notifications.
        /// 
        /// *Warning*: This method is not thread-safe and must not be accessed from threads other
        /// than the ordinary actor message processing thread, such as [[scala.concurrent.Future]] callbacks.
        /// </summary>
        void CancelReceiveTimeout();

        /// <summary>
        /// Schedule the sending of the given message to the given target Actor after
        /// the given time period has elapsed. The scheduled action can be cancelled
        /// by invoking [[akka.actor.Cancellable#cancel]] on the returned
        /// handle.
        /// 
        /// This method is thread-safe and can be called from other threads than the ordinary
        /// actor message processing thread, such as [[scala.concurrent.Future]] callbacks.
        /// </summary>
        IDisposable ScheduleOnce<T2>(TimeSpan delay, IActorRef<T2> target, T2 message) where T2 : class;

        /// <summary>
        /// This Actor’s execution context. It can be used to run asynchronous tasks
        /// like [[scala.concurrent.Future]] operators.
        /// 
        /// This field is thread-safe and can be called from other threads than the ordinary
        /// actor message processing thread, such as [[scala.concurrent.Future]] callbacks.
        /// </summary>
        TaskScheduler TaskScheduler { get; }

        /// <summary>
        /// Create a message adapter that will convert or wrap messages such that other Actor’s
        /// protocols can be ingested by this Actor.
        /// 
        /// You can register several message adapters for different message classes.
        /// It's only possible to have one message adapter per message class to make sure
        /// that the number of adapters are not growing unbounded if registered repeatedly.
        /// That also means that a registered adapter will replace an existing adapter for
        /// the same message class.
        /// 
        /// A message adapter will be used if the message class matches the given class or
        /// is a subclass thereof. The registered adapters are tried in reverse order of
        /// their registration order, i.e. the last registered first.
        /// 
        /// A message adapter (and the returned `ActorRef`) has the same lifecycle as
        /// this actor. It's recommended to register the adapters in a top level
        /// `Behaviors.setup` or constructor of `AbstractBehavior` but it's possible to
        /// register them later also if needed. Message adapters don't have to be stopped since
        /// they consume no resources other than an entry in an internal `Map` and the number
        /// of adapters are bounded since it's only possible to have one per message class.
        /// *
        /// The function is running in this actor and can safely access state of it.
        /// 
        /// *Warning*: This method is not thread-safe and must not be accessed from threads other
        /// than the ordinary actor message processing thread, such as [[scala.concurrent.Future]] callbacks.
        /// </summary>
        IActorRef<TSource> MessageAdapter<TSource>(Func<TSource, TMessage> adapter) where TSource : class;

        /// <summary>
        /// Perform a single request-response message interaction with another actor, and transform the messages back to
        /// the protocol of this actor.
        /// 
        /// The interaction has a timeout (to avoid a resource leak). If the timeout hits without any response it
        /// will be passed as a `Failure(`[[java.util.concurrent.TimeoutException]]`)` to the `mapResponse` function
        /// (this is the only "normal" way a `Failure` is passed to the function).
        /// 
        /// For other messaging patterns with other actors, see [[ActorContext#messageAdapter]].
        /// 
        /// This method is thread-safe and can be called from other threads than the ordinary
        /// actor message processing thread, such as [[scala.concurrent.Future]] callbacks.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="createRequest">A function that creates a message for the other actor,
        /// containing the provided `ActorRef[Res]` that the other actor can send a message back through.</param>
        ValueTask<TResult> Ask<TRequest, TResult>(IRecipientRef<TRequest> target, CancellationToken cancellationToken, Func<IActorRef<TResult>, TRequest> createRequest) where TResult : class;
    }
}