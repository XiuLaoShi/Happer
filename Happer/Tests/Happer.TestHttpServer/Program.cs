﻿using System;
using System.Diagnostics;
using Happer.Hosting.Self;
using Happer.Http;
using Happer.Metrics;
using Logrila.Logging;
using Logrila.Logging.NLogIntegration;
using Metrics;

namespace Happer.TestHttpServer
{
    class Program
    {
        private static ILog _log;

        static Program()
        {
            NLogLogger.Use();
            _log = Logger.Get<Program>();
        }

        static void Main(string[] args)
        {
            var pipelines = new Pipelines();

            Metric.Config
                .WithAllCounters()
                //.WithReporting(r => r.WithConsoleReport(TimeSpan.FromSeconds(30))) // optional -- display to console
                //.WithReporting(r => r.WithCSVReports(@"C:\metrics\csv", TimeSpan.FromSeconds(30)))
                //.WithReporting(r => r.WithTextFileReport(@"C:\metrics\text\metrics.txt", TimeSpan.FromSeconds(30)))
                //.WithHttpEndpoint("http://localhost:3201/metrics/")                // optional -- listen http port
                .WithHapper(pipelines);

            var container = new ModuleContainer();
            container.AddModule(new TestModule());

            // optional -- http://localhost:3202/metrics
            // optional -- http://localhost:3202/metrics/ping
            // optional -- http://localhost:3202/metrics/text
            // optional -- http://localhost:3202/metrics/health
            // optional -- http://localhost:3202/metrics/json
            // optional -- http://localhost:3202/metrics/v1/health
            // optional -- http://localhost:3202/metrics/v1/json
            // optional -- http://localhost:3202/metrics/v2/json
            container.AddModule(new MetricsModule());

            var bootstrapper = new Bootstrapper();
            var engine = bootstrapper.BootWith(container, pipelines);

            string uri = "http://localhost:3202/";
            var host = new SelfHost(engine, new Uri(uri));
            host.Start();
            _log.WarnFormat("Server is listening on [{0}].", uri);

            //AutoNavigateTo(uri);

            Console.ReadKey();
            _log.WarnFormat("Stopped. Goodbye!");
            host.Stop();
            Console.ReadKey();
        }

        private static void AutoNavigateTo(string uri)
        {
            try
            {
                Process.Start(uri);
            }
            catch (Exception) { }
        }
    }
}
