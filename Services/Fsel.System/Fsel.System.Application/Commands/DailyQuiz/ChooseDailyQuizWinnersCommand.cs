using Fsel.Common.ActionResults;
using Fsel.Common.Helpers;
using Fsel.System.Domain.Enums;
using Fsel.System.Domain.IRepositories.DailyQuizs;
using Fsel.System.Infrastructure.ValueSettings;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fsel.System.Application.Commands.DailyQuiz
{
    public class ChooseDailyQuizWinnersCommand : IRequest<MethodResult<bool>>
    {
    }

    public class ChooseDailyQuizWinnersCommandHandler : IRequestHandler<ChooseDailyQuizWinnersCommand, MethodResult<bool>>
    {
        private readonly IDailyQuizWinnerRepository _dailyQuizWinnerRepository;
        private readonly AppSetting _appSetting;

        public ChooseDailyQuizWinnersCommandHandler(IDailyQuizWinnerRepository dailyQuizWinnerRepository, AppSetting appSetting)
        {
            _dailyQuizWinnerRepository = dailyQuizWinnerRepository;
            _appSetting = appSetting;
        }

        public async Task<MethodResult<bool>> Handle(ChooseDailyQuizWinnersCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var numberWinner = _appSetting.DailyQuizConfig?.NumberWinner;

            if (!numberWinner.HasValue)
            {
                methodResult.AddErrorBadRequest(nameof(EnumDailyQuizErrorCode.MissingEventConfiguration), nameof(EnumDailyQuizErrorCode.MissingEventConfiguration), EnumDailyQuizErrorCode.MissingEventConfiguration.GetDescription());
                return methodResult;
            }

            var currentDate = DateTime.UtcNow.ConvertTimeToUtc(EnumCountryKey.Vietnam).Date;

            var dailyQuizWinners = await _dailyQuizWinnerRepository.Queryable
                .Where(p => p.IsWin)
                .Select(p => new { p.SchoolId, p.CreatedUserId })
                .ToListAsync(cancellationToken);

            var schoolIdSet = new HashSet<Guid>(dailyQuizWinners.Select(p => p.SchoolId));
            var userIdSet = new HashSet<Guid>(dailyQuizWinners.Select(p => p.CreatedUserId));

            var dailyQuizWinnersInDay = await _dailyQuizWinnerRepository.Queryable
                .Where(p => !p.IsWin && p.CreatedDate.AddHours(7).Date == currentDate)
                .ToListAsync(cancellationToken);

            if (dailyQuizWinnersInDay.Count > numberWinner.Value)
            {
                var filtered = dailyQuizWinnersInDay
                    .Where(p => !schoolIdSet.Contains(p.SchoolId))
                    .ToList();

                if (filtered.Count > numberWinner.Value)
                {
                    filtered = filtered
                        .Where(p => !userIdSet.Contains(p.CreatedUserId))
                        .ToList();
                }

                dailyQuizWinnersInDay = filtered.Take(numberWinner.Value).ToList();
            }

            await _dailyQuizWinnerRepository.ExecuteTransactionAsync(async () =>
            {
                dailyQuizWinnersInDay.ForEach(p => p.IsWin = true);
                _dailyQuizWinnerRepository.UpdateList(dailyQuizWinnersInDay);
                await _dailyQuizWinnerRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                return methodResult;
            });

            return methodResult;
        }
    }
}
