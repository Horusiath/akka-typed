using System;
using System.Collections.Generic;
using System.Text;

namespace Akka.Typed
{
    public interface IBehaviorProvider<T> where T: class
    {
        Behavior<T> Behavior { get; }
    }
}
