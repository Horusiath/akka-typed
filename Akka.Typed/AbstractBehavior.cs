#region copyright
// -----------------------------------------------------------------------
// <copyright file="AbstractBehavior.cs" company="Bartosz Sypytkowski">
//     Copyright (C) 2018-2018 Bartosz Sypytkowski <b.sypytkowski@gmail.com>
// </copyright>
// -----------------------------------------------------------------------
#endregion

using System.Threading.Tasks;

namespace Akka.Typed
{
    /// <summary>
    /// An actor `Behavior` can be implemented by extending this class and implement the
    /// abstract method <see cref="OnMessage"/> and optionally override <see cref="OnSignal"/>.
    /// Mutable state can be defined as instance variables of the class.
    /// 
    /// This is an Object-oriented style of defining a `Behavior`. A more functional style
    /// alternative is provided by the factory methods in [[Behaviors]], for example
    /// [[Behaviors.receiveMessage]].
    /// 
    /// Instances of this behavior should be created via [[Behaviors.setup]] and if
    /// the [[ActorContext]] is needed it can be passed as a constructor parameter
    /// from the factory function.
    /// </summary>
    public abstract class AbstractBehavior<T> where T : class
    {
        public abstract ValueTask<Behavior<T>> OnMessage(IActorContext<T> context, T message);

        public virtual ValueTask<Behavior<T>> OnSignal(IActorContext<T> context, ISignal message) =>
            new ValueTask<Behavior<T>>(Behaviors.Unhandled<T>());
    }
}