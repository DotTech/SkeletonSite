using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using SkeletonSite.Kernel.Logging;
using SkeletonSite.Kernel;
using SkeletonSite.Mvc.Logic;
using SkeletonSite.Kernel.Enumerations;
using SkeletonSite.Mvc.Models;
using SkeletonSite.Kernel.Database;

namespace SkeletonSite.Mvc.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return RedirectToAction("Login");
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginModel model)
        {
            return View();
        }

        /// <summary>
        /// Set language cookie with specified language
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SetLanguage(Languages language)
        {
            SessionManager.ActiveLanguage = language;
            return Json(true);
        }


        /// <summary>
        /// Use this method when you run the solution for the first time to create the database
        /// </summary>
        /// <returns></returns>
        public ActionResult CreateDatabase()
        {
            Export.ExportObjectModel(true, false);
            return View();
        }

        /// <summary>
        /// Execute system tests
        /// </summary>
        /// <returns></returns>
        public ActionResult Testing()
        {
            Logic.Testing.Logging.TestLogging();
            Logic.Testing.DataAccess.TestCrud();
            Logic.Testing.DataAccess.LoadList();
            //Logic.Testing.DataAccess.ExportSchema();

            // Service will not work, it is only an example of how to implement one
            //Logic.Testing.ExampleService.TestConnection();

            return View();
        }
    }
}
