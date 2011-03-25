using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;
using SkeletonSite.Kernel.Database.Entities;

namespace SkeletonSite.Kernel.Database.Mappings
{
    public class BaseMapping<T> : ClassMap<T>
    {
        public BaseMapping()
        {
        }
    }
}
