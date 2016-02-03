﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Happer.Http.Responses;

namespace Happer.Http.Routing
{
    public class RouteInvoker
    {
        private readonly ResponseNegotiator negotiator;

        public RouteInvoker(ResponseNegotiator negotiator)
        {
            if (negotiator == null)
                throw new ArgumentNullException("negotiator");

            this.negotiator = negotiator;
        }

        public async Task<Response> Invoke(Route route, CancellationToken cancellationToken, DynamicDictionary parameters, Context context)
        {
            var result = await route.Invoke(parameters, cancellationToken);
            return this.negotiator.NegotiateResponse(result, context);
        }
    }
}
