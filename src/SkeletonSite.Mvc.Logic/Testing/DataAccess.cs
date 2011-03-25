using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SkeletonSite.Kernel.Database.Entities;
using SkeletonSite.Kernel;
using SkeletonSite.Kernel.OptionModels;
using SkeletonSite.Kernel.Logging;
using SkeletonSite.Kernel.Database;
using System.IO;

namespace SkeletonSite.Mvc.Logic.Testing
{
    /// <summary>
    /// Just a class with some methods to test the skeleton site
    /// </summary>
    public static class DataAccess
    {
        private static readonly BaseLogger _logger = Logger.GetLogger(typeof(DataAccess));

        public static void TestCrud()
        {
            _logger.Debug("Test: TestCrud()");
            string name = "Blaaaa " + DateTime.Now.ToString();

            _logger.Debug("Creating new Example() object with name {0}", name);
            var example = new TestObject();
            example.Name = "Blaaaa " + DateTime.Now.ToString();
            example.Save();
            SessionManager.FlushSession();

            _logger.Debug("Load Example object with name {0}", name);
            example = TestObject.Load(SessionManager.CurrentSession.QueryOver<TestObject>().Where(x => x.Name == name).UnderlyingCriteria);
            _logger.Debug((example != null) ? String.Format("Found example object (id={0})", example.Id) : "Could not find example object");

            if (example != null)
            {
                _logger.Debug("Update Example object with id {0}", example.Id);
                example.Name = "Bla bla updated !!!";
                example.Update();
                SessionManager.FlushSession();

                _logger.Debug("Load Example object with id {0}", example.Id);
                example = TestObject.Load(example.Id);
                _logger.Debug("Example.Name = {0}", example.Name);
            }
            
            for (int i = 0; i < 2; i++)
            {
                _logger.Debug("Checking for existing Example() object with name 'BAAA'");
                example = TestObject.ExistsWithCriteria(new TestObject { Name = "BAAA" },
                                                     SessionManager.CurrentSession.QueryOver<TestObject>().Where(x => x.Name == "BAAA").UnderlyingCriteria);
                example.SaveOrUpdate();
                SessionManager.FlushSession();

                if (i == 0) _logger.Debug("Now do it again!");
            }

            int id = example.Id;
            _logger.Debug("Delete the BAAA record (id={0})", id);
            example.Delete();
            SessionManager.FlushSession();

            _logger.Debug("Check if record with id {0} exists", id);
            _logger.Debug("Example.Exists({0}) = {1}", id, TestObject.Exists(id));

        }

        public static void LoadList()
        {
            _logger.Debug("Test: LoadList()");

            var sorting = new SortingOptions<TestObject>(x => x.Name).Asc();
            var paging = new PagingOptions { ResultsPerPage = 5, Page = 0 };

            _logger.Debug("Loading page 1");
            var list = TestObject.Load(paging, sorting);
            foreach (var i in list)
            {
                _logger.Debug("Record {0}: {1}", i.Id, i.Name);
            }

            if (paging.PageCount > 1)
            {
                _logger.Debug("Loading page 2");
                paging.Page = 1;
                list = TestObject.Load(paging, sorting);
                foreach (var i in list)
                {
                    _logger.Debug("Record {0}: {1}", i.Id, i.Name);
                }
            }
        }

        public static void ExportSchema()
        {
            _logger.Debug("Test: ExportSchema()");

            string scriptFile = Path.Combine(Configuration.System.TempFolder, "export_schema.sql");
            Export.ExportObjectModel(true, true, scriptFile);
        }
    }
}
