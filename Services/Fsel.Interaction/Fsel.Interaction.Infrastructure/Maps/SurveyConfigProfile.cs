using AutoMapper;
using Fsel.Core.Extensions;
using Fsel.Interaction.Domain.Entities;
using Fsel.Interaction.Domain.Models.CommandModels.SurveyConfigs;
using Fsel.Interaction.Domain.Models.EntityModels;

namespace Fsel.Interaction.Infrastructure.Maps
{
    public class SurveyConfigProfile : Profile
    {
        public SurveyConfigProfile()
        {
            CreateMap<SaveSurveyConfigCommandModel, SurveyConfig>().IgnoreAllNonExisting();
            CreateMap<SurveyConfig, SurveyConfigModel>().IgnoreAllNonExisting();
        }
    }
}
