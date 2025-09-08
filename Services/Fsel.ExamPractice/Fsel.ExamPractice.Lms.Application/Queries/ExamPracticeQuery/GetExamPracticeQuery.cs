// Copyright (c) Atlantic. All rights reserved.

using Microsoft.EntityFrameworkCore;

namespace Fsel.ExamPractice.Lms.Application.Queries.ExamPracticeQuery
{
    using System.Linq.Dynamic.Core;
    using System.Threading;
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
    using Fsel.ExamPractice.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;

    public class GetExamPracticeQuery : IRequest<MethodResult<ExamPracticeDetailModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetExamPracticeQueryHandler : IRequestHandler<GetExamPracticeQuery, MethodResult<ExamPracticeDetailModel>>
    {
        private readonly IExamPracticeSectionRepository _examPracticeSectionRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IExamPracticeResultRepository _examPracticeResultRepository;
        private readonly IExamPracticeAnswerRepository _examPracticeAnswerRepository;
        private readonly IMapper _mapper;

        public GetExamPracticeQueryHandler(
            IExamPracticeSectionRepository examPracticeSectionRepository,
            AuthContext authContext,
            IUserService userService,
            IExamPracticeResultRepository examPracticeResultRepository,
            IExamPracticeAnswerRepository examPracticeAnswerRepository,
            IMapper mapper)
        {
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

            var methodResultStudent = await GetStudentAsync();
            if (!methodResultStudent.IsOK)
            {
                methodResult.AddErrorBadRequest(methodResultStudent.ErrorMessages);
                return methodResult;
            }
            var student = methodResultStudent.Result ?? new StudentModel();

            var examPracticeResult = await _examPracticeResultRepository.Queryable.Include(x => x.ExamPractice)
                                                                        .Where(x => x.ExamPracticeId == request.Id && x.StudentId == student.Id)
                                                                        .Where(x => x.WorkingStatus == EnumWorkingStatus.Active)
                                                                        .AsNoTracking()
                                                                        .FirstOrDefaultAsync(cancellationToken);
            if (examPracticeResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(examPracticeResult));
                return methodResult;
            }
            if (examPracticeResult.ExamPractice == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(examPracticeResult.ExamPractice), request.Id);
                return methodResult;
            }

            var examPracticeDetail = GetExamPracticeDetail(examPracticeResult.ExamPractice, examPracticeResult);
            examPracticeDetail.ExamPracticeSections = await GetExamPracticeSections(examPracticeResult.ExamPractice, examPracticeResult);
            methodResult.Result = examPracticeDetail;
            return methodResult;
        }

        private ExamPracticeDetailModel GetExamPracticeDetail(ExamPractice examPractice, ExamPracticeResult examPracticeResult)
        {
            var examPracticeDetail = _mapper.Map<ExamPracticeDetailModel>(examPractice);
            var examPracticeResultModel = _mapper.Map<ExamPracticeResultModel>(examPracticeResult);

            double executionExamPracticeTime;
            if (examPracticeResult.PracticeMode == EnumPracticeMode.Practice && examPracticeResult.Config?.PracticeTimeLimitOption != EnumPracticeTimeLimitOption.ExamBased)
            {
                executionExamPracticeTime = examPracticeResult.Config?.ExecutionTime ?? default;
                examPracticeResultModel.RemainingTime = executionExamPracticeTime - examPracticeResult.WorkingTime > 0 ? executionExamPracticeTime - examPracticeResult.WorkingTime : default;
            }

            examPracticeDetail.ExamPracticeResult = examPracticeResultModel;
            return examPracticeDetail;
        }

        private async Task<List<ExamPracticeSectionDetailModel>> GetExamPracticeSections(ExamPractice examPractice, ExamPracticeResult examPracticeResult)
        {
            var examPracticeSectionDetails = new List<ExamPracticeSectionDetailModel>();
            var examPracticeSections = await GetSectionsAsync(examPractice, examPracticeResult.Id);
            var examPracticeAnswers = await _examPracticeAnswerRepository.Queryable
                                                                         .Where(x => x.ExamPracticeResultId == examPracticeResult.Id && x.QuestionId.HasValue)
                                                                         .ToListAsync();
            var examPracticeAnswerDict = examPracticeAnswers.GroupBy(x => x.QuestionId!.Value).ToDictionary(g => g.Key, g => g.FirstOrDefault());

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
                                                                         examPracticeAnswerDict.TryGetValue(x.Id, out var answer);
                                                                         return new QuestionCorrectStatusModel
                                                                         {
                                                                             QuestionId = x.Id,
                                                                             Status = EnumHelper.GetStatus(answer, examPracticeResult.Status == EnumResultStatus.Done)
                                                                         };
                                                                     }).ToList();
                examPracticeSectionDetails.Add(examPracticeSectionDto);
            }
            return examPracticeSectionDetails;
        }

        private async Task<List<ExamPracticeSection>> GetSectionsAsync(ExamPractice examPractice, Guid examPracticeResultId)
        {
            // Base query
            var query = _examPracticeSectionRepository.Queryable
                .AsNoTracking()
                .Where(x => x.ExamPracticeId == examPractice.Id);

            if (examPractice.Type != EnumExamPracticeType.ExamPractice)
            {
                query = query.Include(x => x.ExamPracticeSectionResults.Where(r => r.ExamPracticeResultId == examPracticeResultId));
            }
            else
            {
                query = query.Include(x => x.Questions);
            }

            return await query.OrderBy(x => x.DisplayOrder).ToListAsync();
        }

        private async Task<MethodResult<StudentModel>> GetStudentAsync()
        {
            MethodResult<StudentModel> methodResult = new();
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
            methodResult.Result = student;
            return methodResult;
        }
    }
}
