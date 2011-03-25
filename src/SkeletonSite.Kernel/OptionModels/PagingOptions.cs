using System;

namespace SkeletonSite.Kernel.OptionModels
{
    public class PagingOptions
    {
        /// <summary>
        /// Get or set current page (zero-based index)
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Gets or sets total number of items
        /// </summary>
        public int ItemsCount { get; set; }
        
        /// <summary>
        /// Gets or sets number of results per page
        /// </summary>
        public int ResultsPerPage { get; set; }
        
        /// <summary>
        /// Gets total number of pages
        /// </summary>
        public int PageCount 
        { 
            get 
            {
                if (ResultsPerPage == 0 || ResultsPerPage == 0)
                {
                    return 0;
                }
                return ItemsCount / ResultsPerPage + (ItemsCount % ResultsPerPage == 0 ? 0 : 1); 
            }
        }

        /// <summary>
        /// Set to true of paging should not be applied at all
        /// </summary>
        public bool DontApplyPaging { get; set; }
        
        /// <summary>
        /// Returns a PagingOptions object indicating that no paging should be applied
        /// </summary>
        public static PagingOptions NoPaging
        {
            get
            {
                return new PagingOptions { DontApplyPaging = true };
            }
        }
    }
}
