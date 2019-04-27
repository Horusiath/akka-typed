#region copyright
// -----------------------------------------------------------------------
// <copyright file="ServiceKey.cs" company="Bartosz Sypytkowski">
//     Copyright (C) 2018-2018 Bartosz Sypytkowski <b.sypytkowski@gmail.com>
// </copyright>
// -----------------------------------------------------------------------
#endregion

using System;

namespace Akka.Typed.Receptionists
{
    public readonly struct ServiceKey<T>
    {
        public readonly string Id;

        public ServiceKey(string id)
        {
            Id = id;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode() => Akka.Util.MurmurHash.StringHash(Id);

        public static void Deconstruct(ServiceKey<T> key, out string id, out Type type)
        {
            id = key.Id;
            type = typeof(T);
        }

        public override string ToString() => $"ServiceKey<{typeof(T)}>({Id})";
    }
}