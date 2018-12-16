#region copyright
// -----------------------------------------------------------------------
// <copyright file="ThrowHelpers.cs" company="Bartosz Sypytkowski">
//     Copyright (C) 2018-2018 Bartosz Sypytkowski <b.sypytkowski@gmail.com>
// </copyright>
// -----------------------------------------------------------------------
#endregion

using System;
using System.Runtime.CompilerServices;

namespace Akka.Typed
{
    internal static class Raises
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void DeathPact(IActorRef<Nothing> terminatedRef)
        {
            throw new DeathPactException(terminatedRef);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void InvalidArgument(string msg)
        {
            throw new ArgumentException(msg);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void NotSupported(string msg)
        {
            throw new NotSupportedException(msg);
        }
    }
}