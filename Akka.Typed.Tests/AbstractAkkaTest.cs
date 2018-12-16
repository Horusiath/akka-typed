#region copyright
// -----------------------------------------------------------------------
//  <copyright file="AbstractAkkaTest.cs" company="Akka.NET Project">
//      Copyright (C) 2009-2018 Lightbend Inc. <http://www.lightbend.com>
//      Copyright (C) 2013-2018 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------
#endregion

using Xunit.Abstractions;

namespace Akka.Typed.Tests
{
    public abstract class AbstractAkkaTest : ActorSystem<Nothing>
    {
        public AbstractAkkaTest(ITestOutputHelper output)
        {
        }
    }
}