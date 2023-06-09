// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Configs
{
    using System;
    using Fsel.Common.Helpers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class TeacherBankAccountEntityTypeConfiguration : IEntityTypeConfiguration<TeacherBankAccount>
    {
        public void Configure(EntityTypeBuilder<TeacherBankAccount> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.Teacher)
                    .WithMany(b => b.TeacherBankAccounts)
                    .HasForeignKey(b => b.TeacherId)
                    .OnDelete(DeleteBehavior.Cascade);

            builder.Property(e => e.Status)
                   .HasMaxLength(100)
                   .HasConversion(
                        v => v.ToString(),
                        v => v.EnumParse<EnumStatusBank>());
        }
    }
}
