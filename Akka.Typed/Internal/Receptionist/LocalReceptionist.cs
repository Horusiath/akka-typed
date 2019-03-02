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
               
        public static Behavior<Receptionist.ICommand> CreateBehavior(LocalServiceRegistry serviceRegistry, SubscriptionRegistry subscriptions)
        {
            Behavior<Receptionist.ICommand> UpdateRegistry()
            {

            }

            return Behaviors.Receive<Receptionist.ICommand>(async (ctx, msg) =>
            {
                switch (msg)
                {
                    case Register<object> 
                    default: return Behaviors.Unhandled<Receptionist.ICommand>();
                }
            });
        }
    }
}