// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.DailyStreakCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Identity.Application.Queues.Publishers;
    using Fsel.Identity.Application.Services.LmsCourseService;
    using Fsel.Identity.Application.Services.SystemService;
    using Fsel.Identity.Application.Services.SystemService.Model;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ReceiveTokensStudentCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class ReceiveTokensStudentCommandHandler : IRequestHandler<ReceiveTokensStudentCommand, MethodResult<bool>>
    {
        private readonly IStudentRepository _studentRepository;
        private readonly CreateTokenHistoryPublisher _createTokenHistoryPublisher;
        private readonly ILmsCourseService _lmsCourseService;
        private readonly AuthContext _authContext;
        private readonly IStudentDailyStreakRepository _studentDailyStreakRepository;
        private readonly ISystemService _systemService;

        public ReceiveTokensStudentCommandHandler(IStudentRepository studentRepository, CreateTokenHistoryPublisher createTokenHistoryPublisher, ILmsCourseService lmsCourseService, AuthContext authContext, IStudentDailyStreakRepository studentDailyStreakRepository, ISystemService systemService)
        {
            _studentRepository = studentRepository;
            _createTokenHistoryPublisher = createTokenHistoryPublisher;
            _lmsCourseService = lmsCourseService;
            _authContext = authContext;
            _studentDailyStreakRepository = studentDailyStreakRepository;
            _systemService = systemService;
        }

        public async Task<MethodResult<bool>> Handle(ReceiveTokensStudentCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();
            var student = await _studentRepository.Queryable.Include(x => x.StudentDailyStreaks).Include(x => x.Human)
                                                  .FirstOrDefaultAsync(x => x.Human != null && x.Human.UserId == _authContext.CurrentUserId, cancellationToken: cancellationToken);
            if (student == null)
            {
                methodResult.Result = false;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var studentDailyStreak = student.StudentDailyStreaks.FirstOrDefault(x => x.Id == request.Id && x.LevelOfGift != null && !x.IsGiftReceive);
            if (studentDailyStreak == null)
            {
                methodResult.Result = false;
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
            if (tokenConfigResult != null && tokenConfigDailyCheckIns != null)
            {
                var targetNumber = tokenConfigDailyCheckIns.Where(x => x.Level == studentDailyStreak.LevelOfGift).Max(x => x.BaseValue);
                await _createTokenHistoryPublisher.Publish(new List<TokenHistoryQueueModel>
                {
                    new TokenHistoryQueueModel
                    {
                        ObjectId = studentDailyStreak.Id,
                        RemainToken = targetNumber,
                        Type = EnumTokenHistoryType.Exchanged,
                        Feature = EnumTokenFeature.DailyCheckin,
                        Mission = EnumTokenMission.DailyCheckin,
                        UserId = _authContext.CurrentUserId,
                    }
                }, cancellationToken).ConfigureAwait(false);

                student.NumberOfToken += targetNumber;
            }

            await _studentDailyStreakRepository.ExecuteTransactionAsync(async () =>
             {
                 _studentRepository.Update(student);
                 await _studentRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                 _studentDailyStreakRepository.Update(studentDailyStreak);
                 await _studentDailyStreakRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                 methodResult.StatusCode = StatusCodes.Status200OK;
                 methodResult.Result = true;
                 return methodResult;
             });
            return methodResult;
        }
    }
}
