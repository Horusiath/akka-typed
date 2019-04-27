namespace Akka.Typed.Internal
{
    internal interface IInternalRecipientRef<in TMessage> : IRecipientRef<TMessage>
    {
        /// <summary>
        /// Get a reference to the actor ref provider which created this ref.
        /// </summary>
        Akka.Actor.IActorRefProvider Provider { get; }
        
        /// <summary>
        /// Returns <c>true</c> if the actor is locally known to be terminated, <c>false</c> if alive or uncertain.
        /// </summary>
        bool IsTerminated { get; }
    }
}