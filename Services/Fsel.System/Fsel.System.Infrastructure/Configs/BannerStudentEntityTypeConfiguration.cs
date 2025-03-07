// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Configs
{
    using Fsel.System.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class BannerStudentEntityTypeConfiguration : IEntityTypeConfiguration<BannerStudent>
    {
        public void Configure(EntityTypeBuilder<BannerStudent> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.Banner)
                   .WithMany(b => b.BannerStudents)
                   .HasForeignKey(b => b.BannerId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(c => new { c.IsDeleted, c.StudentId, c.BannerId });
            builder.HasIndex(c => new { c.IsDeleted, c.StudentId, c.CreatedDate });
        }
    }
}
