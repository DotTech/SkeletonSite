using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SkeletonSite.Kernel;
using SkeletonSite.Kernel.Enumerations;
using SkeletonSite.Kernel.Attributes.Validation;

namespace SkeletonSite.Mvc.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Must be at least 3 characters")]
        public string Username { get; set; }
        
        [Required(ErrorMessage = "Password is required")]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "Must be at least 6 characters")]
        public string Password { get; set; }

        public LoginModel()
        {
        }
    }
}
