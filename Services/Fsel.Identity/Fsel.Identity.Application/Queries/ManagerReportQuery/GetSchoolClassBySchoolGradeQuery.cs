// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.ManagerReportQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetSchoolClassBySchoolGradeQuery : IRequest<MethodResult<IList<string>>>
    {
        public string? SchoolGrade { get; set; }
    }

    public class GetSchoolClassBySchoolGradeQueryHandler : IRequestHandler<GetSchoolClassBySchoolGradeQuery, MethodResult<IList<string>>>
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IUserSchoolRepository _userSchoolRepository;

        public GetSchoolClassBySchoolGradeQueryHandler(IStudentRepository studentRepository, IUserSchoolRepository userSchoolRepository)
        {
            _studentRepository = studentRepository;
            _userSchoolRepository = userSchoolRepository;
        }

        public async Task<MethodResult<IList<string>>> Handle(GetSchoolClassBySchoolGradeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<string>> methodResult = new MethodResult<IList<string>>();

            var schoolId = await _userSchoolRepository.GetSchoolIdAsync();
            request.SchoolGrade = request.SchoolGrade?.Trim().ToLower(System.Globalization.CultureInfo.CurrentCulture);
            var query = _studentRepository.Queryable.Where(x => string.IsNullOrEmpty(request.SchoolGrade) || (x.SchoolGrade ?? string.Empty).Trim().ToLower() == request.SchoolGrade)
                                                    .Where(x => !string.IsNullOrEmpty(x.SchoolClass));

            if (schoolId.HasValue)
            {
                query = query.Where(x => x.SchoolId.HasValue && x.SchoolId == schoolId.Value);
            }

            var schoolClass = await query.Select(x => x.SchoolClass!).Distinct().ToListAsync(cancellationToken);
            methodResult.Result = schoolClass.OrderBy(x =>
            {
                // Tách phần số ra
                var numberPart = new string(x.Where(char.IsDigit).ToArray());
                return string.IsNullOrEmpty(numberPart) ? int.MaxValue : int.Parse(numberPart);
            })
            .ThenBy(y =>
            {
                // Tách phần chữ cái sau số
                var letterPart = new string(y.SkipWhile(char.IsDigit).ToArray());
                return letterPart;
            }).ToList();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}