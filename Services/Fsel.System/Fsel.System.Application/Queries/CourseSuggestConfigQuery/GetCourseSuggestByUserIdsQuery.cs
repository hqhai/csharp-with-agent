// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.CourseSuggestConfigQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels.IntegrationModels;
    using Fsel.System.Domain.Models.QueryModels.IntegrationModels;
    using global::System.Threading;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetCourseSuggestByUserIdsQuery : GetCourseSuggestByUserIdsQueryModel, IRequest<MethodResult<IList<CourseSuggestUsersModel>>>
    {
    }

    public class GetCourseSuggestByUserIdsQueryHandler : IRequestHandler<GetCourseSuggestByUserIdsQuery, MethodResult<IList<CourseSuggestUsersModel>>>
    {
        private readonly ICourseSuggestConfigRepository _courseSuggestConfigRepository;
        private readonly IMapper _mapper;

        public GetCourseSuggestByUserIdsQueryHandler(ICourseSuggestConfigRepository courseSuggestConfigRepository,
                                                     IMapper mapper)
        {
            _courseSuggestConfigRepository = courseSuggestConfigRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<CourseSuggestUsersModel>>> Handle(GetCourseSuggestByUserIdsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<CourseSuggestUsersModel>> methodResult = new MethodResult<IList<CourseSuggestUsersModel>>();

            if (request.GetCourseSuggestByUserIds == null || !request.GetCourseSuggestByUserIds.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            IList<CourseSuggestUsersModel> courseSuggestUsers = new List<CourseSuggestUsersModel>();

            foreach (var item in request.GetCourseSuggestByUserIds)
            {
                CourseSuggestUsersModel courseSuggest = new CourseSuggestUsersModel();

                var courseSuggestConfigs = await _courseSuggestConfigRepository.Queryable
                                                                               .Where(x => x.PlacementTestLevel == item.BaseCourseLevel && x.FromAge <= item.Age && x.ToAge >= item.Age)
                                                                               .ToListAsync(cancellationToken);
                if (courseSuggestConfigs == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                    return methodResult;
                }

                courseSuggest.UserId = item.UserId;
                _mapper.Map(courseSuggestConfigs, courseSuggest.CourseSuggestConfigs);
                courseSuggestUsers.Add(courseSuggest);
            }

            methodResult.Result = courseSuggestUsers;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
