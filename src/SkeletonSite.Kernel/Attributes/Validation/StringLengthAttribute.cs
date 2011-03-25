using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace SkeletonSite.Kernel.Attributes.Validation
{
    /// <summary>
    /// Custom implementation of StringLengthAttribute that applies translation to the error message
    /// </summary>
    public class StringLengthAttribute : System.ComponentModel.DataAnnotations.StringLengthAttribute, IClientValidatable
    {
        public StringLengthAttribute(int maximumLength) : base(maximumLength)
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
        public IEnumerable<ModelClientValidationRule>  GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            return new ModelClientValidationStringLengthRule[] { new ModelClientValidationStringLengthRule(Dictionary.Translate(base.ErrorMessage), base.MinimumLength, base.MaximumLength) };
        }
        #endregion
    }
}
