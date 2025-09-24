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
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class StartHomeWorkExtraPracticeCommand : IRequest<MethodResult<bool>>
    {
        public Guid HomeWorkId { get; set; }
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
        private const int MaxRetry = 1;

        public StartHomeWorkExtraPracticeCommandHandler(AuthContext authContext,
            IUserService userService,
            IHomeWorkExtraPracticeResultRepository homeWorkExtraPracticeResultRepository,
            IHomeWorkRepository homeWorkRepository,
            IHomeWorkRetryRepository homeWorkRetryRepository,
            ICurriculumRepository curriculumRepository,
            ICurriculumStudentRepository curriculumStudentRepository,
            IHomeWorkConfigRepository homeWorkConfigRepository)
        {
            _authContext = authContext;
            _userService = userService;
            _homeWorkExtraPracticeResultRepository = homeWorkExtraPracticeResultRepository;
            _homeWorkRepository = homeWorkRepository;
            _homeWorkRetryRepository = homeWorkRetryRepository;
            _curriculumRepository = curriculumRepository;
            _curriculumStudentRepository = curriculumStudentRepository;
            _homeWorkConfigRepository = homeWorkConfigRepository;
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

            var homeWorkRetry = await GetExamPracticeRetryAsync(homeWork, student, curriculumId, cancellationToken);
            await SaveHomeWorkExtraPracticeResultAsync(homeWorkRetry);

            methodResult.Result = true;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<Guid?> GetCurriculumConfigIdAsync(StudentModel student, CancellationToken cancellationToken)
        {
            var curriculumId = await (from baseQ in _curriculumRepository.Queryable
                                      join cs in _curriculumStudentRepository.Queryable on baseQ.Id equals cs.CurriculumId
                                      where baseQ.CourseCloneId == student.CourseId && cs.StudentId == student.Id
                                      select baseQ.Id).FirstOrDefaultAsync(cancellationToken);
            return curriculumId;
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
                HomeWorkRetryId = homeWorkRetry.Id,
            };
            try
            {
                await _homeWorkExtraPracticeResultRepository.BulkMergeAsync(new List<HomeWorkExtraPracticeResult> { homeWorkExtraPracticeResult }, bulk =>
                {
                    bulk.ColumnPrimaryKeyExpression = c => new { c.StudentId, c.HomeWorkId, c.HomeWorkRetryId, c.WorkingStatus };
                });
            }
            catch
            {
            }
        }

        private async Task<HomeWorkRetry> GetExamPracticeRetryAsync(HomeWork homeWork, StudentModel student, Guid? curriculumId, CancellationToken cancellationToken)
        {
            var homeWorkRetry = await _homeWorkRetryRepository.Queryable.FirstOrDefaultAsync(x => x.HomeWorkId == homeWork.Id && x.StudentId == student.Id, cancellationToken);
            var retry = await GetNumberRetryAsync(curriculumId, homeWorkRetry, cancellationToken);
            if (homeWorkRetry == null)
            {
                homeWorkRetry = new HomeWorkRetry
                {
                    StudentId = student.Id,
                    HomeWorkId = homeWork.Id,
                    CurriculumId = curriculumId,
                    NumberRetry = retry
                };
                try
                {
                    await _homeWorkRetryRepository.BulkMergeAsync(new List<HomeWorkRetry> { homeWorkRetry }, bulk =>
                    {
                        bulk.ColumnPrimaryKeyExpression = c => new { c.StudentId, c.HomeWorkId, c.CurriculumId };
                    });
                }
                catch
                {
                }
            }
            else if (!homeWorkRetry.CurriculumId.HasValue && curriculumId.HasValue)
            {
                homeWorkRetry.CurriculumId = curriculumId.Value;
                homeWorkRetry.NumberRetry = retry;
                try
                {
                    await _homeWorkRetryRepository.BulkUpdateList(new List<HomeWorkRetry> { homeWorkRetry }, bulk =>
                    {
                        bulk.ColumnInputExpression = c => new { c.CurriculumId, c.NumberRetry };
                    });
                }
                catch
                {
                }
            }
            return homeWorkRetry;
        }

        private async Task<int> GetNumberRetryAsync(Guid? curriculumId, HomeWorkRetry? homeWorkRetry, CancellationToken cancellationToken)
        {
            var homeWorkConfig = await _homeWorkConfigRepository.Queryable.FirstOrDefaultAsync(x => x.CurriculumId == curriculumId, cancellationToken);
            if (homeWorkConfig == null)
            {
                return MaxRetry;
            }
            if (homeWorkRetry == null)
            {
                return homeWorkConfig.NumberRetry;
            }
            var countResult = await _homeWorkExtraPracticeResultRepository.Queryable.Where(x => x.HomeWorkRetryId == homeWorkRetry.Id && x.WorkingStatus != EnumWorkingStatus.Active).CountAsync(cancellationToken);
            var retry = homeWorkConfig.NumberRetry - countResult;
            return retry > 0 ? retry : default;
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
