// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.DailyStreakCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Identity.Application.Queues.Publishers;
    using Fsel.Identity.Application.Services.LmsCourseService;
    using Fsel.Identity.Application.Services.SystemService;
    using Fsel.Identity.Application.Services.SystemService.Model;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ReceiveTokensStudentCommand : IRequest<MethodResult<double?>>
    {
        public Guid Id { get; set; }
    }

    public class ReceiveTokensStudentCommandHandler : IRequestHandler<ReceiveTokensStudentCommand, MethodResult<double?>>
    {
        private readonly IStudentRepository _studentRepository;
        private readonly ILmsCourseService _lmsCourseService;
        private readonly AuthContext _authContext;
        private readonly IStudentDailyStreakRepository _studentDailyStreakRepository;
        private readonly ISystemService _systemService;
        private readonly CreateTokenHistoryPublisher _createTokenHistoryPublisher;

        public ReceiveTokensStudentCommandHandler(IStudentRepository studentRepository, ILmsCourseService lmsCourseService, AuthContext authContext, IStudentDailyStreakRepository studentDailyStreakRepository, ISystemService systemService, CreateTokenHistoryPublisher createTokenHistoryPublisher)
        {
            _studentRepository = studentRepository;
            _lmsCourseService = lmsCourseService;
            _authContext = authContext;
            _studentDailyStreakRepository = studentDailyStreakRepository;
            _systemService = systemService;
            _createTokenHistoryPublisher = createTokenHistoryPublisher;
        }

        public async Task<MethodResult<double?>> Handle(ReceiveTokensStudentCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<double?>();
            var student = await _studentRepository.Queryable.Include(x => x.StudentDailyStreaks)
                                                  .FirstOrDefaultAsync(x => x.UserId == _authContext.CurrentUserId, cancellationToken: cancellationToken);
            if (student == null)
            {
                methodResult.Result = default;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var studentDailyStreak = student.StudentDailyStreaks.FirstOrDefault(x => x.Id == request.Id && x.LevelOfGift.HasValue && !x.IsGiftReceive);
            if (studentDailyStreak == null)
            {
                methodResult.Result = default;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            var date = DateTime.UtcNow.Date;
            studentDailyStreak.IsGiftReceive = true;
            var courseResult = await _lmsCourseService.GetCourseStudied();
            var course = courseResult.Content?.Result;

            if (!courseResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallCourseServiceError));
                return methodResult;
            }
            var tokenConfig = await _systemService.GetTokenConfigAsync(new GetTokenQueryModel
            {
                Feature = EnumTokenFeature.DailyCheckin,
                Mission = EnumTokenMission.DailyCheckin,
                CourseType = course?.CourseType
            });
            var tokenConfigResult = tokenConfig.Content?.Result;

            var tokenConfigDailyCheckIns = tokenConfigResult.GetTokenConfig<List<TokenConfigDailyCheckIns>>();
            long targetNumber = default;
            if (tokenConfigResult != null && tokenConfigDailyCheckIns != null)
            {
                targetNumber = tokenConfigDailyCheckIns.Where(x => x.Level == studentDailyStreak.LevelOfGift).Max(x => x.BaseValue);
                await _createTokenHistoryPublisher.Publish(new List<TokenHistoryQueueModel>
                {
                    new TokenHistoryQueueModel
                    {
                        ObjectId = studentDailyStreak.Id,
                        VolatileToken = targetNumber,
                        Type = EnumTokenHistoryType.Recevived,
                        Feature = EnumTokenFeature.DailyCheckin,
                        Mission = GetTokenMission(studentDailyStreak),
                        UserId = _authContext.CurrentUserId,
                    }
                }, cancellationToken).ConfigureAwait(false);
            }

            await _studentDailyStreakRepository.ExecuteTransactionAsync(async () =>
            {
                _studentDailyStreakRepository.Update(studentDailyStreak);
                await _studentDailyStreakRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = targetNumber;
                return methodResult;
            });
            return methodResult;
        }

        private static EnumTokenMission? GetTokenMission(StudentDailyStreak studentDailyStreak)
        {
            if (!studentDailyStreak.LevelOfGift.HasValue)
            {
                return default;
            }
            if (studentDailyStreak.LevelOfGift == 1)
            {
                return EnumTokenMission.DailyCheckinLevelOne;
            }
            else if (studentDailyStreak.LevelOfGift == 2)
            {
                return EnumTokenMission.DailyCheckinLevelTwo;
            }
            return EnumTokenMission.DailyCheckinLevelThree;
        }
    }
}
