using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Cfg;
using SkeletonSite.Kernel.Database.Mappings;
using NHibernate.Tool.hbm2ddl;
using System.IO;
using SkeletonSite.Kernel.Logging;

namespace SkeletonSite.Kernel.Database
{
    public static class Export
    {
        private static readonly BaseLogger _logger = Logger.GetLogger(typeof(Export));

        /// <summary>
        /// Exports the object model to the database schema
        /// </summary>
        /// <param name="updateDatabase">If true the schema is committed to the database</param>
        /// <param name="saveScript">If true saves the update script to a file</param>
        /// <param name="scriptFile">Path of the file to save the script to</param>
        /// <returns>true if export succeeded</returns>
        public static bool ExportObjectModel(bool updateDatabase, bool saveScript, string scriptFile = null)
        {
            _logger.Debug("ExportObjectModel(updateDatabase:{0}, saveScript:{1}, scriptFile:{2})", updateDatabase, saveScript, scriptFile);

            if (!updateDatabase && !saveScript)
            {
                _logger.Error("ExportObjectModel() requires either updateDatabase or saveScript to be true");
                return false;
            }
            else if (updateDatabase && !Configuration.Database.AllowSchemaUpdate)
            {
                _logger.Error("Update of database schema is prevented by configuration (Configuration.Database.AllowSchemaUpdate=false)");
                return false;
            }

            try
            {
                // Load NHibernate configuration
                FluentConfiguration config = Fluently.Configure()
                    .Database(Configuration.Database.NHibernateConfiguration())
                    .Mappings(m => m.FluentMappings.AddFromAssemblyOf<BaseMapping<Object>>());

                // Setup SchemaExport
                var export = new SchemaExport(config.BuildConfiguration());
                if (!String.IsNullOrEmpty(scriptFile))
                {
                    export.SetOutputFile(scriptFile);
                }

                // Execute
                export.Execute(saveScript, updateDatabase, false);
                _logger.Debug("ExportObjectModel succeeded");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error("ExportObjectModel failed", ex);
                return false;
            }
        }
    }
}
