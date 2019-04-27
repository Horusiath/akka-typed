#region copyright
// -----------------------------------------------------------------------
// <copyright file="BehaviorImpl.cs" company="Bartosz Sypytkowski">
//     Copyright (C) 2018-2018 Bartosz Sypytkowski <b.sypytkowski@gmail.com>
// </copyright>
// -----------------------------------------------------------------------
#endregion

using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Akka.Typed.Internal
{
    public sealed class ReceiveBehavior<TMessage> : ExtensibleBehavior<TMessage>
    {
        private readonly Receive<TMessage> _onMessage;
        private readonly ReceiveSignal<TMessage> _onSignal;

        public ReceiveBehavior(Receive<TMessage> onMessage, ReceiveSignal<TMessage> onSignal = null)
        {
            _onMessage = onMessage;
            _onSignal = onSignal;
        }

        public override Behavior<TNarrowed> Narrow<TNarrowed>()
        {
            throw new System.NotImplementedException();
        }

        public ReceiveBehavior<TMessage> ReceiveSignal(ReceiveSignal<TMessage> onSignal) =>
            new ReceiveBehavior<TMessage>(_onMessage, onSignal);

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
    public sealed class ReceiveMessageBehavior<TMessage> : ExtensibleBehavior<TMessage>
    {
        private readonly ReceiveMessage<TMessage> _onMessage;
        private readonly ReceiveSignal<TMessage> _onSignal;

        public ReceiveMessageBehavior(ReceiveMessage<TMessage> onMessage, ReceiveSignal<TMessage> onSignal = null)
        {
            _onMessage = onMessage;
            _onSignal = onSignal ?? Behavior<TMessage>.UnhandledSignal;
        }

        public override Behavior<TNarrowed> Narrow<TNarrowed>()
        {
            throw new System.NotImplementedException();
        }

        public ReceiveMessageBehavior<TMessage> ReceiveSignal(ReceiveSignal<TMessage> onSignal) =>
            new ReceiveMessageBehavior<TMessage>(_onMessage, onSignal);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override ValueTask<Behavior<TMessage>> Receive(IActorContext<TMessage> context, TMessage message) =>
            _onMessage(message);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override ValueTask<Behavior<TMessage>> ReceiveSignal(IActorContext<TMessage> context, ISignal signal) =>
            _onSignal(context, signal);
    }

    internal sealed class OrElseBehavior<TMessage> : ExtensibleBehavior<TMessage>
    {
        private readonly Behavior<TMessage> _first;
        private readonly Behavior<TMessage> _second;

        public OrElseBehavior(Behavior<TMessage> first, Behavior<TMessage> second)
        {
            _first = first;
            _second = second;
        }

        public override Behavior<TNarrowed> Narrow<TNarrowed>() => new OrElseBehavior<TNarrowed>(_first.Narrow<TNarrowed>(), _second.Narrow<TNarrowed>());

        public override async ValueTask<Behavior<TMessage>> Receive(IActorContext<TMessage> context, TMessage message)
        {
            var result = await Behaviors.Interpret(_first, context, message);
            if (result is UnhandledBehavior<TMessage>)
            {
                result = await Behaviors.Interpret(_second, context, message);
            }

            return result;
        }

        public override async ValueTask<Behavior<TMessage>> ReceiveSignal(IActorContext<TMessage> context, ISignal signal)
        {
            var result = await Behaviors.Interpret(_first, context, signal);
            if (result is UnhandledBehavior<TMessage>)
            {
                result = await Behaviors.Interpret(_second, context, signal);
            }

            return result;
        }
    }
}