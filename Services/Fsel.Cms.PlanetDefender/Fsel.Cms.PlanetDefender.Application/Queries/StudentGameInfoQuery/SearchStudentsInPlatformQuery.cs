using Fsel.Cms.PlanetDefender.Application.Services.UserServices;
using Fsel.Cms.PlanetDefender.Application.Services.UserServices.Models;
using Fsel.Cms.PlanetDefender.Domain.IRepositories;
using Fsel.Cms.PlanetDefender.Domain.Models.QueryModels.StudentGameInfos;
using Fsel.Common.ActionResults;
using Fsel.Core.Base.BaseModels;
using Fsel.Core.Extensions;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Fsel.Cms.PlanetDefender.Application.Queries.StudentGameInfoQuery
{
    public class SearchStudentsInPlatformQuery : SearchStudentsInPlatformQueryModel, IRequest<MethodResult<PagingItemsModel<StudentInPlatformModel>>>
    {
    }

    public class SearchStudentsInPlatformQueryHandler : IRequestHandler<SearchStudentsInPlatformQuery, MethodResult<PagingItemsModel<StudentInPlatformModel>>>
    {
        private readonly IStudentGameInfoRepository _studentGameInfoRepository;
        private readonly IUserService _userService;

        public SearchStudentsInPlatformQueryHandler(IStudentGameInfoRepository studentGameInfoRepository, IUserService userService)
        {
            _studentGameInfoRepository = studentGameInfoRepository;
            _userService = userService;
        }

        public async Task<MethodResult<PagingItemsModel<StudentInPlatformModel>>> Handle(SearchStudentsInPlatformQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<StudentInPlatformModel>>();

            var studentsInPlatformResult = await _userService.GetStudentsInPlatform(new GetStudentInPlatformQueryModel
            {
                Keyword = request.Keyword,
                PlatformCode = request.PlatformCode,
                Role = request.Role,
                Status = request.UserPlatformStatus
            });
            var studentsInPlatform = studentsInPlatformResult.Content?.Result;

            if (studentsInPlatform == null)
            {
                return methodResult;
            }

            studentsInPlatform.ForEach(p =>
            {
                p.Level = _studentGameInfoRepository.Queryable.FirstOrDefault(x => x.StudentId == p.StudentId)?.Level;
            });

            if (request.Level.HasValue)
            {
                studentsInPlatform = studentsInPlatform.Where(p => p.Level == request.Level).ToList();
            }

            int totalItem = studentsInPlatform.Count;

            var lists = studentsInPlatform
                    .ApplySortAndPaging(request)
                    .ToList();

            methodResult.Result = new PagingItemsModel<StudentInPlatformModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
