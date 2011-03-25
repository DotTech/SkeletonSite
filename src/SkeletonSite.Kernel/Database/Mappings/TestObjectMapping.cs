using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;
using SkeletonSite.Kernel.Database.Entities;

namespace SkeletonSite.Kernel.Database.Mappings
{
    public class TestObjectMapping : BaseMapping<TestObject>
    {
        public TestObjectMapping(): base()
        {
            Table("TestObjects");
            Id(x => x.Id);
            Map(x => x.Name);

            /*
            References(x => x.Parent)
                .Column("ParentId")
                .Cascade.None()
                .Nullable();
            HasMany(x => x.UserClickouts)
                .KeyColumn("ProductId")
                .Cascade.None().ReadOnly()
                .Table("ProductMerges");
            HasManyToMany(x => x.ProductPropertyValues)
                .Cascade.All()
                .ParentKeyColumn("ProductId")
                .ChildKeyColumn("ProductPropertyValueId")
                .Table("ProductsProductPropertyValues");
            */
        }
    }
}
