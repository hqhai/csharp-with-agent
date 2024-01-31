// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.DailyStreakCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Helpers;
    using Fsel.Core.Base;
    using Fsel.Identity.Application.Services.SystemService;
    using Fsel.Identity.Application.Services.SystemService.Model;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Shared.Enums;
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
        private readonly AuthContext _authContext;
        private readonly IStudentDailyStreakRepository _studentDailyStreakRepository;
        private readonly ISystemService _systemService;

        public ReceiveTokensStudentCommandHandler(IStudentRepository studentRepository, AuthContext authContext, IStudentDailyStreakRepository studentDailyStreakRepository, ISystemService systemService)
        {
            _studentRepository = studentRepository;
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

            //var tokenConfig = await _systemService.GetTokenConfigAsync(new GetTokenQueryModel
            //{
            //    Feature = EnumTokenFeature.DailyCheckin,
            //    Mission = EnumTokenMission.DailyCheckin
            //});
            //var tokenConfigResult = tokenConfig.Content?.Result;

            //var targetConfig = tokenConfigResult.GetTokenNumber<TokenDailyCheckIn>();
            //var targetNumber = targetConfig?.DailyCheckIns?.FirstOrDefault(x => x.Level == studentDailyStreak.LevelOfGift)?.Number;
            //if (targetNumber.HasValue)
            //{
            //    student.NumberOfToken += targetNumber.Value;
            //}

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
