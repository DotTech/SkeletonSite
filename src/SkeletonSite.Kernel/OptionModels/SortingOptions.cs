using System;
using System.Linq.Expressions;
using SkeletonSite.Kernel.Enumerations;

namespace SkeletonSite.Kernel.OptionModels
{
    public class SortingOptions<T>
    {
        /// <summary>
        /// Property that we want to sort on
        /// </summary>
        public Expression<Func<T, object>> OrderBy { get; set; }

        /// <summary>
        /// Direction we want to sort in
        /// </summary>
        public SortOrder SortOrder { get; set; }

        /// <summary>
        /// Set to true of sorting should not be applied at all
        /// </summary>
        public bool DontApplySorting { get; set; }

        public SortingOptions()
        {
        }

        public SortingOptions(Expression<Func<T, object>> orderBy)
        {
            OrderBy = orderBy;
        }

        public SortingOptions<T> Asc()
        {
            SortOrder = Enumerations.SortOrder.Ascending;
            return this;
        }

        public SortingOptions<T> Desc()
        {
            SortOrder = Enumerations.SortOrder.Descending;
            return this;
        }

        /// <summary>
        /// Returns a SortingOptions objects indicating that no sorting should be applied
        /// </summary>
        public static SortingOptions<T> NoSorting
        {
            get
            {
                return new SortingOptions<T> { DontApplySorting = true };
            }
        }
    }
}
