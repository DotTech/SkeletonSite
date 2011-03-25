using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Xml;
using System.IO;
using SkeletonSite.Kernel.Logging;
using System.Text;
using FluentNHibernate.Cfg.Db;
using SkeletonSite.Kernel.Enumerations;

namespace SkeletonSite.Kernel
{
    public static class Configuration
    {
        public const string Solution = "SkeletonSite";
        private static BaseLogger _logger;

        #region Configuration definition
        public class DatabaseConfiguration
        {
            public string ConnectionString { get; set; }
            public string ConnectionDriverClass { get; set; }
            public string Dialect { get; set; }
            public string ConnectionProvider { get; set; }
            public string FactoryClass { get; set; }
            public bool ShowSql { get; set; }
            public bool FormatSql { get; set; }
            public bool AllowSchemaUpdate { get; set; }

            public MsSqlConfiguration NHibernateConfiguration()
            {
                var database = MsSqlConfiguration
                    .MsSql2008
                    .ConnectionString(Configuration.Database.ConnectionString)
                    .Driver(Configuration.Database.ConnectionDriverClass)
                    .Dialect(Configuration.Database.Dialect)
                    .Provider(Configuration.Database.ConnectionProvider)
                    .ProxyFactoryFactory(Configuration.Database.FactoryClass);

                if (Configuration.Database.ShowSql) database.ShowSql();
                if (Configuration.Database.FormatSql) database.FormatSql();

                return database;
            }
        }

        public class LoggingConfiguration
        {
            public class Logger
            {
                public Type LoggerType { get; set; }
                public LogLevels Level { get; set; }
                public string LogFormat { get; set; }
                public Dictionary<string, string> CustomSettings { get; set; }
            }

            public bool Enabled { get; set; }
            public List<Logger> Loggers { get; set; }
        }

        public class SystemConfiguration
        {
            public string Domain { get; set; }
            public double AuthenticationExpiration { get; set; }
            public bool ReadOnlyMode { get; set; }
            public string TempFolder { get; set; }
            public bool DebugMode { get; set; }
            public Languages DefaultLanguage { get; set; }
        }
        #endregion

        #region Private fields containing loaded configuration data
        private static DatabaseConfiguration _database;
        private static LoggingConfiguration _logging;
        private static SystemConfiguration _system;
        #endregion

        #region Public properties containing configuration objects
        /// <summary>
        /// Configuration regarding database connections and NHibernate
        /// </summary>
        public static DatabaseConfiguration Database
        {
            get
            {
                if (_database == null)
                {
                    _database = new DatabaseConfiguration
                    {
                        ConnectionString = GetSetting<string>("Database/ConnectionString"),
                        ConnectionDriverClass = GetSetting<string>("Database/ConnectionDriverClass"),
                        ConnectionProvider = GetSetting<string>("Database/ConnectionProvider"),
                        Dialect = GetSetting<string>("Database/Dialect"),
                        FactoryClass = GetSetting<string>("Database/FactoryClass"),
                        ShowSql = GetSetting<bool>("Database/ShowSql"),
                        FormatSql = GetSetting<bool>("Database/FormatSql"),
                        AllowSchemaUpdate = GetSetting<bool>("Database/AllowSchemaUpdate")
                    };

                    // Setup a DebugConsoleWrite so NHibernate sql logging will be displayed in the debug console
                    if (_database.ShowSql)
                    {
                        Console.SetOut(new DebugConsoleWriter());
                    }
                }
                return _database;
            }
        }

        /// <summary>
        /// Configuration regarding logging
        /// </summary>
        public static LoggingConfiguration Logging
        {
            get
            {
                if (_logging == null)
                {
                    _logging = new LoggingConfiguration
                    {
                        Enabled = GetSetting<bool>("Logging/Enabled"),
                        Loggers = new List<LoggingConfiguration.Logger>()
                    };

                    // Populate list with Logger configuration objects
                    foreach (XmlElement loggerSettings in ConfigXml.SelectSingleNode(ConfigurationRootElement + "/Logging/Loggers").ChildNodes)
                    {
                        var logger = new LoggingConfiguration.Logger
                        {
                            LoggerType = Type.GetType(GetSetting<string>("LoggerType", loggerSettings)),
                            Level = GetSetting<LogLevels>("Level", loggerSettings),
                            LogFormat = GetSetting<string>("LogFormat", loggerSettings),
                            CustomSettings = new Dictionary<string,string>()
                        };

                        // Populate list with CustomSettings values
                        var customSettings = loggerSettings.SelectSingleNode("CustomSettings");
                        if (customSettings != null && customSettings.HasChildNodes)
                        {
                            foreach (XmlElement customSetting in customSettings.ChildNodes)
                            {
                                logger.CustomSettings.Add(customSetting.Name, customSetting.InnerText);
                            }
                        }
                        _logging.Loggers.Add(logger);
                    }

                    // Enable logging for the rest of the execution of the configuration class
                    _logger = Logger.GetLogger(typeof(Configuration));
                }
                return _logging;
            }
        }

        /// <summary>
        /// Configuration regarding the application
        /// </summary>
        public static SystemConfiguration System
        {
            get
            {
                if (_system == null)
                {
                    _system = new SystemConfiguration
                    {
                        Domain = GetSetting<string>("System/Domain"),
                        AuthenticationExpiration = GetSetting<double>("System/AuthenticationExpiration"),
                        ReadOnlyMode = GetSetting<bool>("System/ReadOnlyMode"),
                        TempFolder = GetSetting<string>("System/TempFolder"),
                        DebugMode = GetSetting<bool>("System/DebugMode"),
                        DefaultLanguage = GetSetting<Languages>("System/DefaultLanguage")
                    };
                }
                return _system;
            }
        }
        #endregion

        #region Configuration logic
        private static XmlDocument _configXml;

        /// <summary>
        /// Direct access to the settings XML file
        /// </summary>
        private static XmlDocument ConfigXml
        {
            get
            {
                if (_configXml == null)
                {
                    LoadConfigXml();
                }
                return _configXml;
            }
        }

        /// <summary>
        /// Determines file path to the configuration file
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static string GetConfigPath(string fileName)
        {
            string configPath = AppDomain.CurrentDomain.BaseDirectory + string.Format(@"..\..\{0}", fileName);
            if (!File.Exists(configPath))
            {
                configPath = AppDomain.CurrentDomain.BaseDirectory + string.Format(@"..\{0}", fileName);
            }
            if (!File.Exists(configPath))
            {
                configPath = AppDomain.CurrentDomain.BaseDirectory + string.Format(@"..\..\..\{0}", fileName);
            }
            if (!File.Exists(configPath))
            {
                throw new FileNotFoundException("File not found", configPath);
            }

            return configPath;
        }

        /// <summary>
        /// Load settings XML file
        /// </summary>
        /// <returns></returns>
        private static bool LoadConfigXml()
        {
            try
            {
                // HACK: Relative path to XML file is different when we run from the Mvc application
                // We just try the first path and if it doesnt work we try the fallback path, if none works we throw an exception
                XmlTextReader xmlReader = new XmlTextReader(GetConfigPath("Configuration.xml"));
                _configXml = new XmlDocument();
                _configXml.Load(xmlReader);
            }
            catch (FileNotFoundException ex)
            {
                throw new CustomException("Configuration file could not be found! Make sure configuraton.xml is located in the root of the solution!", ex);
            }
            catch (Exception ex)
            {
                throw new CustomException("Unknown error occurred while trying to load settings.xml", ex);
            }

            return true;
        }

        /// <summary>
        /// Returns web services configuration
        /// </summary>
        /// <returns>Configuration</returns>
        public static System.Configuration.Configuration GetServicesConfiguration()
        {
            try
            {
                if (_logger != null)
                {
                    _logger.Debug("Configuration.GetServicesConfiguration()");
                }

                ExeConfigurationFileMap executionFileMap = new ExeConfigurationFileMap();
                executionFileMap.ExeConfigFilename = GetConfigPath("Services.xml");

                return ConfigurationManager.OpenMappedExeConfiguration(executionFileMap, ConfigurationUserLevel.None);
            }
            catch (Exception ex)
            {
                if (_logger != null)
                {
                    _logger.Error("Error occured in Configuration.GetServicesConfiguration()", ex);
                }
                throw new CustomException("Error occured in Configuration.GetServicesConfiguration()", ex);
            }
        }

        /// <summary>
        /// Read a specific setting from the configuration Xml
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="elementName"></param>
        /// <returns></returns>
        private static T GetSetting<T>(string elementName)
        {
            return GetSetting<T>(elementName, ConfigXml.SelectSingleNode(ConfigurationRootElement));
        }

        /// <summary>
        /// Read a specific setting from the configuration Xml
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="elementName">Name of Xml element that contains our desired setting</param>
        /// <param name="startNode">Node to start at when selection the element</param>
        /// <returns></returns>
        private static T GetSetting<T>(string elementName, XmlNode startNode)
        {
            try
            {
                if (_logger != null)
                {
                    _logger.Debug("Get configuration setting {0}", elementName);
                }

                string value = startNode.SelectSingleNode(elementName).InnerText;

                if (!typeof(T).IsEnum)
                {
                    // Convert value to desired type
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                else
                {
                    // Desired type of an enumerator, conversion is a little different for that
                    return (T)Enum.Parse(typeof(T), value);
                }
            }
            catch (Exception ex)
            {
                if (_logger != null)
                {
                    _logger.Error("Could not read setting {0}", ex, elementName);
                }
                throw new CustomException(String.Format("Could not read setting {0}", elementName), ex);
            }
        }

        /// <summary>
        /// Start element of configuration for this solution
        /// </summary>
        private static string ConfigurationRootElement
        {
            get
            {
                return String.Format("/Configurations/Configuration[@solution='{0}']", Solution);
            }
        }

        /// <summary>
        /// Returns an overview of all configuration formatted with HTML
        /// Obviously this contains sensitive information, keep that in mind!
        /// </summary>
        /// <returns></returns>
        public static string ConfigurationReport()
        {
            string sectionStart = "<table class=\"boxshadow borderradius\">\r\n";
            string sectionHeader = "<tr><th colspan=\"2\">{0}</th></tr>\r\n";
            string sectionRow = "<tr><td class=\"name\">{0}</td><td class=\"value\">{1}</td></tr>\r\n";
            string sectionEnd = "</table>\r\n";
            var html = new StringBuilder();

            // Database configuration
            html.Append(sectionStart);
            html.AppendFormat(sectionHeader, "Configuration.Database");
            html.AppendFormat(sectionRow, "ConnectionDriverClass", Configuration.Database.ConnectionDriverClass);
            html.AppendFormat(sectionRow, "ConnectionProvider", Configuration.Database.ConnectionProvider);
            html.AppendFormat(sectionRow, "ConnectionString", Configuration.Database.ConnectionString);
            html.AppendFormat(sectionRow, "Dialect", Configuration.Database.Dialect);
            html.AppendFormat(sectionRow, "FactoryClass", Configuration.Database.FactoryClass);
            html.AppendFormat(sectionRow, "FormatSql", Configuration.Database.FormatSql);
            html.AppendFormat(sectionRow, "ShowSql", Configuration.Database.ShowSql);
            html.Append(sectionEnd);

            // System configuration
            html.Append(sectionStart);
            html.AppendFormat(sectionHeader, "Configuration.System");
            html.AppendFormat(sectionRow, "Domain", Configuration.System.Domain);
            html.AppendFormat(sectionRow, "AuthenticationExpiration", Configuration.System.AuthenticationExpiration);
            html.AppendFormat(sectionRow, "ReadOnlyMode", Configuration.System.ReadOnlyMode);
            html.Append(sectionEnd);

            // Logging configuration
            html.Append(sectionStart);
            html.AppendFormat(sectionHeader, "Configuration.Logging");
            html.AppendFormat(sectionRow, "Enabled", Configuration.Logging.Enabled);

            var loggersHtml = new StringBuilder();
            int i = 0;

            foreach (var logger in Configuration.Logging.Loggers)
            {
                loggersHtml.Append(sectionStart);
                loggersHtml.AppendFormat(sectionHeader, String.Format("Configuration.Logging.Loggers.Logger[{0}]", i));
                loggersHtml.AppendFormat(sectionRow, "LoggerType", logger.LoggerType);
                loggersHtml.AppendFormat(sectionRow, "Level", logger.Level);
                loggersHtml.Append(sectionEnd);
                i++;
            }

            html.AppendFormat(sectionRow, "Loggers", loggersHtml);
            html.Append(sectionEnd);

            html.Insert(0, "<div class=\"ccsConfigurationReport\">\r\n");
            html.AppendLine("</div>");

            return html.ToString();
        }
        #endregion
    }
}
