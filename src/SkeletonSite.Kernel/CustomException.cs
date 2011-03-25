using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkeletonSite.Kernel
{
    /// <summary>
    /// Base class for all exceptions within this solution
    /// </summary>
    [Serializable]
    public class CustomException : Exception
    {
        public CustomException()
            : base()
        {
        }

        public CustomException(string message)
            : base(message)
        {
        }

        public CustomException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public CustomException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}
