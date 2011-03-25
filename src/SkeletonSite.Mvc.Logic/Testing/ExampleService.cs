using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SkeletonSite.Kernel.Logging;

namespace SkeletonSite.Mvc.Logic.Testing
{
    public static class ExampleService
    {
        private static readonly BaseLogger _logger = Logger.GetLogger(typeof(ExampleService));

        public static void TestConnection()
        {
            _logger.Debug("Test ExampleServiceServiceProvider connection");
            var provider = new ExampleServiceServiceProvider();

            _logger.Debug("Open connection");
            provider.Service.Open();

            _logger.Debug("Close connection");
            provider.Service.Close();

            _logger.Debug("Done");
            provider.Dispose();
        }
    }
}
