#region copyright
// -----------------------------------------------------------------------
// <copyright file="Receptionist.cs" company="Bartosz Sypytkowski">
//     Copyright (C) 2018-2018 Bartosz Sypytkowski <b.sypytkowski@gmail.com>
// </copyright>
// -----------------------------------------------------------------------
#endregion

using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Akka.Typed.Receptionists
{

    /// <summary>
    /// Associate the given <see cref="IActorRef{T}"/> with the given <see cref="ServiceKey{T}"/>. Multiple
    /// registrations can be made for the same key. De-registration is implied by
    /// the end of the referenced Actor’s lifecycle.
    /// 
    /// Registration will be acknowledged with the <see cref="Registered{T}"/> message to the given replyTo actor
    /// if there is one.
    /// </summary>
    /// <seealso cref="Registered{T}"/>
    public sealed class Register<T> : Receptionist.ICommand where T : class
    {
        public ServiceKey<T> Key { get; }
        public IActorRef<T> Service { get; }
        public IActorRef<Registered<T>>? ReplyTo { get; }

        public Register(ServiceKey<T> key, IActorRef<T> service, IActorRef<Registered<T>>? replyTo = null)
        {
            Key = key;
            Service = service;
            ReplyTo = replyTo;
        }
    }

    /// <summary>
    /// Confirmation that the given <see cref="Service"/> has been associated with the given <see cref="Key"/>.
    /// </summary>
    /// <seealso cref="Register{T}"/>
    /// <typeparam name="T"></typeparam>
    public sealed class Registered<T> where T : class
    {
        public ServiceKey<T> Key { get; }
        public IActorRef<T> Service { get; }

        internal Registered(ServiceKey<T> key, IActorRef<T> service)
        {
            Key = key;
            Service = service;
        }
    }

    /// <summary>
    /// Subscribe the given actor to service updates. When new instances are registered or unregistered to the given key
    /// the given subscriber will be sent a <see cref="Listing{T}"/> with the new set of instances for that service.
    /// 
    /// The subscription will be acknowledged by sending out a first <see cref="Listing{T}"/>. The subscription automatically ends
    /// with the termination of the subscriber.
    /// </summary>
    public sealed class Subscribe<T> : Receptionist.ICommand where T : class
    {
        public ServiceKey<T> Key { get; }
        public IActorRef<Listing<T>> Subscriber { get; }

        public Subscribe(ServiceKey<T> key, IActorRef<Listing<T>> subscriber)
        {
            Key = key;
            Subscriber = subscriber;
        }
    }

    /// <summary>
    /// Query the Receptionist for a list of all Actors implementing the given
    /// protocol at one point in time.
    /// </summary>
    public sealed class Find<T> : Receptionist.ICommand where T : class
    {
        public ServiceKey<T> Key { get; }
        public IActorRef<Listing<T>> ReplyTo { get; }

        public Find(ServiceKey<T> key, IActorRef<Listing<T>> replyTo)
        {
            Key = key;
            ReplyTo = replyTo;
        }
    }

    /// <summary>
    /// Current listing of all Actors that implement the protocol given by the <see cref="ServiceKey{T}"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class Listing<T> where T : class
    {
        public ServiceKey<T> Key { get; }
        public ImmutableHashSet<IActorRef<T>> Services { get; }

        public Listing(ServiceKey<T> key, ImmutableHashSet<IActorRef<T>> services)
        {
            Key = key;
            Services = services;
        }
    }

    /// <summary>
    /// A Receptionist is an entry point into an Actor hierarchy where select Actors
    /// publish their identity together with the protocols that they implement. Other
    /// Actors need only know the Receptionist’s identity in order to be able to use
    /// the services of the registered Actors.
    /// 
    /// These are the messages (and the extension) for interacting with the receptionist.
    /// The receptionist is easiest accessed through the system: [[ActorSystem.receptionist]]
    /// </summary>
    public static class Receptionist
    {
        public interface ICommand { }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Register<T> Register<T>(string id, IActorRef<T> service, IActorRef<Registered<T>>? replyTo = null) where T: class =>
            new Register<T>(new ServiceKey<T>(id), service, replyTo);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Subscribe<T> Subscribe<T>(string id, IActorRef<Listing<T>> subscriber) where T : class =>
            new Subscribe<T>(new ServiceKey<T>(id), subscriber);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Find<T> Find<T>(string id, IActorRef<Listing<T>> replyTo) where T : class =>
            new Find<T>(new ServiceKey<T>(id), replyTo);
    }
}