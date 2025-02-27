// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Infrastructure.Repositories
{
    using AutoMapper;
    using Fsel.Core.Base;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Interaction.Domain.IRepositories;

    public class CustomerSurveyGroupRepository : BaseRepository<CustomerSurveyGroup>, ICustomerSurveyGroupRepository
    {
        public CustomerSurveyGroupRepository(InteractionDbContext dbContext, AuthContext authContext, IMapper mapper) : base(dbContext, authContext, mapper)
        {
        }
    }
}
