#region copyright
// -----------------------------------------------------------------------
// <copyright file="Nothing.cs" company="Bartosz Sypytkowski">
//     Copyright (C) 2018-2018 Bartosz Sypytkowski <b.sypytkowski@gmail.com>
// </copyright>
// -----------------------------------------------------------------------
#endregion

namespace Akka.Typed
{
    /// <summary>
    /// A class representing type, that cannot be instantiated or used in any way.
    /// It can only be used as filler for type generic params.
    /// </summary>
    public sealed class Nothing
    {
        private Nothing() { }
    }

    /// <summary>
    /// An empty value type. It has no space and occupies no memory.
    /// </summary>
    public readonly struct Void
    {
        public static readonly Void Default = new Void();
    }
}