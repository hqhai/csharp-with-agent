// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.HomeWorkResultCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.HomeWorkResults;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ReviewLessonHomeWorkCommand : ReviewLessonHomeWorkCommandModel, IRequest<MethodResult<HomeWorkResultModel>>
    {
    }

    public class ReviewLessonHomeWorkCommandHandler : IRequestHandler<ReviewLessonHomeWorkCommand, MethodResult<HomeWorkResultModel>>
    {
        private readonly IHomeWorkResultRepository _homeWorkResultRepository;
        private readonly IMapper _mapper;
        private readonly IQuestionRepository _questionRepository;
        private readonly IHomeWorkAnswerRepository _homeWorkAnswerRepository;
        private readonly IHomeWorkRepository _homeWorkRepository;
        private readonly IHomeWorkQuestionRepository _homeWorkQuestionRepository;

        public ReviewLessonHomeWorkCommandHandler(IHomeWorkResultRepository homeWorkResultRepository,
            IMapper mapper,
            IQuestionRepository questionRepository,
            IHomeWorkAnswerRepository homeWorkAnswerRepository,
            IHomeWorkRepository homeWorkRepository,
            IHomeWorkQuestionRepository homeWorkQuestionRepository)
        {
            _homeWorkResultRepository = homeWorkResultRepository;
            _mapper = mapper;
            _questionRepository = questionRepository;
            _homeWorkAnswerRepository = homeWorkAnswerRepository;
            _homeWorkRepository = homeWorkRepository;
            _homeWorkQuestionRepository = homeWorkQuestionRepository;
        }

        public async Task<MethodResult<HomeWorkResultModel>> Handle(ReviewLessonHomeWorkCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<HomeWorkResultModel> methodResult = new MethodResult<HomeWorkResultModel>();

            var homeWorkResult = await _homeWorkResultRepository.Queryable.FirstOrDefaultAsync(x => x.LessonResultId == request.LessonResulttId, cancellationToken: cancellationToken);
            if (homeWorkResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumHomeWorkResultErrorCode.HomeWorkResultNotExist), nameof(request.LessonResulttId), request.LessonResulttId);
                return methodResult;
            }

            var answerQuery = from vtca in _homeWorkAnswerRepository.Queryable
                              where vtca.HomeWorkResultId == homeWorkResult.Id
                              select vtca.CorrectCount;

            var questionQuery = from h in _homeWorkRepository.Queryable
                                join hq in _homeWorkQuestionRepository.Queryable on h.Id equals hq.HomeWorkId
                                join q in _questionRepository.Queryable on hq.QuestionId equals q.Id
                                where h.Id == homeWorkResult.HomeWorkId
                                select q.CorrectTotal;

            homeWorkResult.CorrectCount = await answerQuery.SumAsync(cancellationToken);
            homeWorkResult.CorrectTotal = await questionQuery.SumAsync(cancellationToken);
            homeWorkResult.Status = EnumResultStatus.Done;

            await _homeWorkResultRepository.ExecuteTransactionAsync(async () =>
            {
                homeWorkResult = _homeWorkResultRepository.Update(homeWorkResult);
                await _homeWorkResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<HomeWorkResultModel>(homeWorkResult);
                return methodResult;
            });
            return methodResult;
        }
    }
}
