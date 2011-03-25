using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkeletonSite.Kernel.Logging
{
    public enum LogLevels
    {
        None = 0,
        Error = 1,
        Info = 2,
        Debug = 3
    }

    public static class Logger
    {
        private static LoggerManager _mainLogger;
        private static DisabledLogger _disabledLogger;

        private static LoggerManager MainLogger
        {
            get
            {
                if (_mainLogger == null)
                {
                    _mainLogger = new LoggerManager(Configuration.Logging.Loggers);
                }
                return _mainLogger;
            }
        }

        private static DisabledLogger DisabledLogger
        {
            get
            {
                if (_disabledLogger == null)
                {
                    _disabledLogger = new DisabledLogger();
                }
                return _disabledLogger;
            }
        }

        /// <summary>
        /// Returns ILogger that handles the logging for specified caller.
        /// From this method we can control wether or not we are going to log anything and which logger to use.
        /// </summary>
        /// <param name="caller"></param>
        /// <returns></returns>
        public static BaseLogger GetLogger(Type caller)
        {
            if (Configuration.Logging.Enabled)
            {
                return MainLogger;
            }

            // If logging is disabled we return an empty ILogger implementation
            return DisabledLogger;
        }
    }
}
