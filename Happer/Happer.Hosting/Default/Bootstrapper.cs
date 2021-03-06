﻿using System;
using System.Collections.Generic;
using Happer.Http;
using Happer.Http.Responses;
using Happer.Http.Routing;
using Happer.Http.Routing.Trie;
using Happer.Http.Serialization;
using Happer.Serialization;
using Happer.StaticContent;

namespace Happer
{
    public class Bootstrapper : IBootstrapper
    {
        public Bootstrapper()
        {
        }

        IEngine IBootstrapper.BootWith(IModuleContainer container)
        {
            return BootWith(container);
        }

        IEngine IBootstrapper.BootWith(IModuleContainer container, IPipelines pipelines)
        {
            return BootWith(container, pipelines);
        }

        public Engine BootWith(IModuleContainer container)
        {
            return BootWith(container, new Pipelines());
        }

        public Engine BootWith(IModuleContainer container, IPipelines pipelines)
        {
            if (container == null)
                throw new ArgumentNullException("container");
            if (pipelines == null)
                throw new ArgumentNullException("pipelines");

            var requestDispatcher = BuildRequestDispatcher(container);
            return new Engine(requestDispatcher, pipelines);
        }

        public StaticContentProvider BuildStaticContentProvider()
        {
            var rootPathProvider = new RootPathProvider();
            var staticContnetConventions = new StaticContentsConventions(new List<Func<Context, string, Response>>
            {
                StaticContentConventionBuilder.AddDirectory("Content")
            });
            var staticContentProvider = new StaticContentProvider(rootPathProvider, staticContnetConventions);

            GenericFileResponse.SafePaths.Add(rootPathProvider.GetRootPath());

            return staticContentProvider;
        }

        public RequestDispatcher BuildRequestDispatcher(IModuleContainer container)
        {
            var moduleCatalog = new ModuleCatalog(
                    () => { return container.GetAllModules(); },
                    (Type moduleType) => { return container.GetModule(moduleType); }
                );

            var routeSegmentExtractor = new RouteSegmentExtractor();
            var routeDescriptionProvider = new RouteDescriptionProvider();
            var routeCache = new RouteCache(routeSegmentExtractor, routeDescriptionProvider);
            routeCache.BuildCache(moduleCatalog.GetAllModules());

            var trieNodeFactory = new TrieNodeFactory();
            var routeTrie = new RouteResolverTrie(trieNodeFactory);
            routeTrie.BuildTrie(routeCache);

            var serializers = new List<ISerializer>() { new JsonSerializer(), new XmlSerializer() };
            var responseFormatterFactory = new ResponseFormatterFactory(serializers);
            var moduleBuilder = new ModuleBuilder(responseFormatterFactory);

            var routeResolver = new RouteResolver(moduleCatalog, moduleBuilder, routeTrie);

            var negotiator = new ResponseNegotiator();
            var routeInvoker = new RouteInvoker(negotiator);
            var requestDispatcher = new RequestDispatcher(routeResolver, routeInvoker);

            return requestDispatcher;
        }
    }
}
