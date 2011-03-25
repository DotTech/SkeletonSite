using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SkeletonSite.Kernel.Enumerations;
using SkeletonSite.Kernel.Database.Entities;
using SkeletonSite.Kernel.Logging;

namespace SkeletonSite.Kernel
{
    /// <summary>
    /// Handles translation of string.
    /// Active language is managed from SessionManager.
    /// </summary>
    public static class Dictionary
    {
        private static readonly BaseLogger _logger = Logger.GetLogger(typeof(Dictionary));
        private const string SystemMessagePrefix = "SystemMessage: ";

        /// <summary>
        /// Returns translation for sepcified SystemMessage
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string Translate(SystemMessages key)
        {
            _logger.Debug("Translate(SystemMessages.{0})", key);
            return Translate(SystemMessagePrefix + key.ToString());
        }

        /// <summary>
        /// Returns translation for specified ky
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string Translate(string key)
        {
            _logger.Debug("Translate(\"{0}\")", key);

            // Load translation from database
            var translation = Translation.Load(key);

            // Return translation for active language
            switch (SessionManager.ActiveLanguage)
            {
                case Languages.Dutch:
                    _logger.Debug("Returned \"{0}\"", translation.TranslationNL);
                    return translation.TranslationNL;
                case Languages.English:
                    _logger.Debug("Returned \"{0}\"", translation.TranslationEN);
                    return translation.TranslationEN;
            }

            // If we get here it means there is not translation implemented for the active language
            _logger.Error("Translation not implemented for active language ({0})", SessionManager.ActiveLanguage);
            return translation.TranslationEN;
        }
    }
}
