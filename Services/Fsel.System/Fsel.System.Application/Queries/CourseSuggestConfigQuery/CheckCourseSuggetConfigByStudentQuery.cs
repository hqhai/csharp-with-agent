// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.CourseSuggestConfigQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CheckCourseSuggetConfigByStudentQuery : IRequest<MethodResult<bool>>
    {
        public EnumCourseLevel BaseCourseLevel { get; set; }

        public EnumCourseLevel ChooseCourseLevel { get; set; }

        public int Age { get; set; }
    }

    public class CheckCourseSuggetConfigByStudentQueryHandler : IRequestHandler<CheckCourseSuggetConfigByStudentQuery, MethodResult<bool>>
    {
        private readonly ICourseSuggestConfigRepository _courseSuggestConfigRepository;

        public CheckCourseSuggetConfigByStudentQueryHandler(ICourseSuggestConfigRepository courseSuggestConfigRepository)
        {
            _courseSuggestConfigRepository = courseSuggestConfigRepository;
        }
        public async Task<MethodResult<bool>> Handle(CheckCourseSuggetConfigByStudentQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var courseSuggestConfigs = await _courseSuggestConfigRepository.Queryable
                                                                           .Where(x => x.PlacementTestLevel == request.BaseCourseLevel && x.FromAge <= request.Age && x.ToAge >= request.Age)
                                                                           .ToListAsync(cancellationToken);
            if (courseSuggestConfigs == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(courseSuggestConfigs));
                return methodResult;
            }

            if (!courseSuggestConfigs.Any(x => x.CourseLevels != null && x.CourseLevels.Any(c => c == request.ChooseCourseLevel)))
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseSuggestConfigErrorCode.YouChoseTheWrongLevel), nameof(request.ChooseCourseLevel), nameof(request.ChooseCourseLevel));
                return methodResult;
            }

            methodResult.Result = true;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
