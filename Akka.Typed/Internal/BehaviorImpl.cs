#region copyright
// -----------------------------------------------------------------------
// <copyright file="BehaviorImpl.cs" company="Bartosz Sypytkowski">
//     Copyright (C) 2018-2018 Bartosz Sypytkowski <b.sypytkowski@gmail.com>
// </copyright>
// -----------------------------------------------------------------------
#endregion

using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Akka.Typed.Internal
{
    internal sealed class ReceiveBehavior<TMessage> : ExtensibleBehavior<TMessage> where TMessage : class
    {
        [NotNull] private readonly Receive<TMessage> _onMessage;
        [NotNull] private readonly ReceiveSignal<TMessage> _onSignal;

        public ReceiveBehavior([NotNull]Receive<TMessage> onMessage, ReceiveSignal<TMessage> onSignal = null)
        {
            _onMessage = onMessage;
            _onSignal = onSignal ?? Behavior<TMessage>.UnhandledSignal;
        }

        public override Behavior<TNarrowed> Narrow<TNarrowed>()
        {
            throw new System.NotImplementedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override ValueTask<Behavior<TMessage>> Receive(IActorContext<TMessage> context, TMessage message) =>
            _onMessage(context, message);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override ValueTask<Behavior<TMessage>> ReceiveSignal(IActorContext<TMessage> context, ISignal signal) =>
            _onSignal(context, signal);
    }

    /// <summary>
    /// Similar to <see cref="ReceiveBehavior{TMessage}"/> however `onMessage` does not accept context.
    /// We implement it separately in order to be able to avoid wrapping each function in
    /// another function which drops the context parameter.
    /// </summary>
    /// <seealso cref="ReceiveBehavior{TMessage}"/>
    internal sealed class ReceiveMessageBehavior<TMessage> : ExtensibleBehavior<TMessage> where TMessage : class
    {
        [NotNull] private readonly ReceiveMessage<TMessage> _onMessage;
        [NotNull] private readonly ReceiveSignal<TMessage> _onSignal;

        public ReceiveMessageBehavior([NotNull]ReceiveMessage<TMessage> onMessage, ReceiveSignal<TMessage> onSignal = null)
        {
            _onMessage = onMessage;
            _onSignal = onSignal ?? Behavior<TMessage>.UnhandledSignal;
        }

        public override Behavior<TNarrowed> Narrow<TNarrowed>()
        {
            throw new System.NotImplementedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override ValueTask<Behavior<TMessage>> Receive(IActorContext<TMessage> context, TMessage message) =>
            _onMessage(message);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override ValueTask<Behavior<TMessage>> ReceiveSignal(IActorContext<TMessage> context, ISignal signal) =>
            _onSignal(context, signal);
    }

    internal sealed class OrElseBehavior<TMessage> : ExtensibleBehavior<TMessage> where TMessage : class
    {
        private readonly Behavior<TMessage> _first;
        private readonly Behavior<TMessage> _second;

        public OrElseBehavior(Behavior<TMessage> first, Behavior<TMessage> second)
        {
            _first = first;
            _second = second;
        }

        public override Behavior<TNarrowed> Narrow<TNarrowed>()
        {
            throw new System.NotImplementedException();
        }

        public override ValueTask<Behavior<TMessage>> Receive(IActorContext<TMessage> context, TMessage message)
        {
            throw new System.NotImplementedException();
        }

        public override ValueTask<Behavior<TMessage>> ReceiveSignal(IActorContext<TMessage> context, ISignal signal)
        {
            throw new System.NotImplementedException();
        }
    }
}