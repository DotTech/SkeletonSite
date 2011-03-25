using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkeletonSite.Kernel.Logging
{
    /// <summary>
    /// This the main logger which is returned by Logger.GetLogger()
    /// When a log method is called, this class will send the call to all Logger instances that must be called
    /// </summary>
    public class LoggerManager : BaseLogger
    {
        private List<BaseLogger> _loggers;

        public LoggerManager(List<Configuration.LoggingConfiguration.Logger> loggersConfiguration)
        {
            _loggers = new List<BaseLogger>();

            // Create ILogger instances
            foreach (var config in loggersConfiguration)
            {
                BaseLogger logger = (BaseLogger)Activator.CreateInstance(config.LoggerType);
                logger.Level = config.Level;
                logger.LogFormat = config.LogFormat;
                logger.CustomSettings = config.CustomSettings;
                _loggers.Add(logger);
            }
        }


        #region ILogger implementation
        public override void Debug(string message, params object[] formatParameters)
        {
            foreach (var logger in _loggers)
            {
                if (logger.Level == LogLevels.Debug)
                {
                    logger.Debug(message, formatParameters);
                }
            }
        }

        public override void Info(string message, params object[] formatParameters)
        {
            foreach (var logger in _loggers)
            {
                if (logger.Level >= LogLevels.Info)
                {
                    logger.Info(message, formatParameters);
                }
            }
        }

        public override void Error(string message, params object[] formatParameters)
        {
            foreach (var logger in _loggers)
            {
                if (logger.Level >= LogLevels.Error)
                {
                    logger.Error(message, formatParameters);
                }
            }
        }

        public override void Error(string message, Exception ex, params object[] formatParameters)
        {
            foreach (var logger in _loggers)
            {
                if (logger.Level >= LogLevels.Error)
                {
                    logger.Error(message, ex: ex, formatParameters: formatParameters);
                }
            }
        }

        protected override void Log(string message, params object[] formatParameters)
        {
        }
        #endregion
    }
}
