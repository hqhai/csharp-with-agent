// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Lms.Application.Queries.ExamPracticeQuery.V1i1
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.ExamPractice.Domain.Enums;
    using Fsel.ExamPractice.Domain.IRepositories;
    using Fsel.ExamPractice.Domain.Models.EntityModels.ExamPractices;
    using Fsel.ExamPractice.Lms.Application.Services.UserServices;
    using Fsel.ExamPractice.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class SearchResultDoneByTypeQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<ExamPracticeReportModel>>>
    {
        public EnumExamPracticeType Type { get; set; }
    }

    public class GetResultDoneByTypeQueryHandler : IRequestHandler<SearchResultDoneByTypeQuery, MethodResult<PagingItemsModel<ExamPracticeReportModel>>>
    {
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IExamPracticeResultRepository _examPracticeResultRepository;
        private readonly IMapper _mapper;
        private readonly IExamPracticeRepository _examPracticeRepository;

        public GetResultDoneByTypeQueryHandler(
            AuthContext authContext,
            IUserService userService,
            IExamPracticeResultRepository examPracticeResultRepository,
            IMapper mapper,
            IExamPracticeRepository examPracticeRepository)
        {
            _authContext = authContext;
            _userService = userService;
            _examPracticeResultRepository = examPracticeResultRepository;
            _mapper = mapper;
            _examPracticeRepository = examPracticeRepository;
        }

        public async Task<MethodResult<PagingItemsModel<ExamPracticeReportModel>>> Handle(SearchResultDoneByTypeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<ExamPracticeReportModel>>();

            var methodResultStudent = await GetStudentAsync();
            if (!methodResultStudent.IsOK)
            {
                methodResult.AddErrorBadRequest(methodResultStudent.ErrorMessages);
                return methodResult;
            }
            var student = methodResultStudent.Result ?? new StudentModel();
            var query = from epr in _examPracticeResultRepository.Queryable
                        join ep in _examPracticeRepository.Queryable on epr.ExamPracticeId equals ep.Id
                        where epr.StudentId == student.Id && ep.Type == request.Type && !ep.IsArchive
                        && epr.WorkingStatus == EnumWorkingStatus.Active && epr.Status == EnumResultStatus.Done
                        orderby epr.UpdatedDate descending
                        select new ExamPracticeReportModel
                        {
                            Id = epr.Id,
                            CreatedDate = epr.CreatedDate,
                            CreatedFullName = epr.CreatedFullName,
                            CreatedUserId = epr.CreatedUserId,
                            Name = ep.Code,
                            Code = ep.Code,
                            SubType = ep.SubType,
                            Type = ep.Type,
                            UpdatedDate = epr.UpdatedDate,
                            UpdatedFullName = epr.UpdatedFullName,
                            UpdatedUserId = epr.UpdatedUserId,
                            ExamPracticeResult = _mapper.Map<ExamPracticeResultModel>(epr)
                        };
            int totalItem = await query.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await query.ApplyPaging(request)
                                   .AsNoTracking()
                                   .ToListAsync(cancellationToken: cancellationToken)
                                   .ConfigureAwait(false);

            methodResult.Result = new PagingItemsModel<ExamPracticeReportModel>(lists, request, totalItem);
            return methodResult;
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
