using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SkeletonSite.Kernel;
using SkeletonSite.Mvc.Logic.Services.Example;
using SkeletonSite.Kernel.Logging;

namespace SkeletonSite.Mvc.Logic
{
    public class ExampleServiceServiceProvider
    {
        public const string ContractName = "Services.Example.ExampleService";

        private static readonly BaseLogger _logger = Logger.GetLogger(typeof(ExampleServiceServiceProvider));
        private readonly ExampleServiceSoapChannel _service;
        private readonly ManualChannelFactory<ExampleServiceSoapChannel> _channel;

        public ExampleServiceServiceProvider()
        {
            _logger.Debug("ExampleServiceServiceProvider() constructor");

            _channel = new ManualChannelFactory<ExampleServiceSoapChannel>(ContractName);
            _service = _channel.CreateChannel();

            _logger.Debug("ExampleServiceServiceProvider channel created");
        }

        /// <summary>
        /// Return ExampleService service
        /// </summary>
        public ExampleServiceSoapChannel Service
        {
            get { return _service; }
        }

        public void Dispose()
        {
            _channel.Close();
        }
    }
}
