// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Infrastructure.Repositories
{
    using Fsel.Core.Base;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Interaction.Domain.IRepositories;

    public class CustomerSurveyRepository : BaseRepository<CustomerSurvey>, ICustomerSurveyRepository
    {
        public CustomerSurveyRepository(InteractionDbContext dbContext, AuthContext authContext) : base(dbContext, authContext)
        {
        }
    }
}
