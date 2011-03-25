using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkeletonSite.Kernel.Enumerations
{
    /// <summary>
    /// System messages that can be returned by the application.
    /// They are always processed by Dictionary.Translate() before being displayed to the user
    /// </summary>
    public enum SystemMessages
    {
        RunningInReadOnlyMode,
        PageTitleFormat,
        ErrorLoginIsRequired,
        ErrorPasswordIsRequired
    }
}
