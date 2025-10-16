// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class AiModelManagerEntityTypeConfiguration : IEntityTypeConfiguration<AiModelManager>
    {
        public void Configure(EntityTypeBuilder<AiModelManager> builder)
        {

        }
    }
}
