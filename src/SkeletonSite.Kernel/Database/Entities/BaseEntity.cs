using System;
using System.Collections;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Criterion;
using System.Linq.Expressions;
using Expression = NHibernate.Criterion.Expression;
using SkeletonSite.Kernel.Logging;
using SkeletonSite.Kernel.Enumerations;
using SkeletonSite.Kernel.OptionModels;

namespace SkeletonSite.Kernel.Database.Entities
{
    /// <summary>
    /// CRUD Operation methods for entity classes
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BaseEntity<T> where T : class
    {
        private static readonly BaseLogger _logger = Logger.GetLogger(typeof(BaseEntity<T>));
        private const string _idField = "Id";


        #region Public methods
        /// <summary>
        /// Load a single T object with specified id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static T Load(int id)
        {
            _logger.Debug("{0}.Load({1})", typeof(T), id);

            var result = SessionManager.CurrentSession.Get<T>(id);
            _logger.Debug((result != null) ? "Found object" : "Object not found");

            return result;
        }


        /// <summary>
        /// Load a single T object with specified criteria
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static T Load(ICriteria criteria)
        {
            _logger.Debug("{0}.Load(criteria) [returns single object]", typeof(T));
            _logger.Debug("criteria: {0}", criteria.ToString());

            var result = criteria.SetMaxResults(1).List<T>();
            if (result.Count > 0)
            {
                _logger.Debug("Found object");
                return result[0];
            }

            // No records matched the criteria
            _logger.Debug("Object not found");
            return null;
        }


        /// <summary>
        /// Loads a list of entities with specified ids
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static IList<T> Load(ICollection ids)
        {
            ICriteria criteria = SessionManager.CurrentSession
                .CreateCriteria(typeof(T))
                .Add(Expression.In(Projections.Id(), ids));

            return criteria.List<T>();
        }


        /// <summary>
        /// Load list of T objects that match the specified criteria with support for paging and sorting
        /// </summary>
        /// <param name="paging"></param>
        /// <param name="sorting"></param>
        /// <param name="query">Query used to retrieve the T objects. If not specified, all objects will be returned</param>
        /// <returns></returns>
        public static List<T> Load(PagingOptions paging, SortingOptions<T> sorting, IQueryOver<T, T> useQuery = null)
        {
            IQueryOver<T, T> query = (useQuery == null) ? SessionManager.CurrentSession.QueryOver<T>() : useQuery;

            // Logging the method call
            _logger.Debug("{0}.Load(paging, sorting, useQuery)", typeof(T));
            _logger.Debug("Paging: DontApplyPaging={0} Page={1} ResultsPerPage={2} ItemsCount={3} PageCount={4}", paging.DontApplyPaging, paging.Page, paging.ResultsPerPage, paging.ItemsCount, paging.PageCount);
            _logger.Debug("Sorting: DontApplySorting={0} OrderBy={1} SortOrder={2}", sorting.DontApplySorting, sorting.OrderBy.Body, sorting.SortOrder);

            // Aplly sorting
            if (!sorting.DontApplySorting)
            {
                var order = query.OrderBy(sorting.OrderBy);
                query = (sorting.SortOrder == SortOrder.Ascending) ? order.Asc : order.Desc;
            }

            // Apply paging
            if (!paging.DontApplyPaging)
            {
                // Update the paging object with the total number of records 
                // so we can determine how many pages there are available
                UpdatePagingOptions((ICriteria)query.UnderlyingCriteria.Clone(), paging);

                // Limit the query results
                query.UnderlyingCriteria
                    .SetFirstResult(paging.ResultsPerPage * paging.Page)
                    .SetMaxResults(paging.ResultsPerPage);
            }

            return (List<T>) query.List<T>();
        }


        /// <summary>
        /// Check if an object of type T with Id exists in the database
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool Exists(int id)
        {
            _logger.Debug("{0}.Exists({1})", typeof(T), id);
            return SessionManager.CurrentSession.Get<T>(id) != null;
        }


        /// <summary>
        /// Executes the specified criteria and returns the first record that is found.
        /// If no records matches the criteria, the supplied object is returned
        /// </summary>
        /// <param name="obj">Object to return if criteria did not return any objects</param>
        /// <param name="criteria"></param>
        /// <returns></returns>
        public static T ExistsWithCriteria(T obj, ICriteria criteria)
        {
            _logger.Debug("{0}.ExistsWithCriteria(obj, criteria)", typeof(T));
            _logger.Debug("criteria: {0}", criteria.ToString());

            var result = criteria.SetMaxResults(1).List<T>();

            // Return first found object
            if (result.Count > 0)
            {
                _logger.Debug("Found existing object");
                return result[0];
            }

            // Return supplied object
            _logger.Debug("Object does not exist yet");
            return obj;
        }


        /// <summary>
        /// Save (insert) this object to the database
        /// </summary>
        /// <returns></returns>
        public virtual object Save()
        {
            if (ReadOnlyMode())
            {
                return this;
            }

            _logger.Debug("{0}.Save()", typeof(T));
            return SessionManager.CurrentSession.Save(this);
        }


        /// <summary>
        /// Save (insert) or update this object to the database
        /// </summary>
        public virtual void SaveOrUpdate()
        {
            if (ReadOnlyMode())
            {
                return;
            }

            _logger.Debug("{0}.SaveOrUpdate()", typeof(T));
            SessionManager.CurrentSession.SaveOrUpdate(this);
        }


        /// <summary>
        /// Update this object in the database
        /// </summary>
        public virtual void Update()
        {
            if (ReadOnlyMode())
            {
                return;
            }

            _logger.Debug("{0}.Update()", typeof(T));
            SessionManager.CurrentSession.Update(this);
        }


        /// <summary>
        /// Delete this object from the database
        /// </summary>
        public virtual void Delete()
        {
            if (ReadOnlyMode())
            {
                return;
            }

            _logger.Debug("{0}.Delete()", typeof(T));
            SessionManager.CurrentSession.Delete(this);
        }
        #endregion


        #region Private methods
        /// <summary>
        /// Check wether or not the site is running in ReadOnlyMode
        /// </summary>
        /// <returns></returns>
        private static bool ReadOnlyMode()
        {
            // During maintenance we can set the site to read-only so the database will not be modified
            if (Configuration.System.ReadOnlyMode)
            {
                _logger.Debug("Save/Update/Delete operation ignored because system is running in read-only mode");
                string message = Dictionary.Translate(SystemMessages.RunningInReadOnlyMode);

                if (!SessionManager.SessionNotifications.Exists(x => x == message))
                {
                    SessionManager.SessionNotifications.Add(message);
                }
                return true;
            }
            return false;
        }


        /// <summary>
        /// Performs a count over the query and sets the paging.ItemsCount property to the total number of records
        /// </summary>
        /// <param name="query"></param>
        /// <param name="pagingOptions"></param>
        private static void UpdatePagingOptions(ICriteria criteria, PagingOptions paging)
        {
            criteria.SetFirstResult(0).SetMaxResults(-1).SetProjection(Projections.CountDistinct(_idField)).ClearOrders();
            paging.ItemsCount = (int)(criteria.UniqueResult() ?? 0);

            _logger.Debug("UpdatePagingOptions(): paging.ItemsCount={0}", paging.ItemsCount);
        }
        #endregion
    }
}
