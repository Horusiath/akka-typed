#region copyright
// -----------------------------------------------------------------------
// <copyright file="LocalReceptionist.cs" company="Bartosz Sypytkowski">
//     Copyright (C) 2018-2018 Bartosz Sypytkowski <b.sypytkowski@gmail.com>
// </copyright>
// -----------------------------------------------------------------------
#endregion

using Akka.Typed.Receptionists;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Akka.Typed.Internal.Receptionists
{
    using LocalServiceRegistry = ImmutableDictionary<(string, Type), ImmutableHashSet<IAddressable>>;
    using SubscriptionRegistry = ImmutableDictionary<(string, Type), ImmutableHashSet<IAddressable>>;

    public struct LocalReceptionist : IBehaviorProvider<Receptionist.ICommand>
    {
        public Behavior<Receptionist.ICommand> Behavior => behavior;

        private static Behavior<Receptionist.ICommand> behavior = CreateBehavior(LocalServiceRegistry.Empty, SubscriptionRegistry.Empty);
               
        private static void WatchWith(IActorContext<Receptionist.ICommand> context, IAddressable target, object message)
        {

        }

        public static Behavior<Receptionist.ICommand> CreateBehavior(LocalServiceRegistry serviceRegistry, SubscriptionRegistry subscriptions)
        {
            Behavior<Receptionist.ICommand> Register<T>(IActorContext<Receptionist.ICommand> context, ServiceKey<T> key, IActorRef<T> target, IActorRef<Registered<T>>? maybeReplyTo)
            {
                context.Log.Debug("Actor was registered: {0} {1}", key, target);
                WatchWith(context, target, new RegisteredActorTerminated<T>(key, target));
                if (!(maybeReplyTo is null))
                {
                    maybeReplyTo.Tell(new Registered<T>(key, target));
                }

            }

            Behavior<Receptionist.ICommand> Find()
            {

            }

            Behavior<Receptionist.ICommand> Subscribe()
            {

            }

            Behavior<Receptionist.ICommand> RegisteredActorTerminated()
            {

            }

            Behavior<Receptionist.ICommand> SubscriberTerminated()
            {

            }

            return Behaviors.Receive<Receptionist.ICommand>(async (ctx, msg) =>
            {
                switch (msg)
                {
                    case Register
                    default: return Behaviors.Unhandled<Receptionist.ICommand>();
                }
            });
        }
    }
}