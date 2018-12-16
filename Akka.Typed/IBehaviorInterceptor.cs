#region copyright
// -----------------------------------------------------------------------
// <copyright file="IBehaviorInterceptor.cs" company="Bartosz Sypytkowski">
//     Copyright (C) 2018-2018 Bartosz Sypytkowski <b.sypytkowski@gmail.com>
// </copyright>
// -----------------------------------------------------------------------
#endregion

using System.Threading.Tasks;

namespace Akka.Typed
{
    /// <summary>
    /// A behavior interceptor allows for intercepting message and signal reception and perform arbitrary logic -
    /// transform, filter, send to a side channel etc. It is the core API for decoration of behaviors. Many built-in
    /// intercepting behaviors are provided through factories in the respective `Behaviors`.
    /// </summary>
    /// <typeparam name="TOut">The outer message type – the type of messages the intercepting behavior will accept.</typeparam>
    /// <typeparam name="TIn">The inner message type - the type of message the wrapped behavior accepts.</typeparam>
    public interface IBehaviorInterceptor<TOut, TIn> 
        where TOut : class 
        where TIn : class
    {
        /// <summary>
        /// Override to intercept actor startup. To trigger startup of
        /// the next behavior in the stack, call <c>target.Start(context)</c>.
        /// </summary>
        /// <returns>
        /// The returned behavior will be the "started" behavior of the actor used to accept
        /// the next message or signal.
        /// </returns>
        ValueTask<Behavior<TIn>> AroundStart<TPreStartTarget>(IActorContext<TOut> context, TPreStartTarget target)
            where TPreStartTarget : IPreStartTarget<TIn>;

        /// <summary>
        /// Intercept a message sent to the running actor. Pass the message on to the next behavior
        /// in the stack by passing it to <c>target.Apply(context, message)</c>,
        /// return <see cref="Behaviors.Same{T}"/> without invoking <paramref name="target"/> to filter out the message.
        /// </summary>
        /// <returns>The behavior for next message or signal</returns>
        ValueTask<Behavior<TIn>> AroundReceive<TReceiveTarget>(IActorContext<TOut> context, TOut message, TReceiveTarget target)
            where TReceiveTarget : IReceiveTarget<TIn>;

        /// <summary>
        /// Intercept a signal sent to the running actor. Pass the signal on to the next behavior
        /// in the stack by passing it to <c>target.Apply(context, signal)</c>.
        /// </summary>
        /// <returns>The behavior for next message or signal</returns>
        ValueTask<Behavior<TIn>> AroundSignal<TSignalTarget>(IActorContext<TOut> context, ISignal signal, TSignalTarget target)
            where TSignalTarget : ISignalTarget<TIn>;
    }

    /// <summary>
    /// Abstraction of passing the on further in the behavior stack in
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IPreStartTarget<T> where T : class
    {
        ValueTask<Behavior<T>> Start<T2>(IActorContext<T2> context) where T2 : class;
    }

    /// <summary>
    /// Abstraction of passing the message on further in the behavior stack in
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IReceiveTarget<T> where T : class
    {
        ValueTask<Behavior<T>> Apply<T2>(IActorContext<T2> context, T message) where T2 : class;

        /// <summary>
        /// Signal that the received message will result in a simulated restart
        /// by the <see cref="IBehaviorInterceptor{TOuter,TInner}"/>. A <see cref="PreRestart"/> will be sent to the
        /// current behavior but the returned Behavior is ignored as a restart
        /// is taking place.
        /// </summary>
        ValueTask SignalRestart<T2>(IActorContext<T2> context) where T2 : class;
    }

    /// <summary>
    /// Abstraction of passing the signal on further in the behavior stack in
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ISignalTarget<T> where T : class
    {
        ValueTask<Behavior<T>> Apply<T2>(IActorContext<T2> context, ISignal signal) where T2 : class;
    }
}