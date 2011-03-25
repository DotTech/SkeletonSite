using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using SkeletonSite.Kernel;
using SkeletonSite.Kernel.Logging;

namespace SkeletonSite
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        private static readonly BaseLogger _logger = Logger.GetLogger(typeof(MvcApplication));

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute("Default", "{controller}/{action}/{id}", new { controller = "Home", action = "Index", id = UrlParameter.Optional });

        }

        protected void Application_Start()
        {
            _logger.Debug("Application_Start()");
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            SessionManager.CreateFactory();
        }

        protected void Application_End()
        {
            _logger.Debug("Application_End()");
            SessionManager.CloseFactory();
        }

        protected void Application_BeginRequest()
        {
            _logger.Debug("Application_BeginRequest()");

            // Start NHibernate session and transaction
            SessionManager.StartSession();
            SessionManager.BeginTransaction();
        }

        protected void Application_EndRequest()
        {
            _logger.Debug("Application_EndRequest()");

            // Commit transaction, flush and close session
            SessionManager.CommitTransaction();
            SessionManager.FlushSession();
            SessionManager.CloseSession();
        }

        protected void Application_Error(Object sender, EventArgs e)
        {
            // TODO: Update application error handling
            /*
            #if !DEBUG
            string errorEventLogName = "Application";
            string errorEventSourceName = Configuration.Solution + ".Error";
            string pageNotFoundEventLogName = "PageNotFound";
            string pageNotFoundEventSourceName = Configuration.Solution + ".404";
            
            Exception exception = HttpContext.Current.Server.GetLastError();

            // Get local address info to identify the server
            var localAddress = (from ni in NetworkInterface.GetAllNetworkInterfaces()
                                where ni.NetworkInterfaceType != NetworkInterfaceType.Loopback
                                let props = ni.GetIPProperties()
                                from ipAddress in props.UnicastAddresses
                                select ipAddress).FirstOrDefault();

            // Error info for error page
            string errorPageInfo =
               "<br/>URL: " + HttpContext.Current.Request.Url.ToString() +
               "<br/>Tijd: " + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss") +
               "<br/>Server: " + SystemInformation.ComputerName +
               "<br/>Message: " + exception.Message;

            // Error info for windows eventlog
            string errorLogInfo =
               "\r\nURL: " + HttpContext.Current.Request.Url.ToString() +
               "\r\nUser: " + HttpContext.Current.User.Identity.Name +
               "\r\nTime: " + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss") +
               "\r\nReferer: " + ((HttpContext.Current.Request.UrlReferrer != null) ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : "") +
               "\r\nUserAgent: " + HttpContext.Current.Request.UserAgent +
               "\r\nClient IP: " + HttpContext.Current.Request.UserHostAddress +
               "\r\nServer: " + SystemInformation.ComputerName +
               "\r\nResponse code: {0}" + 
               "\r\n" +
               "\r\nSource: " + exception.Source +
               "\r\nMessage: " + exception.Message +
               "\r\nStacktrace: " + exception.StackTrace;

            HttpContext.Current.Session["errorinfo"] = errorPageInfo;
            HttpContext.Current.Server.ClearError();

            string message = exception.Message.ToLower();
            string childActionExceptionMessage = "Error executing child request for handler 'System.Web.Mvc.HttpHandlerUtil+ServerExecuteHttpHandlerAsyncWrapper'";
            if (exception.Message.Contains(childActionExceptionMessage) && exception.InnerException != null)
            {
                message = exception.InnerException.Message.ToLower();
            }

            // If NHibernate could not find a record, we return a 404 error instead of 500
            if (message.ToLower().Contains("no row with the given identifier exists")           // Record not found
                || message.ToLower().Contains("not found or does not implement icontroller")    // Nonexisting controller called
                || message.ToLower().Contains("was not found on controller")                    // Inaccessible or invalid action called
                || message.ToLower().Contains("the result isn't viewresultbase")                // Uncategorized product called
                || message.ToLower().Contains("sorting is not implemented")                     // Nonexisting sorting option called
                || message.ToLower().Contains("no level 4 or level 3 category found"))          // Uncategorized product called
            {
                // Create connection to custom PageNotFound eventlog
                EventLog pageNotFoundEventLog = new EventLog(pageNotFoundEventLogName);
                pageNotFoundEventLog.Source = pageNotFoundEventSourceName;

                if (!EventLog.SourceExists(pageNotFoundEventSourceName))
                {
                    EventLog.CreateEventSource(pageNotFoundEventSourceName, pageNotFoundEventLogName);
                }

                errorLogInfo += InnerExceptionMessages(exception, "\r\nInner exception message (depth level {0}): {1}");
                pageNotFoundEventLog.WriteEntry(String.Format(errorLogInfo, 404), EventLogEntryType.Warning);
                HttpContext.Current.Response.Status = "404 Not Found";
            }
            else
            {
                // Create connection to windows application eventlog
                EventLog errorEventLog = new EventLog(errorEventLogName);
                errorEventLog.Source = errorEventSourceName;

                if (!EventLog.SourceExists(errorEventSourceName))
                {
                    EventLog.CreateEventSource(errorEventSourceName, errorEventLogName);
                }

                // Add inner exception messages 
                errorLogInfo += InnerExceptionMessages(exception, "\r\nInner exception message (depth level {0}): {1}");
                errorEventLog.WriteEntry(String.Format(errorLogInfo, 500), EventLogEntryType.Error);
                HttpContext.Current.Response.Status = "500 Internal Server Error";
            }
            #endif
            */
        }

        /// <summary>
        /// Returns string with inner exception messages and keeps adding child inner exception messages until maxDepth recursion is reached
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="template"></param>
        /// <param name="maxDepth"></param>
        /// <returns></returns>
        private string InnerExceptionMessages(Exception ex, string template, int maxDepth = 5, int iteration = 0)
        {
            string result = String.Empty;

            if (ex.InnerException != null && iteration < maxDepth)
            {
                result = String.Format(template, iteration, ex.InnerException.Message);
                result += InnerExceptionMessages(ex.InnerException, template, maxDepth, iteration + 1);
            }

            return result;
        }
    }
}