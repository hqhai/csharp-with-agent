// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using Domain.Entities;
    using Fsel.Common.Helpers;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class AiPromptManagerEntityTypeConfiguration : IEntityTypeConfiguration<AiPromptManager>
    {
        public void Configure(EntityTypeBuilder<AiPromptManager> builder)
        {

        }
    }
}
