﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Happer.Http.Responses
{
    /// <summary>
    /// Represents a HTML (text/html) response
    /// </summary>
    public class HtmlResponse : Response
    {
        public HtmlResponse(
            HttpStatusCode statusCode = HttpStatusCode.OK, 
            Action<Stream> contents = null, 
            IDictionary<string, string> headers = null, 
            IEnumerable<Cookie> cookies = null)
        {
            this.ContentType = "text/html";
            this.StatusCode = statusCode;

            if (contents != null)
            {
                this.Contents = contents;
            }

            if (headers != null)
            {
                this.Headers = headers;
            }

            if (cookies != null)
            {
                foreach (var cookie in cookies)
                {
                    this.Cookies.Add(cookie);
                }
            }
        }
    }
}
