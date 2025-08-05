// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Infrastructure.Repositories
{
    using Fsel.Core.Base;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Interaction.Domain.IRepositories;

    public class SurveyQuestionRepository : BaseRepository<SurveyQuestion>, ISurveyQuestionRepository
    {
        public SurveyQuestionRepository(InteractionDbContext dbContext, InteractionReadDbContext readDbContext, AuthContext authContext, AutoMapper.IMapper mapper) : base(dbContext, readDbContext, authContext, mapper)
        {
        }
    }
}
