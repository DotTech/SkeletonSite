using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace SkeletonSite.Kernel.Logging
{
    /// <summary>
    /// Implementation of StringWriter that writes string output to the Debug console.
    /// It's used to show log output from NHibernate in the debug console
    /// </summary>
    public class DebugConsoleWriter : StringWriter
    {
        public override void WriteLine(string value)
        {
            Debug.WriteLine(value);
            base.WriteLine(value);
        }

        public override void Write(string value)
        {
            Debug.Write(value);
            base.Write(value);
        }
    }
}
