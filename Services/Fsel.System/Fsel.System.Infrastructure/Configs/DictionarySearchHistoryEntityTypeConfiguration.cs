// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Configs
{
    using Fsel.System.Domain.Entities;
    using global::System;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class DictionarySearchHistoryEntityTypeConfiguration : IEntityTypeConfiguration<DictionarySearchHistory>
    {
        public void Configure(EntityTypeBuilder<DictionarySearchHistory> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            // Id is generated on application side, not database
            builder.Property(e => e.Id)
                .ValueGeneratedNever();

            // Index for user queries
            builder.HasIndex(e => e.UserId)
                .HasDatabaseName("IX_DictionarySearchHistory_UserId");

            // Index for sorting by search time
            builder.HasIndex(e => e.CreatedDate)
                .HasDatabaseName("IX_DictionarySearchHistory_CreatedDate");

            // Composite index for user's search history with pagination
            builder.HasIndex(e => new { e.UserId, e.CreatedDate })
                .HasDatabaseName("IX_DictionarySearchHistory_UserId_CreatedDate");

            // Configure SearchTerm with max length
            builder.Property(e => e.SearchTerm)
                .HasMaxLength(500)
                .IsRequired();

            // Configure SearchContext
            builder.Property(e => e.SearchContext)
                .HasMaxLength(2000);
        }
    }
}
