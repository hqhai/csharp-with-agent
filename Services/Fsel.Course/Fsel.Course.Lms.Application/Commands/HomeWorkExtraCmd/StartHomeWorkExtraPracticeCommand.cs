// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.HomeWorkExtraCmd
{
    using System;
    using System.Linq.Dynamic.Core;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.ApplicationServices.CacheServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class StartHomeWorkExtraPracticeCommand : IRequest<MethodResult<bool>>
    {
        public Guid HomeWorkId { get; set; }
        public Guid? HomeWorkConfigId { get; set; }
    }

    public class StartHomeWorkExtraPracticeCommandHandler : IRequestHandler<StartHomeWorkExtraPracticeCommand, MethodResult<bool>>
    {
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IHomeWorkExtraPracticeResultRepository _homeWorkExtraPracticeResultRepository;
        private readonly IHomeWorkRepository _homeWorkRepository;
        private readonly IHomeWorkRetryRepository _homeWorkRetryRepository;
        private readonly ICurriculumRepository _curriculumRepository;
        private readonly ICurriculumStudentRepository _curriculumStudentRepository;
        private readonly IHomeWorkConfigRepository _homeWorkConfigRepository;
        private readonly IRequestSafeCachingService _requestSafeCachingService;
        private const int MaxRetry = 1;

        public StartHomeWorkExtraPracticeCommandHandler(AuthContext authContext,
            IUserService userService,
            IHomeWorkExtraPracticeResultRepository homeWorkExtraPracticeResultRepository,
            IHomeWorkRepository homeWorkRepository,
            IHomeWorkRetryRepository homeWorkRetryRepository,
            ICurriculumRepository curriculumRepository,
            ICurriculumStudentRepository curriculumStudentRepository,
            IHomeWorkConfigRepository homeWorkConfigRepository,
            IRequestSafeCachingService requestSafeCachingService)
        {
            _authContext = authContext;
            _userService = userService;
            _homeWorkExtraPracticeResultRepository = homeWorkExtraPracticeResultRepository;
            _homeWorkRepository = homeWorkRepository;
            _homeWorkRetryRepository = homeWorkRetryRepository;
            _curriculumRepository = curriculumRepository;
            _curriculumStudentRepository = curriculumStudentRepository;
            _homeWorkConfigRepository = homeWorkConfigRepository;
            _requestSafeCachingService = requestSafeCachingService;
        }

        public async Task<MethodResult<bool>> Handle(StartHomeWorkExtraPracticeCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();
            var studentResult = await GetStudentAsync();
            if (!studentResult.IsOK)
            {
                methodResult.AddErrorBadRequest(studentResult.ErrorMessages);
                return methodResult;
            }
            var student = studentResult.Result!;

            var homeWork = await _homeWorkRepository.GetByIdAsync(request.HomeWorkId);
            if (homeWork == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(homeWork), request.HomeWorkId);
                return methodResult;
            }
            var curriculumId = await GetCurriculumConfigIdAsync(student, cancellationToken);

            var homeWorkRetry = await GetExamPracticeRetryAsync(homeWork, student, request.HomeWorkConfigId, curriculumId, cancellationToken);
            await SaveHomeWorkExtraPracticeResultAsync(homeWorkRetry);

            methodResult.Result = true;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<Guid?> GetCurriculumConfigIdAsync(StudentModel student, CancellationToken cancellationToken)
        {
            Guid? curriculumId = await (from baseQ in _curriculumRepository.Queryable
                                        join cs in _curriculumStudentRepository.Queryable on baseQ.Id equals cs.CurriculumId
                                        where baseQ.CourseCloneId == student.CourseId && cs.StudentId == student.Id
                                        select baseQ.Id).FirstOrDefaultAsync(cancellationToken);

            return curriculumId.HasValue && curriculumId.Value != Guid.Empty ? curriculumId.Value : null;
        }

        private async Task SaveHomeWorkExtraPracticeResultAsync(HomeWorkRetry homeWorkRetry)
        {
            var homeWorkExtraPracticeResult = await _homeWorkExtraPracticeResultRepository.Queryable
                                                        .Where(x => x.HomeWorkRetryId == homeWorkRetry.Id && x.WorkingStatus == EnumWorkingStatus.Active)
                                                        .FirstOrDefaultAsync();
            if (homeWorkExtraPracticeResult != null)
            {
                return;
            }
            homeWorkExtraPracticeResult = new HomeWorkExtraPracticeResult
            {
                StudentId = homeWorkRetry.StudentId,
                HomeWorkId = homeWorkRetry.HomeWorkId,
                WorkingStatus = EnumWorkingStatus.Active,
                SubmissionCount = EnumSubmissionCount.FirstSubmit,
                HomeWorkRetryId = homeWorkRetry.Id,
            };
            try
            {
                await _requestSafeCachingService.SafeRequest<HomeWorkExtraPracticeResult>(
                    key: $"Add_HomeWorkExtraPracticeResult_{homeWorkExtraPracticeResult.StudentId}_{homeWorkExtraPracticeResult.HomeWorkId}_{homeWorkExtraPracticeResult.HomeWorkRetryId}_{homeWorkExtraPracticeResult.WorkingStatus}_{homeWorkExtraPracticeResult.IsDeleted}",
                    safeFunction: async () =>
                    {
                        await _homeWorkExtraPracticeResultRepository.BulkMergeAsync(new List<HomeWorkExtraPracticeResult> { homeWorkExtraPracticeResult }, bulk =>
                        {
                            bulk.ColumnPrimaryKeyExpression = c => new { c.StudentId, c.HomeWorkId, c.HomeWorkRetryId, c.WorkingStatus, c.IsDeleted };
                        });
                        return homeWorkExtraPracticeResult;
                    });
            }
            catch
            {
            }
        }

        private async Task<HomeWorkRetry> GetExamPracticeRetryAsync(HomeWork homeWork, StudentModel student, Guid? homeWorkConfigId, Guid? curriculumId, CancellationToken cancellationToken)
        {
            var homeWorkRetry = await _homeWorkRetryRepository.Queryable.FirstOrDefaultAsync(x => x.HomeWorkId == homeWork.Id && x.HomeWorkConfigId == homeWorkConfigId && x.StudentId == student.Id, cancellationToken);
            var retry = await GetNumberRetryAsync(curriculumId, homeWorkConfigId, cancellationToken);
            if (homeWorkRetry == null)
            {
                homeWorkRetry = new HomeWorkRetry
                {
                    StudentId = student.Id,
                    HomeWorkId = homeWork.Id,
                    HomeWorkConfigId = homeWorkConfigId,
                    CurriculumId = curriculumId,
                    NumberRetry = retry
                };
                try
                {
                    await _requestSafeCachingService.SafeRequest<HomeWorkRetry>(
                        key: $"Add_HomeWorkRetry_{homeWorkRetry.StudentId}_{homeWorkRetry.HomeWorkId}_{homeWorkRetry.CurriculumId}_{homeWorkRetry.HomeWorkConfigId}",
                        safeFunction: async () =>
                        {
                            await _homeWorkRetryRepository.BulkMergeAsync(new List<HomeWorkRetry> { homeWorkRetry }, bulk =>
                            {
                                bulk.ColumnPrimaryKeyExpression = c => new { c.StudentId, c.HomeWorkId, c.CurriculumId, c.HomeWorkConfigId };
                            });
                            return homeWorkRetry;
                        });
                }
                catch
                {
                }
            }
            return homeWorkRetry;
        }

        private async Task<int> GetNumberRetryAsync(Guid? curriculumId, Guid? homeWorkConfigId, CancellationToken cancellationToken)
        {
            var homeWorkConfig = await _homeWorkConfigRepository.Queryable.FirstOrDefaultAsync(x => x.CurriculumId == curriculumId && x.Id == homeWorkConfigId, cancellationToken);
            if (homeWorkConfig == null)
            {
                return MaxRetry;
            }
            return homeWorkConfig.NumberRetry;
        }

        private async Task<MethodResult<StudentModel>> GetStudentAsync()
        {
            var methodResult = new MethodResult<StudentModel>();
            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }

            var student = studentResult?.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }
            methodResult.Result = student;
            return methodResult;
        }
    }
}
