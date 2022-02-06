using System;

namespace UnionBankApi
{
    public class UnionBankApiException : Exception
    {
        public UnionBankApiException()
        {
        }

        public UnionBankApiException(string message) : base(message)
        {
        }

        public UnionBankApiException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
