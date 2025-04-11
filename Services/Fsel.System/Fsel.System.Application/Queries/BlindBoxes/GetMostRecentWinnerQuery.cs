namespace Fsel.System.Application.Queries.BlindBoxes
{
    using Fsel.Common.ActionResults;
    using Fsel.System.Application.Services.UserServices;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.IRepositories.BlindBoxes;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetMostRecentWinnerQuery : IRequest<MethodResult<string?>>
    {
    }

    public class GetMostRecentWinnerQueryHandler : IRequestHandler<GetMostRecentWinnerQuery, MethodResult<string?>>
    {
        private readonly IBlindBoxHistoryRepository _blindBoxHistoryRepository;
        private readonly IUserService _userService;
        private readonly ICrmLocationRepository _crmLocationRepository;

        public GetMostRecentWinnerQueryHandler(IBlindBoxHistoryRepository blindBoxHistoryRepository, IUserService userService, ICrmLocationRepository crmLocationRepository)
        {
            _blindBoxHistoryRepository = blindBoxHistoryRepository;
            _userService = userService;
            _crmLocationRepository = crmLocationRepository;
        }

        public async Task<MethodResult<string?>> Handle(GetMostRecentWinnerQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<string?>();

            var mostWinner = await _blindBoxHistoryRepository.Queryable.Where(p => !string.IsNullOrEmpty(p.Code)).OrderByDescending(p => p.CreatedDate).FirstOrDefaultAsync(cancellationToken);
            if (mostWinner != null)
            {
                var userResult = await _userService.GetStudentByUserIdAsync(mostWinner.CreatedUserId);
                if (!userResult.IsSuccessStatusCode)
                {
                    methodResult.Result = null;
                    return methodResult;
                }
                var user = userResult.Content?.Result;
                if (user == null)
                {
                    methodResult.Result = null;
                    return methodResult;
                }

                var fullName = user.Human?.FullName;

                if (user.DistrictId.HasValue)
                {
                    var district = await _crmLocationRepository.Queryable.FirstOrDefaultAsync(p => p.GlobalId == user.DistrictId, cancellationToken);
                    if (district != null)
                    {
                        var province = await _crmLocationRepository.Queryable.FirstOrDefaultAsync(p => p.Id == district.ParentId, cancellationToken);
                        if (province != null)
                        {
                            fullName = $"{fullName} ({district.Name}, {province.Name})";
                        }
                    }
                }
                methodResult.Result = fullName;
                return methodResult;
            }
            else
            {
                methodResult.Result = null;
                return methodResult;
            }
        }
    }
}
