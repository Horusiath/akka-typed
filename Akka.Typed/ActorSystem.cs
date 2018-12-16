#region copyright
// -----------------------------------------------------------------------
// <copyright file="ActorSystem.cs" company="Bartosz Sypytkowski">
//     Copyright (C) 2018-2018 Bartosz Sypytkowski <b.sypytkowski@gmail.com>
// </copyright>
// -----------------------------------------------------------------------
#endregion

using System;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Typed.Internal.Receptionist;
using Microsoft.Extensions.Logging;

namespace Akka.Typed
{
    /// <summary>
    /// An ActorSystem is home to a hierarchy of Actors. It is created using
    /// [[ActorSystem#apply]] from a [[Behavior]] object that describes the root
    /// Actor of this hierarchy and which will create all other Actors beneath it.
    /// A system also implements the [[ActorRef]] type, and sending a message to
    /// the system directs that message to the root Actor.
    /// 
    /// Not for user extension.
    /// </summary>
    public abstract class ActorSystem<TMessage> : IActorRef<TMessage>, IAsyncDisposable where TMessage : class
    {
        /// <summary>
        /// The name of this actor system, used to distinguish multiple ones within the same process.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// A [[akka.actor.typed.Logger]] that can be used to emit log messages
        /// without specifying a more detailed source. Typically it is desirable to
        /// use the dedicated `Logger` available from each Actor’s [[ActorContext]]
        /// as that ties the log entries to the actor.
        /// </summary>
        public ILogger Log { get; }

        /// <summary>
        /// Up-time of this actor system in seconds.
        /// </summary>
        public TimeSpan Uptime { get; }

        /// <summary>
        /// A ThreadFactory that can be used if the transport needs to create any Threads.
        /// </summary>
        public IAdvancedScheduler Scheduler { get; }

        /// <summary>
        /// Returns a Future which will be completed after the ActorSystem has been terminated
        /// and termination hooks have been executed.
        /// </summary>
        public Task TerminationTask { get; }

        /// <summary>
        /// The deadLetter address is a destination that will accept (and discard)
        /// every message sent to it.
        /// </summary>
        public IActorRef<object> DeadLetters { get; }

        /// <summary>
        /// Return a reference to this system’s [[akka.actor.typed.receptionist.Receptionist]].
        /// </summary>
        public IActorRef<IReceptionistCommand> Receptionist { get; }

        /// <summary>
        /// Create an actor in the "/system" namespace. This actor will be shut down
        /// during system.terminate only after all user actors have terminated.
        /// 
        /// The returned Future of [[ActorRef]] may be converted into an [[ActorRef]]
        /// to which messages can immediately be sent by using the `ActorRef.apply`
        /// method.
        /// </summary>
        public ValueTask<IActorRef<T2>> SystemActorOf<T2>(Behavior<T2> behavior, string name, CancellationToken cancellationToken, Props props = null) where T2 : class
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public IActorRef<T2> SpawnAnonymous<T2>(Behavior<T2> behavior, Props props = null) where T2 : class
        {
            throw new NotImplementedException();
        }

        public IActorRef<T2> Spawn<T2>(Behavior<T2> behavior, string name, Props props = null) where T2 : class
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Terminates this actor system. This will stop the guardian actor, which in turn
        /// will recursively stop all its child actors, then the system guardian
        /// (below which the logging actors reside).
        /// </summary>
        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }

        #region IActorRef implementations

        public ActorPath Path => throw new System.NotImplementedException();

        public int CompareTo(IActorRef<TMessage> other)
        {
            throw new System.NotImplementedException();
        }

        public IActorRef<TNarrowed> Narrow<TNarrowed>() where TNarrowed : class, TMessage
        {
            throw new System.NotImplementedException();
        }

        public void Tell(TMessage message)
        {
            throw new System.NotImplementedException();
        }

        public IActorRef<T2> UnsafeCast<T2>() where T2 : class
        {
            throw new System.NotImplementedException();
        }

        #endregion

        public int CompareTo(object obj)
        {
            throw new NotImplementedException();
        }
    }
}