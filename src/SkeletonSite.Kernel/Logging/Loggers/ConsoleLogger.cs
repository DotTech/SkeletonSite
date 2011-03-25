using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace SkeletonSite.Kernel.Logging
{
    public class ConsoleLogger : BaseLogger
    {
        protected override void Log(string message, params object[] formatParameters)
        {
            string formatted = String.Format(message, formatParameters);
            System.Diagnostics.Debug.WriteLine(LogFormat.Replace("{date}", DateTime.Now.ToString("yyyy-MM-dd"))
                                                        .Replace("{time}", DateTime.Now.ToString("HH:mm:ss:fff"))
                                                        .Replace("{message}", formatted));
        }
    }
}
