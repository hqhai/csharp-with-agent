namespace Fsel.System.Application.Queries.DailyQuiz
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.System.Application.Services.UserServices;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.IRepositories.DailyQuizs;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetDailyQuizWinnerInDayQuery : IRequest<MethodResult<DailyQuizWinnerModels>>
    {
    }

    public class GetDailyQuizWinnerInDayQueryHandler : IRequestHandler<GetDailyQuizWinnerInDayQuery, MethodResult<DailyQuizWinnerModels>>
    {
        private readonly IDailyQuizWinnerRepository _dailyQuizWinnerRepository;
        private readonly IUserService _userService;
        private readonly ICrmLocationRepository _crmLocationRepository;
        private readonly AuthContext _authContext;

        public GetDailyQuizWinnerInDayQueryHandler(IDailyQuizWinnerRepository dailyQuizWinnerRepository, IUserService userService, ICrmLocationRepository crmLocationRepository, AuthContext authContext)
        {
            _dailyQuizWinnerRepository = dailyQuizWinnerRepository;
            _userService = userService;
            _crmLocationRepository = crmLocationRepository;
            _authContext = authContext;
        }

        public async Task<MethodResult<DailyQuizWinnerModels>> Handle(GetDailyQuizWinnerInDayQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<DailyQuizWinnerModels>();

            var currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);

            var minCreatedDate = currentDate.Date.AddHours(-7);

            var dailyQuizWinners = await _dailyQuizWinnerRepository.Queryable.Where(p => p.CreatedDate >= minCreatedDate).ToListAsync(cancellationToken);

            var winners = dailyQuizWinners.Where(p => p.IsWin).ToList();

            var userIds = winners.Select(p => p.CreatedUserId).ToList();

            var result = new DailyQuizWinnerModels();

            result.Code = dailyQuizWinners.FirstOrDefault(p => p.CreatedUserId == _authContext.CurrentUserId)?.Code;

            var studentModels = new List<DailyQuizWinnerModel>();

            if (userIds.Any())
            {
                var studentResults = await _userService.GetStudentsByUserIds(userIds);
                var students = studentResults.Content?.Result;

                var schoolIds = winners.Select(p => p.SchoolId).ToList();

                var schools = await _crmLocationRepository.Queryable.WhereBulkContains(schoolIds, p => p.GlobalId).ToListAsync(cancellationToken);

                students.ForEach(p =>
                {
                    var winner = winners.FirstOrDefault(x => x.CreatedUserId == p.Human?.UserId);
                    if (winner != null)
                    {
                        var school = schools.FirstOrDefault(x => x.GlobalId == winner.SchoolId);
                        studentModels.Add(new DailyQuizWinnerModel()
                        {
                            UserId = winner.CreatedUserId,
                            FullName = p.Human?.FullName,
                            School = school?.Name,
                            Code = winner.Code
                        });
                    }
                });

                result.Winner = studentModels;
            }
            methodResult.Result = result;
            return methodResult;
        }
    }
}
