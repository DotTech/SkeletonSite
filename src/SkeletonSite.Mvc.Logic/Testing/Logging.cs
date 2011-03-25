using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SkeletonSite.Kernel.Logging;

namespace SkeletonSite.Mvc.Logic.Testing
{
    public class Logging
    {
        public static void TestLogging()
        {
            BaseLogger logger = Logger.GetLogger(typeof(Logging));
            logger.Debug("Debug message");
            logger.Info("Info message");
            logger.Error("Error message");
        }
    }
}
