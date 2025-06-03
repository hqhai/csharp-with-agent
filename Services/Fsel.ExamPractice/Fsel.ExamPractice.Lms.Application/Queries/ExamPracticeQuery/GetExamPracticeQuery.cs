// Copyright (c) Atlantic. All rights reserved.

using Microsoft.EntityFrameworkCore;

namespace Fsel.ExamPractice.Lms.Application.Queries.ExamPracticeQuery
{
    using System.Linq.Dynamic.Core;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.ExamPractice.Domain.Entities;
    using Fsel.ExamPractice.Domain.Enums;
    using Fsel.ExamPractice.Domain.IRepositories;
    using Fsel.ExamPractice.Domain.Models.EntityModels.ExamPractices;
    using Fsel.ExamPractice.Domain.Models.EntityModels.Questions;
    using Fsel.ExamPractice.Infrastructure.Common;
    using Fsel.ExamPractice.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;

    public class GetExamPracticeQuery : IRequest<MethodResult<ExamPracticeDetailModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetExamPracticeQueryHandler : IRequestHandler<GetExamPracticeQuery, MethodResult<ExamPracticeDetailModel>>
    {
        private readonly IExamPracticeRepository _examPracticeRepository;
        private readonly IExamPracticeSectionRepository _examPracticeSectionRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IExamPracticeResultRepository _examPracticeResultRepository;
        private readonly IExamPracticeAnswerRepository _examPracticeAnswerRepository;
        private readonly IMapper _mapper;

        public GetExamPracticeQueryHandler(IExamPracticeRepository examPracticeRepository,
            IExamPracticeSectionRepository examPracticeSectionRepository,
            AuthContext authContext,
            IUserService userService,
            IExamPracticeResultRepository examPracticeResultRepository,
            IExamPracticeAnswerRepository examPracticeAnswerRepository,
            IMapper mapper)
        {
            _examPracticeRepository = examPracticeRepository;
            _examPracticeSectionRepository = examPracticeSectionRepository;
            _authContext = authContext;
            _userService = userService;
            _examPracticeResultRepository = examPracticeResultRepository;
            _examPracticeAnswerRepository = examPracticeAnswerRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<ExamPracticeDetailModel>> Handle(GetExamPracticeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ExamPracticeDetailModel>();
            var studentResult = await _userService.GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult?.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentResult));
                return methodResult;
            }
            var examPractice = await _examPracticeRepository.GetByIdAsync(request.Id);
            if (examPractice == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(examPractice), request.Id);
                return methodResult;
            }
            var examPracticeResult = await _examPracticeResultRepository.Queryable.Where(x => x.ExamPracticeId == examPractice.Id && x.StudentId == student.Id)
                                                                        .Where(x => x.WorkingStatus == Shared.Enums.EnumWorkingStatus.Active)
                                                                        .FirstOrDefaultAsync(cancellationToken);
            if (examPracticeResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(examPracticeResult));
                return methodResult;
            }
            var examPracticeSections = new List<ExamPracticeSection>();
            if (examPractice.Type == EnumExamPracticeType.IELTS)
            {
                examPracticeSections = await _examPracticeSectionRepository.Queryable.Include(x => x.ExamPracticeSectionResults.Where(x => x.ExamPracticeResultId == examPracticeResult.Id)).Where(x => x.ExamPracticeId == examPracticeResult.ExamPracticeId).OrderBy(x => x.DisplayOrder).ToListAsync(cancellationToken);
            }
            else
            {
                examPracticeSections = await _examPracticeSectionRepository.Queryable.Include(x => x.Questions).Where(x => x.ExamPracticeId == examPracticeResult.ExamPracticeId).OrderBy(x => x.DisplayOrder).ToListAsync(cancellationToken);
            }
            var examPracticeDetail = _mapper.Map<ExamPracticeDetailModel>(examPractice);
            var examPracticeResultModel = _mapper.Map<ExamPracticeResultModel>(examPracticeResult);

            double executionExamPracticeTime = default;
            if (examPracticeResult.PracticeMode == EnumPracticeMode.Practice && examPracticeResult.Config?.PracticeTimeLimitOption != EnumPracticeTimeLimitOption.ExamBased)
            {
                executionExamPracticeTime = examPracticeResult.Config?.ExecutionTime ?? default;
                examPracticeResultModel.RemainingTime = executionExamPracticeTime - examPracticeResult.WorkingTime > 0 ? executionExamPracticeTime - examPracticeResult.WorkingTime : default;
            }

            examPracticeDetail.ExamPracticeResult = examPracticeResultModel;

            var examPracticeSectionDetails = new List<ExamPracticeSectionDetailModel>();
            var examPracticeAnswers = await _examPracticeAnswerRepository.Queryable.Where(x => x.ExamPracticeResultId == examPracticeResult.Id && x.QuestionId.HasValue).ToListAsync(cancellationToken);

            foreach (var item in examPracticeSections)
            {
                var examPracticeSectionDto = _mapper.Map<ExamPracticeSectionDetailModel>(item);
                var examPracticeSectionResult = _mapper.Map<ExamPracticeSectionResultModel>(item.ExamPracticeSectionResults.FirstOrDefault());

                double executionTime = default;
                if (examPracticeSectionResult != null && examPracticeResult.PracticeMode == EnumPracticeMode.Practice && examPracticeResult.Config?.PracticeTimeLimitOption != EnumPracticeTimeLimitOption.ExamBased)
                {
                    executionTime = examPracticeResult.Config?.ExecutionTime ?? default;
                    examPracticeSectionResult.RemainingTime = executionTime - examPracticeSectionResult.WorkingTime > 0 ? executionTime - examPracticeSectionResult.WorkingTime : default;
                }
                else
                {
                    executionTime = item.Config?.ExecutionTime ?? default;
                }

                examPracticeSectionDto.ExecutionTime = executionTime;
                examPracticeSectionDto.ExamPracticeSectionResult = examPracticeSectionResult;
                examPracticeSectionDto.QuestionTests = item.Questions.OrderBy(x => x.CreatedDate)
                                            .Select(x =>
                                            {
                                                var answer = examPracticeAnswers.FirstOrDefault(y => y.QuestionId == x.Id);
                                                return new QuestionCorrectStatusModel
                                                {
                                                    QuestionId = x.Id,
                                                    Status = EnumHelper.GetStatus(answer, examPracticeResult.Status == EnumResultStatus.Done)
                                                };
                                            }).ToList();
                examPracticeSectionDetails.Add(examPracticeSectionDto);
            }
            examPracticeDetail.ExamPracticeSections = examPracticeSectionDetails;

            methodResult.Result = examPracticeDetail;
            return methodResult;
        }
    }
}
