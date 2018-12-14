#region copyright
// -----------------------------------------------------------------------
// <copyright file="SystemMessage.cs" company="Bartosz Sypytkowski">
//     Copyright (C) 2018-2018 Bartosz Sypytkowski <b.sypytkowski@gmail.com>
// </copyright>
// -----------------------------------------------------------------------
#endregion

using System;

namespace Akka.Typed.Internal
{
    internal interface ISystemMessage
    {
    }

    internal sealed class Create : ISystemMessage
    {
        public static readonly Create Instance = new Create();
        private Create() { }
    }

    internal sealed class Terminate : ISystemMessage
    {
        public static readonly Terminate Instance = new Terminate();
        private Terminate() { }
    }

    [Equals]
    internal sealed class Watch : ISystemMessage
    {
        public IActorRef<Nothing> Watchee { get; }
        public IActorRef<Nothing> Watcher { get; }

        public Watch(IActorRef<Nothing> watchee, IActorRef<Nothing> watcher)
        {
            Watchee = watchee;
            Watcher = watcher;
        }
    }

    [Equals]
    internal sealed class Unwatch : ISystemMessage
    {
        public IActorRef<Nothing> Watchee { get; }
        public IActorRef<Nothing> Watcher { get; }

        public Unwatch(IActorRef<Nothing> watchee, IActorRef<Nothing> watcher)
        {
            Watchee = watchee;
            Watcher = watcher;
        }
    }

    internal sealed class DeathWatchNotification : ISystemMessage
    {
        public IActorRef<Nothing> ActorRef { get; }
        public Exception Cause { get; }

        public DeathWatchNotification(IActorRef<Nothing> actorRef, Exception cause)
        {
            ActorRef = actorRef;
            Cause = cause;
        }
    }
}