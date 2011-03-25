using System;
using SkeletonSite.Kernel.Database.Mappings;

namespace SkeletonSite.Kernel.Database.Entities
{
    public class TestObject : BaseEntity<TestObject>
	{
		public virtual int Id { get; set; }
        public virtual string Name { get; set; }

        public TestObject()
        {
        }
	}
}

