﻿using System.Net;

namespace Happer.Http.Responses
{
    /// <summary>
    /// Response with status code 406 (Not Acceptable).
    /// </summary>
    public class NotAcceptableResponse : Response
    {
        public NotAcceptableResponse()
        {
            this.StatusCode = HttpStatusCode.NotAcceptable;
        }
    }
}
