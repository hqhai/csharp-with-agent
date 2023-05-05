// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Configs
{
    using System;
    using Fsel.Identity.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class TeacherBankAccountEntityTypeConfiguration : IEntityTypeConfiguration<TeacherBankAccount>
    {
        public void Configure(EntityTypeBuilder<TeacherBankAccount> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.HasOne(a => a.Teacher)
                    .WithOne(b => b.TeacherBankAccount)
                    .HasForeignKey<TeacherBankAccount>(b => b.TeacherId)
                    .OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(x => x.TeacherId).IsUnique(false);
        }
    }
}
