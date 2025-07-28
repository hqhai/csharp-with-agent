using AutoMapper;
using Fsel.Core.Extensions;
using Fsel.System.Domain.Entities.DailyQuiz;
using Fsel.System.Domain.Models.CommandModels.DailyQuiz;
using Fsel.System.Domain.Models.EntityModels;

namespace Fsel.System.Infrastructure.Maps
{
    public class DailyQuizProfile : Profile
    {
        public DailyQuizProfile()
        {
            CreateMap<DailyQuizQuestion, DailyQuizQuestionModel>().IgnoreAllNonExisting();
            CreateMap<DailyQuizAnswer, DailyQuizAnswerModel>().IgnoreAllNonExisting();
            CreateMap<CreateDailyQuizQuestionCommandModel, DailyQuizQuestion>().IgnoreAllNonExisting();
            CreateMap<CreateDailyQuizAnswerCommandModel, DailyQuizAnswer>().IgnoreAllNonExisting();
            CreateMap<CreateDailyQuizTranslationCommandModel, DailyQuizQuestionTranslation>().IgnoreAllNonExisting();
            CreateMap<CreateDailyQuizTranslationCommandModel, DailyQuizAnswerTranslation>().IgnoreAllNonExisting();
            CreateMap<DailyQuizCommandModel, DailyQuizHistory>().IgnoreAllNonExisting();

            CreateMap<DailyQuizQuestionTranslation, DailyQuizQuestion>().IgnoreEntity()?.ReverseMap();
            CreateMap<DailyQuizQuestion, DailyQuizQuestionModel>().IgnoreAllNonExisting()?.MapTranslations<DailyQuizQuestion, DailyQuizQuestionModel, DailyQuizQuestionTranslation>();

            CreateMap<DailyQuizAnswerTranslation, DailyQuizAnswer>().IgnoreEntity()?.ReverseMap();
            CreateMap<DailyQuizAnswer, DailyQuizAnswerModel>().IgnoreAllNonExisting()?.MapTranslations<DailyQuizAnswer, DailyQuizAnswerModel, DailyQuizAnswerTranslation>();
        }
    }
}
