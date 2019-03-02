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
    }

    public interface IRecipientRef<in T>
    {
        /// <summary>
        /// Send a message to the destination referenced by this <see cref="IRecipientRef{T}"/>
        /// using at-most-once* messaging semantics.
        /// </summary>
        /// <param name="message"></param>
        void Tell(T message); //TODO: make this return ValueTask?
    }

    public interface IActorRef<in T> : IAddressable, IRecipientRef<T>, IComparable where T: class
    {
        /// <summary>
        /// Narrow the type of this <see cref="IActorRef{T}"/>, which is always a safe operation.
        /// </summary>
        /// <typeparam name="TNarrowed">Subtype of <typeparamref name="T"/></typeparam>
        /// <returns></returns>
        IActorRef<TNarrowed> Narrow<TNarrowed>() where TNarrowed : class, T;

        /// <summary>
        /// Unsafe utility method for widening the type accepted by this ActorRef;
        /// provided to avoid having to use `asInstanceOf` on the full reference type,
        /// which would unfortunately also work on non-ActorRefs.
        /// </summary>
        IActorRef<T2> UnsafeCast<T2>() where T2 : class;
    }
}