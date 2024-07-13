// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Configs
{
    using Fsel.System.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class StudentTechieEntityTypeConfiguration : IEntityTypeConfiguration<StudentTechie>
    {
        public void Configure(EntityTypeBuilder<StudentTechie> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.HasOne(a => a.TechieAction)
                   .WithMany(b => b.StudentTechies)
                   .HasForeignKey(b => b.TechieActionId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
