// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.ActionResults;
using Fsel.Shared.Enums;
using Fsel.Shared.Helpers;
using Fsel.Training.Domain.IRepositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Training.Application.Queries.ClassQuery
{
    public class GetNewClassCodeQuery : IRequest<MethodResult<string>>
    {
        public EnumCourseLevel? CourseLevel { get; set; }
        public string? Code { get; set; }
    }

    public class GetNewClassCodeQueryHandler : IRequestHandler<GetNewClassCodeQuery, MethodResult<string>>
    {
        private readonly IClassRepository _classRepository;

        public GetNewClassCodeQueryHandler(IClassRepository classRepository)
        {
            _classRepository = classRepository;
        }

        public async Task<MethodResult<string>> Handle(GetNewClassCodeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<string> methodResult = new MethodResult<string>();

            var currentDate = DateTime.Now;
            var weekNumber = (currentDate.DayOfYear - 1) / 7 + 1;
            var lastDigitOfYear = currentDate.Year % 10;
            var level = request.CourseLevel.GetCodeByEnumCourseLevel();
            var stt = await _classRepository.Queryable.CountAsync(cancellationToken: cancellationToken);
            string codeClass = $"{level}_{weekNumber}{lastDigitOfYear}{stt:000}{request.Code}S";
            methodResult.Result = codeClass;
            return methodResult;
        }
    }
}
