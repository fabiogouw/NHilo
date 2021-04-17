using NHiLo.Common;
using System;
using System.Runtime.Serialization;

namespace NHiLo // this should be available at the root namespace
{
    /// <summary>
    /// A custom exception for NHiLo.
    /// </summary>
    [Serializable]
    public class NHiLoException : ApplicationException, ISerializable
    {

        internal NHiLoException(ErrorCodes errorCode)
            : base(GetMessageForErrorCode(errorCode))
        {
            ErrorCode = errorCode;
        }
        internal NHiLoException(ErrorCodes errorCode, Exception innerException)
            : base(GetMessageForErrorCode(errorCode), innerException)
        {
            ErrorCode = errorCode;
        }

        protected NHiLoException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }

        /// <summary>
        /// Gives the error code that distinguishes the reason.
        /// </summary>
        public ErrorCodes ErrorCode { get; private set; }

        private static string GetMessageForErrorCode(ErrorCodes errorCode)
        {
            return errorCode.GetDescription();
        }

        public NHiLoException WithInfo(string key, string value)
        {
            Data.Add(key, value);
            return this;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("ErrorCode", ErrorCode);
        }
    }
}
