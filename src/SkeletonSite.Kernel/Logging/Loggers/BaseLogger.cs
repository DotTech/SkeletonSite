using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkeletonSite.Kernel.Logging
{
    public abstract class BaseLogger
    {
        public LogLevels Level { get; set; }
        public string LogFormat { get; set; }
        public Dictionary<string, string> CustomSettings { get; set; }

        public virtual void Debug(string message, params object[] formatParameters)
        {
            Log(message, formatParameters);
        }

        public virtual void Info(string message, params object[] formatParameters)
        {
            Log(message, formatParameters);
        }

        public virtual void Error(string message, params object[] formatParameters)
        {
            Error(message, ex: null, formatParameters: formatParameters);
        }

        public virtual void Error(string message, Exception ex, params object[] formatParameters)
        {
            Log(message, formatParameters);
        }

        /// <summary>
        /// Each logger requires to implement its own Log() method
        /// </summary>
        /// <param name="message"></param>
        protected abstract void Log(string message, params object[] formatParameters);
    }
}
