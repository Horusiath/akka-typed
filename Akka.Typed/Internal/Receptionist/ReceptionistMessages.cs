#region copyright
// -----------------------------------------------------------------------
// <copyright file="ReceptionistMessages.cs" company="Bartosz Sypytkowski">
//     Copyright (C) 2018-2018 Bartosz Sypytkowski <b.sypytkowski@gmail.com>
// </copyright>
// -----------------------------------------------------------------------
#endregion

namespace Akka.Typed.Internal.Receptionist
{
    public interface IReceptionistCommand { }

    public sealed class Register : IReceptionistCommand
    {
        public Register()
        {

        }
    }

    public sealed class Find : IReceptionistCommand
    {

    }

    public sealed class Subscribe : IReceptionistCommand
    {

    }
}