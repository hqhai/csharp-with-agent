using Fsel.Course.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fsel.Course.Domain.Enums;

namespace Fsel.Course.Infrastructure.Configs
{
    public class ClassForumEntityTypeConfiguration : IEntityTypeConfiguration<ClassForum>
    {
        public void Configure(EntityTypeBuilder<ClassForum> builder)
        {
            builder.Property(e => e.GradingStyle)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => (EnumGradingStyle)Enum.Parse(typeof(EnumGradingStyle), v));
            builder.Property(e => e.CourseSkill)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => (EnumCourseSkill)Enum.Parse(typeof(EnumCourseSkill), v));
        }
    }
}