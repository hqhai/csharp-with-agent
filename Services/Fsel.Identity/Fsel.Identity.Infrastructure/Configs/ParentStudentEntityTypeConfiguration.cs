// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Configs
{
    using Fsel.Identity.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ParentStudentEntityTypeConfiguration : IEntityTypeConfiguration<ParentStudent>
    {
        public void Configure(EntityTypeBuilder<ParentStudent> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.HasOne(a => a.Student)
                .WithMany(b => b.ParentStudents)
                .HasForeignKey(b => b.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            builder.HasIndex(x => x.StudentId).IsUnique(true);

            builder.HasOne(a => a.Parent)
                .WithMany(b => b.ParentStudents)
                .HasForeignKey(b => b.ParentId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            builder.HasIndex(x => x.ParentId).IsUnique(true);
        }
    }
}
