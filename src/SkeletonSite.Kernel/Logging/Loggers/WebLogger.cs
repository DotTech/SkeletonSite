using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace SkeletonSite.Kernel.Logging
{
    /// <summary>
    /// Logs to SessionManager so log entries can be handled by the web application
    /// </summary>
    public class WebLogger : BaseLogger
    {
        public override void Error(string message, params object[] formatParameters)
        {
            string formatted = String.Format(message, formatParameters);
            formatted = LogFormat.Replace("{date}", DateTime.Now.ToString("yyyy-MM-dd"))
                                 .Replace("{time}", DateTime.Now.ToString("HH:mm:ss:fff"))
                                 .Replace("{message}", formatted);

            SessionManager.AddError(formatted);
        }

        public override void Error(string message, Exception ex, params object[] formatParameters)
        {
            string formatted = String.Format(message, formatParameters);
            formatted = LogFormat.Replace("{date}", DateTime.Now.ToString("yyyy-MM-dd"))
                                 .Replace("{time}", DateTime.Now.ToString("HH:mm:ss:fff"))
                                 .Replace("{message}", formatted);

            SessionManager.AddError(new CustomException(formatted, ex));
        }

        protected override void Log(string message, params object[] formatParameters)
        {
            string formatted = String.Format(message, formatParameters);
            formatted = LogFormat.Replace("{date}", DateTime.Now.ToString("yyyy-MM-dd"))
                                 .Replace("{time}", DateTime.Now.ToString("HH:mm:ss:fff"))
                                 .Replace("{message}", formatted);

            SessionManager.AddNotification(formatted);
        }
    }
}
