using Fsel.Common.ActionResults;
using Fsel.Common.Helpers;
using MediatR;

namespace Fsel.System.Application.Queries.DailyQuiz
{
    public class GetRemainingTimeQuery : IRequest<MethodResult<int>>
    {
    }

    public class GetRemainingTimeQueryHandler : IRequestHandler<GetRemainingTimeQuery, MethodResult<int>>
    {
        public GetRemainingTimeQueryHandler()
        {
        }

        public async Task<MethodResult<int>> Handle(GetRemainingTimeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<int>();
            var currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);
            var targetTime = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 21, 0, 0);
            double remainingSeconds = (targetTime - currentDate).TotalSeconds;
            int secondsLeft = (int)Math.Max(0, Math.Floor(remainingSeconds));
            methodResult.Result = secondsLeft;
            return methodResult;
        }
    }
}
