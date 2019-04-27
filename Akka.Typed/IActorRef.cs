#region copyright
// -----------------------------------------------------------------------
// <copyright file="IActorRef.cs" company="Bartosz Sypytkowski">
//     Copyright (C) 2018-2018 Bartosz Sypytkowski <b.sypytkowski@gmail.com>
// </copyright>
// -----------------------------------------------------------------------
#endregion

using System;
using Akka.Actor;

namespace Akka.Typed
{
    public interface IAddressable
    {
        /// <summary>
        /// The hierarchical path name of the referenced Actor. The lifecycle of the
        /// ActorRef is fully contained within the lifecycle of the [[akka.actor.ActorPath]]
        /// and more than one Actor instance can exist with the same path at different
        /// points in time, but not concurrently.
        /// </summary>
        ActorPath Path { get; }

        /// <summary>
        /// Unsafe utility method for widening the type accepted by this ActorRef;
        /// provided to avoid having to use `asInstanceOf` on the full reference type,
        /// which would unfortunately also work on non-ActorRefs.
        /// </summary>
        IActorRef<T2> UnsafeCast<T2>();
    }

    public interface IRecipientRef<in TMessage>
    {
        /// <summary>
        /// Send a message to the destination referenced by this <see cref="IRecipientRef{T}"/>
        /// using at-most-once messaging semantics.
        /// </summary>
        /// <param name="message"></param>
        void Tell(TMessage message); //TODO: make this return ValueTask?
    }

    /// <summary>
    /// An ActorRef is the identity or address of an Actor instance. It is valid
    /// only during the Actor’s lifetime and allows messages to be sent to that
    /// Actor instance. Sending a message to an Actor that has terminated before
    /// receiving the message will lead to that message being discarded; such
    /// messages are delivered to the <see cref="DeadLetter"/> channel of the
    /// <see cref="EventStream"/> on a best effort basis
    /// (i.e. this delivery is not reliable).
    /// 
    /// Not for user extension
    /// </summary>
    public interface IActorRef<in TMessage> : IAddressable, IRecipientRef<TMessage>, IComparable
    {
        /// <summary>
        /// Narrow the type of this <see cref="IActorRef{T}"/>, which is always a safe operation.
        /// </summary>
        /// <typeparam name="TNarrowed">Subtype of <typeparamref name="TMessage"/></typeparam>
        /// <returns></returns>
        IActorRef<TNarrowed> Narrow<TNarrowed>() where TNarrowed : TMessage;
    }
}