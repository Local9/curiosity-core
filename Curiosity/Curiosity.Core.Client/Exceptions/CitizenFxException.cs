using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Core.Client.Exceptions
{
    class CitizenFxException : Exception
    {
        public CitizenFxException()
        {

        }

        public CitizenFxException(string message) : base(message)
        {

        }

        public CitizenFxException(string message, Exception innerException) : base(message, innerException)
        {

        }

        protected CitizenFxException(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }
    }
}
