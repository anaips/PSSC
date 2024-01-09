using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace ProiectPSSC.Domain.Exceptions
{
    internal class InvalidCantitateException: Exception
    {
        public InvalidCantitateException()
        {
        }

        public InvalidCantitateException(string? message) : base(message)
        {
        }

        public InvalidCantitateException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected InvalidCantitateException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
