#region copyright
// -----------------------------------------------------------------------
//  <copyright file="AskSpec.cs" company="Akka.NET Project">
//      Copyright (C) 2009-2018 Lightbend Inc. <http://www.lightbend.com>
//      Copyright (C) 2013-2018 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------
#endregion

using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Akka.Typed.Tests
{
    public class AskSpec : AbstractAkkaTest
    {
        enum MessageType
        {
            Foo,
            Stop
        }

        sealed class Msg
        {
            public static Msg Foo(string s, IActorRef<string> replyTo) => new Msg(MessageType.Foo, replyTo, s);
            public static Msg Stop(IActorRef<string> replyTo) => new Msg(MessageType.Stop, replyTo, null);

            public readonly MessageType Type;
            public readonly IActorRef<string> ReplyTo;
            public readonly string Content;

            private Msg(MessageType type, IActorRef<string> replyTo, string content)
            {
                Type = type;
                ReplyTo = replyTo;
                Content = content;
            }
        }

        private readonly Behavior<Msg> behavior;

        public AskSpec(ITestOutputHelper output) : base(output)
        {
            this.behavior = Behaviors.Receive<Msg>(async (ctx, msg) =>
            {
                switch (msg.Type)
                {
                    case MessageType.Foo:
                        msg.ReplyTo.Tell("foo");
                        return Behaviors.Same<Msg>();
                    case MessageType.Stop:
                        msg.ReplyTo.Tell("stopped");
                        return Behaviors.Stopped<Msg>();
                    default: throw new NotImplementedException();
                };
            });
        }

        [Fact]
        public async Task Ask_fails_if_actor_is_already_terminated()
        {
            var actor = SpawnAnonymous(behavior);
            await actor.Ask(Msg.Stop(this));
        }

        [Fact]
        public async Task Ask_succeeds_when_actor_is_alive()
        {

        }

        [Fact]
        public async Task Ask_fails_if_actor_doesnt_reply_before_cancellation()
        {

        }

        [Fact]
        public async Task Ask_fails_if_actor_doesnt_exists()
        {

        }

        [Fact]
        public async Task Ask_transforms_StatusFailure_to_failed_result()
        {

        }

        [Fact]
        public async Task Ask_fails_asking_actor_if_responder_throws()
        {

        }
    }
}