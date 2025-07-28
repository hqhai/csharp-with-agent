using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.System.Domain.Entities.DailyQuiz;
using Fsel.System.Domain.IRepositories.DailyQuizs;
using Fsel.System.Domain.Models.CommandModels.DailyQuiz;
using MediatR;

namespace Fsel.System.Application.Commands.DailyQuiz
{
    public class CreateDailyQuizQuestionsCommand : CreateDailyQuizQuestionsCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class CreateDailyQuizQuestionsCommandHandler : IRequestHandler<CreateDailyQuizQuestionsCommand, MethodResult<bool>>
    {
        private readonly IDailyQuizQuestionRepository _dailyQuizQuestionRepository;
        private readonly IMapper _mapper;

        public CreateDailyQuizQuestionsCommandHandler(IDailyQuizQuestionRepository dailyQuizQuestionRepository, IMapper mapper)
        {
            _dailyQuizQuestionRepository = dailyQuizQuestionRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<bool>> Handle(CreateDailyQuizQuestionsCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var questions = _mapper.Map<IList<DailyQuizQuestion>>(request.DailyQuizQuestions);
            if (!questions.Any())
            {
                return methodResult;
            }

            questions.ForEach(p =>
            {
                if (!p.IsValid())
                {
                    methodResult.AddError(p.ErrorMessages);
                }
            });

            if (methodResult.StatusCode != null)
            {
                return methodResult;
            }

            await _dailyQuizQuestionRepository.ExecuteTransactionAsync(async () =>
            {
                await _dailyQuizQuestionRepository.AddList(questions);
                await _dailyQuizQuestionRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.Result = true;
                return methodResult;
            });
            methodResult.Result = true;
            return methodResult;
        }
    }
}
