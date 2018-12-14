#region copyright
// -----------------------------------------------------------------------
// <copyright file="ActorContext.cs" company="Bartosz Sypytkowski">
//     Copyright (C) 2018-2018 Bartosz Sypytkowski <b.sypytkowski@gmail.com>
// </copyright>
// -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Akka.Typed.Internal
{
    internal sealed class ActorContext<TMessage> : IActorContext<TMessage> where TMessage : class
    {
        public IActorRef<TMessage> Self { get; }
        public ActorSystem<Nothing> System { get; }
        public ILogger Log { get; }
        public IEnumerable<IActorRef<Nothing>> Children { get; }
        public bool TryGetChild<T2>(string name, out IActorRef<T2> childRef) where T2 : class
        {
            throw new NotImplementedException();
        }

        public IActorRef<T2> SpawnAnonymous<T2>(Behavior<T2> behavior, Props props = null) where T2 : class
        {
            throw new NotImplementedException();
        }

        public IActorRef<T2> Spawn<T2>(Behavior<T2> behavior, string name, Props props = null) where T2 : class
        {
            throw new NotImplementedException();
        }

        public void Stop<T2>(IActorRef<T2> child) where T2 : class
        {
            throw new NotImplementedException();
        }

        public void Watch<T2>(IActorRef<T2> other) where T2 : class
        {
            throw new NotImplementedException();
        }

        public void WatchWith<T2>(IActorRef<T2> other, TMessage message) where T2 : class
        {
            throw new NotImplementedException();
        }

        public void Unwatch<T2>(IActorRef<T2> other) where T2 : class
        {
            throw new NotImplementedException();
        }

        public void SetReceiveTimeout(TimeSpan timeout, TMessage message)
        {
            throw new NotImplementedException();
        }

        public void CancelReceiveTimeout()
        {
            throw new NotImplementedException();
        }

        public IDisposable ScheduleOnce<T2>(TimeSpan delay, IActorRef<T2> target, T2 message) where T2 : class
        {
            throw new NotImplementedException();
        }

        public TaskScheduler TaskScheduler { get; }
        public IActorRef<TSource> MessageAdapter<TSource>(Func<TSource, TMessage> adapter) where TSource : class
        {
            throw new NotImplementedException();
        }

        public ValueTask<TResult> Ask<TRequest, TResult>(IRecipientRef<TRequest> target, CancellationToken cancellationToken, Func<IActorRef<TResult>, TRequest> createRequest) where TResult : class
        {
            throw new NotImplementedException();
        }
    }
}