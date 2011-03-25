using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SkeletonSite.Kernel;
using SkeletonSite.Kernel.Enumerations;
using SkeletonSite.Kernel.Logging;

namespace SkeletonSite.Mvc.Logic
{
    /// <summary>
    /// Custom implementation of WebViewPage so we can write our own view helper methods
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    public abstract class BaseViewPage<TModel> : System.Web.Mvc.WebViewPage<TModel> where TModel : class
    {
        private static readonly BaseLogger _logger = Logger.GetLogger(typeof(BaseViewPage<TModel>));

        /// <summary>
        /// Shortcut to Kernel.Dictionary.Translate()
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string Translate(string key)
        {
            return Dictionary.Translate(key);
        }

        /// <summary>
        /// Shortcut to Kernel.Dictionary.Translate()
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string Translate(SystemMessages key)
        {
            return Dictionary.Translate(key);
        }

        /// <summary>
        /// Returns page title in active language for specified page 
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public string TitleFor(string pageName)
        {
            string titleFormat = Translate(SystemMessages.PageTitleFormat);
            string title = Translate(pageName);

            return String.Format(titleFormat, title);
        }

        /// <summary>
        /// Set the page title for current view
        /// </summary>
        /// <param name="pageName"></param>
        public void SetTitle(string pageName)
        {
            _logger.Debug("Set page title for \"{0}\"", pageName);
            ViewBag.Title = TitleFor(pageName);
        }
    }
}
