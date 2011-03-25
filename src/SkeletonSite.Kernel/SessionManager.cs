using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NHibernate;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using SkeletonSite.Kernel.Database.Entities;
using SkeletonSite.Kernel.Database.Mappings;
using System.Web;
using System.Collections.Specialized;
using SkeletonSite.Kernel.Logging;
using System.Web.Security;
using SkeletonSite.Kernel.Enumerations;

namespace SkeletonSite.Kernel
{
    /// <summary>
    /// Manager for NHibernate session and HTTP session
    /// </summary>
    public static class SessionManager
    {
        private static readonly BaseLogger _logger = Logger.GetLogger(typeof(SessionManager));

        private const string keySessionErrors = "SessionErrors";
        private const string keySessionNotifications = "SessionNotifications";
        private const string keyNHibernateSession = "NHibernateSession";
        private const string keyAuthentication = "Authentication";
        private const string keyLanguage = "Language";

        private static ISessionFactory _currentFactory;
        private static ISession _currentSession;
        private static List<Exception> _exceptions;
        private static List<string> _notifications;

        #region Properties
        /// <summary>
        /// Contains current HttpContext object. Only works when called from web application
        /// </summary>
        public static HttpContext CurrentHttpContext
        {
            get
            {
                return HttpContext.Current;
            }
        }

        /// <summary>
        /// Current NHibernate factory for this application instance
        /// </summary>
        public static ISessionFactory CurrentFactory 
        {
            get
            {
                if (_currentFactory == null)
                {
                    CreateFactory();
                }
                return _currentFactory;
            }
            set { _currentFactory = value; }
        }
        
        /// <summary>
        /// Returns the currently NHibernate session.
        /// We need a session before we can use objects derived from EntityBase<>
        /// If current session was not created yet or is closed, a new one will be started
        /// </summary>
        public static ISession CurrentSession 
        {
            get 
            {
                if (HttpContext.Current != null)
                {
                    // If the getter was called from a web application,
                    // the active NHibernate session is stored in the HttpContext
                    ISession session = (ISession)HttpContext.Current.Items[keyNHibernateSession];
                    if (session != null && session.IsOpen)
                    {
                        return (ISession)HttpContext.Current.Items[keyNHibernateSession];
                    }
                    else
                    {
                        // Session not found or closed, start a new one
                        _logger.Error("NHibernate session object was requested, but it was not found in HttpContext or is closed. Starting new session");
                        StartSession();
                        return CurrentSession;
                    }
                }
                else
                {
                    // The getter was not called by a web application,
                    // in that case it's stored in a static var.
                    if (_currentSession != null && _currentSession.IsOpen)
                    {
                        return _currentSession;
                    }
                    else
                    {
                        // Session not found or closed, start a new one
                        _logger.Error("NHibernate session object was requested, but it was created yet or is closed. Starting new session");
                        StartSession();
                        return CurrentSession;
                    }
                }
            }
            set 
            {
                if (HttpContext.Current != null)
                {
                    HttpContext.Current.Items[keyNHibernateSession] = value;
                }
                else
                {
                    _currentSession = value;
                }
            }
        }
        
        /// <summary>
        /// Contains exceptions that occurred during this session and should be displayed to the user
        /// </summary>
        public static List<Exception> SessionErrors
        {
            get 
            {
                if (CurrentHttpContext != null && CurrentHttpContext.Session != null)
                {
                    if (CurrentHttpContext.Session[keySessionErrors] == null)
                    {
                        CurrentHttpContext.Session[keySessionErrors] = new List<Exception>();
                    }
                    return (List<Exception>)CurrentHttpContext.Session[keySessionErrors];
                }
                else
                {
                    if (_exceptions == null)
                    {
                        _exceptions = new List<Exception>();
                    }
                    return _exceptions;
                }
            }
        }
        
        /// <summary>
        /// Contains notifications that occurred during this session and should be displayed to the user
        /// </summary>
        public static List<string> SessionNotifications
        {
            get
            {
                if (CurrentHttpContext != null && CurrentHttpContext.Session != null)
                {
                    if (CurrentHttpContext.Session[keySessionNotifications] == null)
                    {
                        CurrentHttpContext.Session[keySessionNotifications] = new List<string>();
                    }
                    return (List<string>)CurrentHttpContext.Session[keySessionNotifications];
                }
                else
                {
                    if (_notifications == null)
                    {
                        _notifications = new List<string>();
                    }
                    return _notifications;
                }
            }
        }

        /// <summary>
        /// Indicates wether or not user is logged in as admin
        /// </summary>
        public static bool IsAdmin
        {
            get
            {
                /*if (CurrentHttpContext != null && CurrentHttpContext.Request.Cookies["adminloggedin"] != null)
                {
                    return Convert.ToBoolean(CurrentHttpContext.Request.Cookies["adminloggedin"].Value);
                }*/
                return false;
            }
            set
            {
                HttpCookie cookie = new HttpCookie("adminloggedin", value.ToString());
                cookie.Expires = DateTime.Now.AddDays(1);
                CurrentHttpContext.Response.SetCookie(cookie);
            }
        }

        /// <summary>
        /// Set or get active language. Value is stored in cookie for 365 days
        /// </summary>
        public static Languages ActiveLanguage
        {
            get
            {
                Languages result = Configuration.System.DefaultLanguage;

                // If property was called from non-web application or cookie could not be found, return default language
                if (CurrentHttpContext == null || CurrentHttpContext.Request == null 
                    || CurrentHttpContext.Request.Cookies == null || CurrentHttpContext.Request.Cookies[keyLanguage] == null)
                {
                    _logger.Debug("Language cookie could not be found");
                    ActiveLanguage = result;
                }
                // Try to parse language value from cookie
                else if (!Enum.TryParse<Languages>(CurrentHttpContext.Request.Cookies[keyLanguage].Value, out result))
                {
                    _logger.Error("Language cookie value could not be parsed to enumerator value");
                }

                _logger.Debug("Returned active language: {0}", result);
                return result;
            }
            set
            {
                _logger.Debug("Set language cookie to {0}", value);
                SetCookie(keyLanguage, value.ToString(), 365);
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Create a new NHibernate factory and save reference in CurrentFactory property
        /// </summary>
        /// <returns></returns>
        public static void CreateFactory()
        {
            _logger.Debug("Creating NHibernate session factory");

            try
            {
                // Create and configure factory
                ISessionFactory factory = Fluently.Configure()
                    .Database(Configuration.Database.NHibernateConfiguration())
                    .Mappings(m => m.FluentMappings.AddFromAssemblyOf<BaseMapping<Object>>())
                    .BuildSessionFactory();
                CurrentFactory = factory;
            }
            catch (FluentConfigurationException ex)
            {
                _logger.Error("Fatal exception occured while creating NHibernate session factory", ex);                
                throw;
            }
        }

        /// <summary>
        /// Close the current factory
        /// </summary>
        public static void CloseFactory()
        {
            if (CurrentFactory != null)
            {
                _logger.Debug("Closing NHibernate session factory");
                CurrentFactory.Close();
                CurrentFactory = null;
            }
        }

        /// <summary>
        /// Open a new NHibernate session and save reference in CurrentSession property
        /// </summary>
        /// <param name="filters">Session filters</param>
        public static void StartSession()
        {
            _logger.Debug("Starting NHibernate session");
            CurrentSession = CurrentFactory.OpenSession();
            CurrentSession.FlushMode = NHibernate.FlushMode.Never;  // TODO: Think if this is wanted
        }

        /// <summary>
        /// Begins NHibernate transaction.
        /// </summary>
        /// <returns></returns>
        public static void BeginTransaction()
        {
            _logger.Debug("Begin NHibernate transaction.");
            CurrentSession.BeginTransaction();
        }

        /// <summary>
        /// Commit NHibernate transaction.
        /// </summary>
        public static void CommitTransaction()
        {
            _logger.Debug("Commit NHibernate transaction.");
            if (CurrentSession != null && CurrentSession.Transaction.IsActive)
            {
                CurrentSession.Flush();
                CurrentSession.Transaction.Commit();
            }
        }

        /// <summary>
        /// Rollback NHibernate transaction.
        /// </summary>
        public static void RollbackTransaction()
        {
            _logger.Debug("Rollback NHibernate transaction.");
            if (CurrentSession != null && CurrentSession.Transaction.IsActive)
            {
                CurrentSession.Transaction.Rollback();
                CurrentSession.Clear();
            }
        }

        /// <summary>
        /// Close current session
        /// </summary>
        public static void CloseSession()
        {
            _logger.Debug("Closing NHibernate session");
            if (CurrentSession != null)
            {
                CurrentSession.Close();
                CurrentSession = null;
            }
        }

        /// <summary>
        /// Flushes current nhibernate session
        /// </summary>
        public static void FlushSession()
        {
            CurrentSession.Flush();
        }

        /// <summary>
        /// Reset the current session (close and re-open)
        /// </summary>
        public static void ResetSession()
        {
            _logger.Debug("Reset NHibernate session");
            CloseSession();
            StartSession();
        }

        /// <summary>
        /// Adds error to <see cref="SessionErrors"/>.
        /// </summary>
        public static void AddError(string message)
        {
            AddError(new CustomException(message));
        }

        /// <summary>
        /// Adds error to <see cref="SessionErrors"/>.
        /// </summary>
        public static void AddError(Exception ex)
        {
            SessionErrors.Add(ex);
        }

        /// <summary>
        /// Adds notification to <see cref="SessionNotifications"/>.
        /// </summary>
        public static void AddNotification(string message)
        {
            SessionNotifications.Add(message);
        }

        /// <summary>
        /// Create authentication ticket and store it in a cookie
        /// </summary>
        public static void SetAuthenticationCookie(string authenticationHash)
        {
            FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(1,
                                                                                 authenticationHash,
                                                                                 DateTime.Now,
                                                                                 DateTime.Now.AddMinutes(Configuration.System.AuthenticationExpiration),
                                                                                 false,
                                                                                 String.Empty,
                                                                                 FormsAuthentication.FormsCookiePath);

            string encrAuthTicket = FormsAuthentication.Encrypt(authTicket);
            HttpCookie authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrAuthTicket);
            
            CurrentHttpContext.Response.Cookies.Add(authCookie);
            CurrentHttpContext.Response.Cookies[FormsAuthentication.FormsCookieName].Domain = "." + Configuration.System.Domain;
            CurrentHttpContext.Response.Cookies[FormsAuthentication.FormsCookieName].Expires = DateTime.Now.AddMinutes(Configuration.System.AuthenticationExpiration);
        }

        /// <summary>
        /// Remove the authentication cookie
        /// </summary>
        public static void RemoveAuthenticationCookie()
        {
            CurrentHttpContext.Response.Cookies.Remove(FormsAuthentication.FormsCookieName);
        }

        /// <summary>
        /// true if current request is coming from a search engine
        /// </summary>
        /// <returns></returns>
        public static bool IsSearchEngineRequest()
        {
            if (!String.IsNullOrEmpty(CurrentHttpContext.Request.UserAgent) && CurrentHttpContext.Request.UserAgent.ToLower().Contains("bot"))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Sets cookie for web app domain for path /
        /// </summary>
        /// <param name="cookieName"></param>
        /// <param name="cookieValue"></param>
        /// <param name="expires">value in days</param>
        public static void SetCookie(string cookieName, string cookieValue, int expires)
        {
            _logger.Debug("SetCookie(cookieName: \"{0}\", cookieValue, expires: {1})", cookieName, expires);
            _logger.Debug("cookieValue (w/o linefeeds): {0}", cookieValue.Replace("\r", "").Replace("\n", ""));

            if (CurrentHttpContext != null && CurrentHttpContext.Response != null && CurrentHttpContext.Response.Cookies != null)
            {
                HttpCookie cookie = new HttpCookie(cookieName, cookieValue);
                cookie.Expires = DateTime.Now.AddDays(expires);
                cookie.Domain = Configuration.System.Domain;
                cookie.Path = "/";
                CurrentHttpContext.Response.SetCookie(cookie);
            }
            else
            {
                _logger.Error("Could not access HttpContext or Cookie collection");
            }
        }
        #endregion
    }
}
