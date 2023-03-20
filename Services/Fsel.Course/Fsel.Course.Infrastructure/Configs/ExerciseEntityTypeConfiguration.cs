using Fsel.Common.Helpers;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fsel.Course.Infrastructure.Configs
{
    public class ExerciseEntityTypeConfiguration : IEntityTypeConfiguration<Exercise>
    {
        public void Configure(EntityTypeBuilder<Exercise> builder)
        {
            builder.Property(e => e.CourseSkill)
               .HasMaxLength(100)
               .HasConversion(
                   v => v.ToString(),
                   v => v.EnumParse<EnumCourseSkill>());
        }
    }
}
