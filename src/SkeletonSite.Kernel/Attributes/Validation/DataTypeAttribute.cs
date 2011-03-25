using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace SkeletonSite.Kernel.Attributes.Validation
{
    /// <summary>
    /// Custom implementation of DataTypeAttribute that applies translation to the error message
    /// </summary>
    public class DataTypeAttribute : System.ComponentModel.DataAnnotations.DataTypeAttribute
    {
        public DataTypeAttribute(string customDataType): base(customDataType)
        {
        }

        public DataTypeAttribute(System.ComponentModel.DataAnnotations.DataType dataType): base(dataType)
        {
        }

        public override string FormatErrorMessage(string name)
        {
            return Dictionary.Translate(base.FormatErrorMessage(name));
        }
    }
}
