using System;
using System.Runtime.Serialization;

namespace ARKPZ_CourseWork_Backend.Controllers
{
    [Serializable]
    internal class DroneNotAvailableException : Exception
    {
        public DroneNotAvailableException()
        {
        }

        public DroneNotAvailableException(string message) : base(message)
        {
        }

        public DroneNotAvailableException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DroneNotAvailableException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}