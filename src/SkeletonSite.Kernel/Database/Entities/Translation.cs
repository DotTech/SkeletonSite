using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SkeletonSite.Kernel.Enumerations;
using NHibernate;

namespace SkeletonSite.Kernel.Database.Entities
{
    public class Translation : BaseEntity<Translation>
    {
        public virtual int Id { get; private set; }
        public virtual string TranslationKey { get; set; }
        public virtual string TranslationNL { get; set; }
        public virtual string TranslationEN { get; set; }

        public Translation()
        {
        }

        /// <summary>
        /// Load translation for specified key.
        /// Creates a new translation if it doesn't exist yet
        /// </summary>
        /// <param name="key">Maximum length for key is 255</param>
        /// <returns></returns>
        public static Translation Load(string key)
        {
            ICriteria criteria = SessionManager.CurrentSession
                .QueryOver<Translation>()
                .Where(x => x.TranslationKey == key)
                .UnderlyingCriteria;

            // Try to load translation from db, but if it doesn't exist return a new translation
            Translation t = ExistsWithCriteria(new Translation { TranslationKey = key, TranslationEN = key, TranslationNL = key }, criteria);

            // If our newly created object was returned, we still need to save it
            if (t.Id == 0)
            {
                t.Save();
                SessionManager.FlushSession();
            }
            
            return t;
        }
    }
}
