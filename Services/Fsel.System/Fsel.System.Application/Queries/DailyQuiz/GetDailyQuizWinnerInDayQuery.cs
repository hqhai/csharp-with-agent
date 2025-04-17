namespace Fsel.System.Application.Queries.DailyQuiz
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.System.Application.Services.UserServices;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.IRepositories.DailyQuizs;
    using Fsel.System.Domain.Models.EntityModels;
    using Fsel.System.Infrastructure.Repositories;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetDailyQuizWinnerInDayQuery : IRequest<MethodResult<IList<DailyQuizWinnerModel>>>
    {
    }

    public class GetDailyQuizWinnerInDayQueryHandler : IRequestHandler<GetDailyQuizWinnerInDayQuery, MethodResult<IList<DailyQuizWinnerModel>>>
    {
        private readonly IDailyQuizWinnerRepository _dailyQuizWinnerRepository;
        private readonly IUserService _userService;
        private readonly ICrmLocationRepository _crmLocationRepository;

        public GetDailyQuizWinnerInDayQueryHandler(IDailyQuizWinnerRepository dailyQuizWinnerRepository, IUserService userService, ICrmLocationRepository crmLocationRepository)
        {
            _dailyQuizWinnerRepository = dailyQuizWinnerRepository;
            _userService = userService;
            _crmLocationRepository = crmLocationRepository;
        }

        public async Task<MethodResult<IList<DailyQuizWinnerModel>>> Handle(GetDailyQuizWinnerInDayQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<DailyQuizWinnerModel>>();

            var currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);

            var winners = await _dailyQuizWinnerRepository.Queryable.Where(p => p.CreatedDate.Date == currentDate.Date && p.IsWin).ToListAsync(cancellationToken);

            var userIds = winners.Select(p => p.CreatedUserId).ToList();

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
                            FullName = p.Human?.FullName,
                            School = school?.Name,
                            Code = winner.Code
                        });
                    }
                });
            }
            methodResult.Result = studentModels;
            return methodResult;
        }
    }
}
