﻿using System;
using System.Collections.Generic;
using System.Linq;
using Happer.Http.Routing.Trie;
using Happer.Http.Utilities;

namespace Happer.Http.Routing
{
    public class RouteResolver
    {
        private readonly ModuleCatalog catalog;
        private readonly ModuleBuilder moduleBuilder;
        private readonly RouteResolverTrie routeTrie;

        public RouteResolver(
            ModuleCatalog catalog,
            ModuleBuilder moduleBuilder,
            RouteResolverTrie routeTrie)
        {
            if (catalog == null)
                throw new ArgumentNullException("catalog");
            if (moduleBuilder == null)
                throw new ArgumentNullException("moduleBuilder");
            if (routeTrie == null)
                throw new ArgumentNullException("routeTrie");

            this.catalog = catalog;
            this.moduleBuilder = moduleBuilder;
            this.routeTrie = routeTrie;
        }

        public ResolveResult Resolve(Context context)
        {
            var pathDecoded = HttpUtility.UrlDecode(context.Request.Path);

            var results = this.routeTrie.GetMatches(GetMethod(context), pathDecoded, context);

            if (!results.Any())
            {
                var allowedMethods = this.routeTrie.GetOptions(pathDecoded, context).ToArray();

                if (IsOptionsRequest(context))
                {
                    return BuildOptionsResult(allowedMethods, context);
                }

                return IsMethodNotAllowed(allowedMethods) ?
                    BuildMethodNotAllowedResult(context, allowedMethods) :
                    GetNotFoundResult(context);
            }

            // Sort in descending order
            Array.Sort(results, (m1, m2) => -m1.CompareTo(m2));

            for (var index = 0; index < results.Length; index++)
            {
                var matchResult = results[index];
                if (matchResult.Condition == null || matchResult.Condition.Invoke(context))
                {
                    return this.BuildResult(context, matchResult);
                }
            }

            return GetNotFoundResult(context);
        }

        private static ResolveResult BuildMethodNotAllowedResult(Context context, IEnumerable<string> allowedMethods)
        {
            var route =
                new MethodNotAllowedRoute(context.Request.Path, context.Request.Method, allowedMethods);

            return new ResolveResult(route, new DynamicDictionary());
        }

        private static bool IsMethodNotAllowed(IEnumerable<string> allowedMethods)
        {
            return allowedMethods.Any();
        }

        private static bool IsOptionsRequest(Context context)
        {
            return context.Request.Method.Equals("OPTIONS", StringComparison.Ordinal);
        }

        private static ResolveResult BuildOptionsResult(IEnumerable<string> allowedMethods, Context context)
        {
            var path = context.Request.Path;

            var optionsResult = new OptionsRoute(path, allowedMethods);

            return new ResolveResult(optionsResult, new DynamicDictionary());
        }

        private ResolveResult BuildResult(Context context, MatchResult result)
        {
            var associatedModule = GetModuleFromMatchResult(context, result);

            var route = associatedModule.Routes.ElementAt(result.RouteIndex);
            var parameters = DynamicDictionary.Create(result.Parameters);

            return new ResolveResult
            {
                Route = route,
                Parameters = parameters,
            };
        }

        private Module GetModuleFromMatchResult(Context context, MatchResult result)
        {
            var module = this.catalog.GetModule(result.ModuleType);
            return this.moduleBuilder.BuildModule(module, context);
        }

        private static ResolveResult GetNotFoundResult(Context context)
        {
            return new ResolveResult
            {
                Route = new NotFoundRoute(context.Request.Method, context.Request.Path),
                Parameters = DynamicDictionary.Empty,
            };
        }

        private static string GetMethod(Context context)
        {
            var requestedMethod = context.Request.Method;
            return requestedMethod;
        }
    }
}
