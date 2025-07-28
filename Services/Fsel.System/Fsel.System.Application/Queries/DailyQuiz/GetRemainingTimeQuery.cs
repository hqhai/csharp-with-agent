using Fsel.Common.ActionResults;
using Fsel.Common.Helpers;
using Fsel.System.Domain.Enums;
using Fsel.System.Infrastructure.ValueSettings;
using MediatR;

namespace Fsel.System.Application.Queries.DailyQuiz
{
    public class GetRemainingTimeQuery : IRequest<MethodResult<int>>
    {
    }

    public class GetRemainingTimeQueryHandler : IRequestHandler<GetRemainingTimeQuery, MethodResult<int>>
    {
        private readonly AppSetting _appSetting;

        public GetRemainingTimeQueryHandler(AppSetting appSetting)
        {
            _appSetting = appSetting;
        }

        public async Task<MethodResult<int>> Handle(GetRemainingTimeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<int>();
            var endHour = _appSetting.DailyQuizConfig?.EndHour;
            if (!endHour.HasValue)
            {
                methodResult.AddErrorBadRequest(nameof(EnumDailyQuizErrorCode.MissingEventConfiguration), nameof(EnumDailyQuizErrorCode.MissingEventConfiguration), EnumDailyQuizErrorCode.MissingEventConfiguration.GetDescription());
                return methodResult;
            }
            var currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);
            var targetTime = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, endHour.Value, 0, 0);
            double remainingSeconds = (targetTime - currentDate).TotalSeconds;
            int secondsLeft = (int)Math.Max(0, Math.Floor(remainingSeconds));
            methodResult.Result = secondsLeft;
            return methodResult;
        }
    }
}
