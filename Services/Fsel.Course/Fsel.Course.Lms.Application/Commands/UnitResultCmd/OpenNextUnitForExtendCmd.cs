// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.ActionResults;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Infrastructure.Repositories;
using Fsel.Course.Lms.Application.Services.UserServices;
using Fsel.Course.Lms.Application.Services.UserServices.Models;
using Fsel.Shared.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Lms.Application.Commands.UnitResultCmd
{
    public class OpenNextUnitForExtendCmd : IRequest<MethodResult<bool>>
    {
        public Guid UserId { get; set; }
    }

    public class OpenNextUnitForExtendCmdHandler : IRequestHandler<OpenNextUnitForExtendCmd, MethodResult<bool>>
    {
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly IUserService _userService;
        private readonly ICourseResultRepository _courseResultRepository;

        public OpenNextUnitForExtendCmdHandler(IUnitResultRepository unitResultRepository, IUserService userService, ICourseResultRepository courseResultRepository)
        {
            _unitResultRepository = unitResultRepository;
            _userService = userService;
            _courseResultRepository = courseResultRepository;
        }

        public async Task<MethodResult<bool>> Handle(OpenNextUnitForExtendCmd request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var studentRegistration = await _userService.GetStudentTrialRegistration(request.UserId);
            var studentRegistrationResult = studentRegistration?.Content?.Result;

            if (studentRegistrationResult != null && studentRegistrationResult.Status != EnumTrialRegistrationStatus.Payment)
            {
                StudentTrialRegistrationModel model = new StudentTrialRegistrationModel()
                {
                    UserId = request.UserId,
                    Status = EnumTrialRegistrationStatus.Payment
                };
                await UpdateTrialRegistrationStatus(model);
            }

            if (studentRegistrationResult != null && studentRegistrationResult.Status != EnumTrialRegistrationStatus.Finished)
            {
                return methodResult;
            }

            var studentResult = await _userService.GetStudentByUserIdWithCacheAsync(request.UserId);
            var student = studentResult.Content?.Result;
            var studentId = student?.Id;

            var courseResult = await _courseResultRepository.Queryable.Where(x => x.StudentId == studentId && x.WorkingStatus == EnumWorkingStatus.Active).FirstOrDefaultAsync(cancellationToken);
            if (courseResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(courseResult), studentId);
                return methodResult;
            }
            var unitResults = await _unitResultRepository.Queryable.Where(x => x.CourseResultId == courseResult.Id && x.Status == EnumResultStatus.Done).ToListAsync(cancellationToken);
            if (unitResults == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentId));
                return methodResult;
            }
            await _unitResultRepository.ExecuteTransactionAsync(async () =>
            {
                await _unitResultRepository.BulkUpdateList(unitResults, bulk =>
                {
                    bulk.IgnoreOnUpdateExpression = c => new { c.CourseId, c.StudentId, c.UnitId };
                });
                await _unitResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }

        private async Task UpdateTrialRegistrationStatus(StudentTrialRegistrationModel model)
        {
            await _userService.UpdateTrialRegistrationStatusAsync(model);
        }
    }
}
