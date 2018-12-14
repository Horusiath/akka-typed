#region copyright
// -----------------------------------------------------------------------
// <copyright file="Exceptions.cs" company="Bartosz Sypytkowski">
//     Copyright (C) 2018-2018 Bartosz Sypytkowski <b.sypytkowski@gmail.com>
// </copyright>
// -----------------------------------------------------------------------
#endregion

using System;

namespace Akka.Typed
{
    /// <summary>
    /// Exception that an actor fails with if it does not handle a Terminated message.
    /// </summary>
    public sealed class DeathPactException : Exception
    {
        public IActorRef<Nothing> ActorRef { get; }

        public DeathPactException(IActorRef<Nothing> actorRef)
            : base($"Death pact with {actorRef} was triggered.")
        {
            ActorRef = actorRef;
        }
    }
}