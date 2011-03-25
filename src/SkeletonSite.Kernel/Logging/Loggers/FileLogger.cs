using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace SkeletonSite.Kernel.Logging
{
    public class FileLogger : BaseLogger
    {
        private ReaderWriterLockSlim _rwl = new ReaderWriterLockSlim();

        protected override void Log(string message, params object[] formatParameters)
        {
            string formatted = String.Format(message, formatParameters);
            string log = LogFormat.Replace("{date}", DateTime.Now.ToString("yyyy-MM-dd"))
                                  .Replace("{time}", DateTime.Now.ToString("HH:mm:ss:fff"))
                                  .Replace("{message}", formatted) + "\r\n";

            // Lock the thread until we're done writing to the log file
            _rwl.EnterWriteLock();
            try
            {
                File.AppendAllText(CustomSettings["LogFilePath"], log);
            }
            finally
            {
                _rwl.ExitWriteLock();
            }
        }
    }
}
