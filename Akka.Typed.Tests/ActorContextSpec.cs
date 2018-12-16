using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Akka.Typed.Tests
{
    public class ActorContextSpec : AbstractAkkaTest
    {
        public ActorContextSpec(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void ActorContext_must_be_usable_from_behavior_interpret_message()
        {
            var b = Behaviors.Receive<string>(async (context, message) =>
            {   
                //TODO: Behavior.interpretMessage(b, context, message)
                return Behaviors.Same<string>();
            });
        }

        [Fact]
        public async Task ActorContext_must_canonicalize_behaviors()
        {
            var probe = new TestProbe<Event>();

            var behavior = Behaviors.Receive<Command>(async (context, message) =>
            {
                switch (message.Type)
                {
                    case Command.CommandType.Ping:
                        probe.Ref.Tell(Event.Pong);
                        return Behaviors.Same<Command>();
                    case Command.CommandType.Miss:
                        probe.Ref.Tell(Event.Missed);
                        return Behaviors.Unhandled<Command>();
                    case Command.CommandType.Renew:
                        var aref = ((Command<Event>)message).Ref;
                        aref.Tell(Event.Renewed);
                        return Behaviors.Same<Command>();
                    default:
                        throw new Exception($"Unexpected message {message}");
                }
            });

            var actor = this.SpawnAnonymous(behavior);
            actor.Tell(Command.Ping);

            await probe.MoveNext();
            probe.Current.Should().Be(Event.Pong);
        }

        [Fact]
        public void ActorContext_must_correctly_wire_lifecycle_hook()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void ActorContext_must_signal_PostStop_after_voluntary_termination()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void ActorContext_must_restart_and_stop_child_actor()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void ActorContext_must_stop_child_actor()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void ActorContext_must_reset_behavior_upon_restart()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void ActorContext_must_not_reset_behavior_upon_resume()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void ActorContext_must_stop_upon_stop()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void ActorContext_must_not_stop_non_child_actor()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void ActorContext_must_watch_child_actor_before_its_termination()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void ActorContext_must_watch_child_actor_after_its_termination()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void ActorContext_must_unwatch_child_actor_before_its_termination()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void ActorContext_must_terminate_upon_not_handling_Terminated()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void ActorContext_must_return_the_right_context_info()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void ActorContext_must_return_right_info_about_children()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void ActorContext_must_set_small_receive_timeout()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void ActorContext_must_set_large_receive_timeout()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void ActorContext_must_schedule_message()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void ActorContext_must_create_named_adapter()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void ActorContext_must_not_allow_null_messages()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void ActorContext_must_not_have_problems_with_stopping_already_stopped_child()
        {
            throw new NotImplementedException();
        }
    }

    internal class Command
    {
        public enum CommandType
        {
            Ping,
            Renew,
            Miss,
            Fail,
            Stop,
            StopRef,
            Inert,
            Watch,
            Unwatch,
            ReceiveTimeout,
            SetTimeout
        }

        public static readonly Command Ping = new Command(CommandType.Ping);
        public static readonly Command Miss = new Command(CommandType.Miss);
        public static readonly Command Fail = new Command(CommandType.Fail);
        public static readonly Command Stop = new Command(CommandType.Stop);
        public static readonly Command Inert = new Command(CommandType.Inert);
        public static readonly Command ReceiveTimeout = new Command(CommandType.ReceiveTimeout);
        public static Command Renew(IActorRef<Event> aref) => new Command<Event>(CommandType.Renew, aref);
        public static Command StopRef<T>(IActorRef<T> aref) where T : class => new Command<T>(CommandType.StopRef, aref);
        public static Command Watch(IActorRef<Command> aref) => new Command<Command>(CommandType.Watch, aref);
        public static Command Unwatch(IActorRef<Command> aref) => new Command<Command>(CommandType.Unwatch, aref);
        public static Command SetTimeout(TimeSpan duration) => new SetTimeout(duration);

        protected Command(CommandType type)
        {
            Type = type;
        }

        public CommandType Type { get; }
    }

    internal sealed class SetTimeout : Command
    {
        public TimeSpan Duration { get; }

        internal SetTimeout(TimeSpan duration) : base(CommandType.SetTimeout)
        {
            Duration = duration;
        }
    }

    internal sealed class Command<T> : Command where T : class
    {
        internal Command(CommandType type, IActorRef<T> @ref) : base(type)
        {
            Ref = @ref;
        }

        public IActorRef<T> Ref { get; }
    }

    internal class Event
    {

        public static readonly Event Pong = new Event(EventType.Pong);
        public static readonly Event Renewed = new Event(EventType.Renewed);
        public static readonly Event Missed = new Event(EventType.Missed);
        public static Event GotSignal(ISignal signal) => new Signaled(EventType.GotSignal, signal);
        public static Event GotChildSignal(ISignal signal) => new Signaled(EventType.GotChildSignal, signal);
        public static Event ChildMade(IActorRef<Command> aref) => new Event<Command>(EventType.ChildMade, aref);
        public static readonly Event InertEvent = new Event(EventType.InertEvent);
        public static readonly Event TimeoutSet = new Event(EventType.TimeoutSet);
        public static readonly Event GotReceiveTimeout = new Event(EventType.GotReceiveTimeout);

        protected Event(EventType type)
        {
            Type = type;
        }

        public enum EventType
        {
            Pong,
            Renewed,
            Missed,
            GotSignal,
            GotChildSignal,
            ChildMade,
            InertEvent,
            TimeoutSet,
            GotReceiveTimeout
        }

        public EventType Type { get; }
    }

    internal sealed class Signaled : Event
    {

        public ISignal Signal { get; }

        internal Signaled(EventType type, ISignal signal) : base(type)
        {
            Signal = signal;
        }
    }

    internal sealed class Event<T> : Event where T : class
    {
        internal Event(EventType type, IActorRef<T> @ref = null) : base(type)
        {
            Ref = @ref;
        }

        public IActorRef<T> Ref { get; }
    }
}
