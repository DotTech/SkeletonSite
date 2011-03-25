using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace SkeletonSite.Kernel.Attributes.Validation
{
    /// <summary>
    /// Custom implementation of RequiredAttribute that applies translation to the error message
    /// </summary>
    public class RequiredAttribute : System.ComponentModel.DataAnnotations.RequiredAttribute, IClientValidatable
    {
        public RequiredAttribute(): base()
        {
        }

        public override string FormatErrorMessage(string name)
        {
            return Dictionary.Translate(base.FormatErrorMessage(name));
        }

        #region IClientValidatable Members
        /// <summary>
        /// This implementation enabled client-side validation
        /// </summary>
        /// <param name="metadata"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            return new ModelClientValidationRequiredRule[] { new ModelClientValidationRequiredRule(Dictionary.Translate(base.ErrorMessage)) };
        }
        #endregion
    }
}
