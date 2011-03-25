using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace SkeletonSite.Kernel.Attributes.Validation
{
    /// <summary>
    /// Custom implementation of RangeAttribute that applies translation to the error message
    /// </summary>
    public class RangeAttribute : System.ComponentModel.DataAnnotations.RangeAttribute, IClientValidatable
    {
        public RangeAttribute(double minimum, double maximum): base(minimum, maximum)
        {
        }

        public RangeAttribute(int minimum, int maximum): base(minimum, maximum)
        {
        }

        public RangeAttribute(Type type, string minimum, string maximum): base(type, minimum, maximum)
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
            return new ModelClientValidationRangeRule[] { new ModelClientValidationRangeRule(Dictionary.Translate(base.ErrorMessage), base.Minimum, base.Maximum) };
        }
        #endregion
    }
}
