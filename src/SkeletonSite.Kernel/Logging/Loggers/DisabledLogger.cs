using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkeletonSite.Kernel.Logging
{
    /// <summary>
    /// Empty BaseLogger implementation which is used when logging is disabled
    /// </summary>
    public class DisabledLogger : BaseLogger
    {
        protected override void Log(string message, params object[] formatParameters)
        {
        }
    }
}
