#region copyright
// -----------------------------------------------------------------------
// <copyright file="PoisonPill.cs" company="Bartosz Sypytkowski">
//     Copyright (C) 2018-2018 Bartosz Sypytkowski <b.sypytkowski@gmail.com>
// </copyright>
// -----------------------------------------------------------------------
#endregion

namespace Akka.Typed.Internal
{
    internal sealed class PoisonPill : ISignal
    {
        public static readonly PoisonPill Instance = new PoisonPill();
        private PoisonPill() { }
    }


}