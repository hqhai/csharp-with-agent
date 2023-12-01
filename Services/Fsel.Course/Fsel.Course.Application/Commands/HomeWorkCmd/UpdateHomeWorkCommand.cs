// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.HomeWorkCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.HomeWorks;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateHomeWorkCommand : UpdateHomeWorkCommandModel, IRequest<MethodResult<HomeWorkModel>>
    {
    }

    public class UpdateHomeWorkCommandHandler : IRequestHandler<UpdateHomeWorkCommand, MethodResult<HomeWorkModel>>
    {
        private readonly IMapper _mapper;
        private readonly IHomeWorkRepository _homeWorkRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly QuestionTypeConverter _questionTypeConverter;

        public UpdateHomeWorkCommandHandler(IMapper mapper
            , IHomeWorkRepository homeWorkRepository
            , IQuestionRepository questionRepository
            , QuestionTypeConverter questionTypeConverter)
        {
            _mapper = mapper;
            _homeWorkRepository = homeWorkRepository;
            _questionRepository = questionRepository;
            _questionTypeConverter = questionTypeConverter;
        }

        public async Task<MethodResult<HomeWorkModel>> Handle(UpdateHomeWorkCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<HomeWorkModel> methodResult = new MethodResult<HomeWorkModel>();
            var homeWork = await _homeWorkRepository.Queryable
                            .Include(x => x.HomeWorkQuestions.Where(n => !n.IsDeleted))
                            .ThenInclude(x => x.Question)
                            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken: cancellationToken);
            if (homeWork == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(homeWork));
                return methodResult;
            }
            var questionDeletes = homeWork.HomeWorkQuestions.Where(x => x.Question != null && !x.IsDeleted).Select(x => x.Question!);

            homeWork = _mapper.Map(request, homeWork);

            if (request.Questions == null || request.Questions.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Questions));
                return methodResult;
            }
            var homeWorkQuestions = new List<HomeWorkQuestion>();
            request.Questions.ForEach(q =>
            {
                if (q == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Questions));
                }
                else
                {
                    Question question = _mapper.Map<Question>(q);
                    var (config, correctTotal) = _questionTypeConverter.QuestionTypeConverterObject(question.Config, question.QuestionType, !question.Ungraded);
                    if (config == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumVideoErrorCode.ConfigIsInTheWrongFormat), nameof(question.Config), question.Config);
                    }
                    question.CorrectTotal = correctTotal;

                    if (!question.IsValid())
                    {
                        methodResult.AddErrorBadRequest(question.ErrorMessages);
                    }

                    homeWorkQuestions.Add(new HomeWorkQuestion
                    {
                        Question = question
                    });
                }
            });

            homeWork.HomeWorkQuestions = homeWorkQuestions;
            if (!homeWork.IsValid())
            {
                methodResult.AddErrorBadRequest(homeWork.ErrorMessages);
                return methodResult;
            }
            else if (!methodResult.IsOK)
            {
                return methodResult;
            }

            var isHomeWorkUsed = await _homeWorkRepository.IsHomeWorkUsed(request.Id);
            if (isHomeWorkUsed)
            {
                methodResult.AddErrorBadRequest(nameof(EnumHomeWorkErrorCode.HomeWorkUsed), nameof(request.Id), request.Id);
                return methodResult;
            }

            await _homeWorkRepository.ExecuteTransactionAsync(async () =>
            {
                homeWork = _homeWorkRepository.Update(homeWork);
                await _homeWorkRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                foreach (var item in questionDeletes)
                {
                    await _questionRepository.DeleteAsync(item).ConfigureAwait(false);
                }
                await _questionRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<HomeWorkModel>(homeWork);
                return methodResult;
            });

            return methodResult;
        }
    }
}
