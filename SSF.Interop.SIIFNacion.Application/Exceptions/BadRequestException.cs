using System;
using System.Runtime.Serialization;

namespace SSF.Interop.SIIFNacion.Application.Exceptions
{
    [Serializable]

    public class BadRequestException : ApplicationException
    {
        public BadRequestException()
        {

        }

        public BadRequestException(string message) : base(message)
        {

        }

        protected BadRequestException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }
    }
}