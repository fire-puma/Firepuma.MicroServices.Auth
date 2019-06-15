using System;
using System.Net;

namespace Firepuma.MicroServices.Auth.Exceptions
{
    public class MicroServiceHttpException : Exception
    {
        public MicroServiceHttpException(HttpStatusCode statusCode, string statusDescription)
            : base($"Code: {statusCode}, Status: {statusDescription}")
        {
        }
    }
}