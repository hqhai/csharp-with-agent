// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Infrastructure.Configs
{
    using Fsel.Training.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ClassStudentEntityTypeConfiguration : IEntityTypeConfiguration<ClassStudent>
    {
        public void Configure(EntityTypeBuilder<ClassStudent> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.HasOne(a => a.Class)
               .WithMany(b => b.ClassStudents)
               .HasForeignKey(b => b.ClassId)
               .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(c => new { c.IsDeleted, c.ClassId, c.StudentId });
        }
    }
}
