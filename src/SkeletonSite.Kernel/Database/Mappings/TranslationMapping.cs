using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;
using SkeletonSite.Kernel.Database.Entities;

namespace SkeletonSite.Kernel.Database.Mappings
{
    public class TranslationMapping : BaseMapping<Translation>
    {
        public TranslationMapping(): base()
        {
            Table("Translations");
            Id(x => x.Id);
            Map(x => x.TranslationKey).Length(255).Unique();
            Map(x => x.TranslationNL).Length(4000);
            Map(x => x.TranslationEN).Length(4000);
        }
    }
}
