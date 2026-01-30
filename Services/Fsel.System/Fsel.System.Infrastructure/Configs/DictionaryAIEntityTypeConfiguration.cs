// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Configs
{
    using Fsel.System.Domain.Entities;
    using global::System;
    using global::System.Linq;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class DictionaryAIEntityTypeConfiguration : IEntityTypeConfiguration<DictionaryAI>
    {
        public void Configure(EntityTypeBuilder<DictionaryAI> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            // Id is generated on application side, not database
            builder.Property(e => e.Id)
                .ValueGeneratedNever();

            // Configure Embedding - store as text, cast to vector when querying
            builder.Property(e => e.Embedding)
                .HasColumnType("text")
                .HasConversion(
                    v => v == null ? null : "[" + string.Join(",", v.Select(x => x.ToString("G9"))) + "]",
                    v => ParseVector(v));
        }

        private static float[]? ParseVector(string? vectorString)
        {
            if (string.IsNullOrEmpty(vectorString))
                return null;

            var cleaned = vectorString.Trim('[', ']', ' ', '\n', '\r');
            if (string.IsNullOrEmpty(cleaned))
                return null;

            return cleaned.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => float.TryParse(x.Trim(), out var f) ? f : 0f)
                .ToArray();
        }
    }
}
