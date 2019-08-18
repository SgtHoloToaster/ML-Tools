using System;

namespace Common.Exceptions
{
    public class NotFittedException : InvalidOperationException
    {
        public NotFittedException() : base("Object is not fitted") { }

        public NotFittedException(string message) : base(message) { }
    }
}
