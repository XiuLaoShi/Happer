﻿using System;

namespace Happer.Http
{
    public class RequestPipelinesException : Exception
    {
        public RequestPipelinesException(Exception innerException)
            : base("Request pipelines error occurred.", innerException)
        {
        }
    }
}
