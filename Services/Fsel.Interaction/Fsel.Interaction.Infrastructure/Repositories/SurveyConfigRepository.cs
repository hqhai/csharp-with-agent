using Fsel.Core.Base;
using Fsel.Interaction.Domain.Entities;
using Fsel.Interaction.Domain.IRepositories;

namespace Fsel.Interaction.Infrastructure.Repositories
{
    public class SurveyConfigRepository : BaseRepository<SurveyConfig>, ISurveyConfigRepository
    {
        public SurveyConfigRepository(InteractionDbContext dbContext, AuthContext authContext, AutoMapper.IMapper mapper) : base(dbContext, authContext, mapper)
        {
        }
    }
}
