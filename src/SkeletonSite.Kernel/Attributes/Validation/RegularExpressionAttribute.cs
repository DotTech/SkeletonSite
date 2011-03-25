using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace SkeletonSite.Kernel.Attributes.Validation
{
    /// <summary>
    /// Custom implementation of RegularExpressionAttribute that applies translation to the error message
    /// </summary>
    public class RegularExpressionAttribute : System.ComponentModel.DataAnnotations.RegularExpressionAttribute, IClientValidatable
    {
        public RegularExpressionAttribute(string pattern) : base(pattern) 
        { 
        }

        public override string FormatErrorMessage(string name)
        {
            return Dictionary.Translate(base.FormatErrorMessage(name));
        }

        #region IClientValidatable Members
        /// <summary>
        /// This implementation enables client-side validation
        /// </summary>
        /// <param name="metadata"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            return new ModelClientValidationRegexRule[] { new ModelClientValidationRegexRule(Dictionary.Translate(base.ErrorMessage), base.Pattern) };
        }
        #endregion
    }
}
