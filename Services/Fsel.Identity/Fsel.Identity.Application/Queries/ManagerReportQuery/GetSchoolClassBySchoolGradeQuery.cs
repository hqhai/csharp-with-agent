// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.ManagerReportQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetSchoolClassBySchoolGradeQuery : IRequest<MethodResult<IList<string>>>
    {
        public string? SchoolGrade { get; set; }

        public IList<string>? ListSchoolGrade
        {
            get
            {
                return SchoolGrade.ToList<string>();
            }
        }
    }

    public class GetSchoolClassBySchoolGradeQueryHandler : IRequestHandler<GetSchoolClassBySchoolGradeQuery, MethodResult<IList<string>>>
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IUserSchoolRepository _userSchoolRepository;
        private readonly AuthContext _authContext;

        public GetSchoolClassBySchoolGradeQueryHandler(IStudentRepository studentRepository,
            IUserSchoolRepository userSchoolRepository,
            AuthContext authContext)
        {
            _studentRepository = studentRepository;
            _userSchoolRepository = userSchoolRepository;
            _authContext = authContext;
        }

        public async Task<MethodResult<IList<string>>> Handle(GetSchoolClassBySchoolGradeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<string>> methodResult = new MethodResult<IList<string>>();

            request.SchoolGrade = request.SchoolGrade?.Trim().ToLower(System.Globalization.CultureInfo.CurrentCulture);
            var query = _studentRepository.Queryable.Where(x => !string.IsNullOrEmpty(x.SchoolClass));

            if (request.ListSchoolGrade != null && request.ListSchoolGrade.Any())
            {
                query = query.WhereBulkContains(request.ListSchoolGrade, x => x.SchoolGrade);
            }

            if (_authContext.Roles != null && _authContext.Roles.Contains(EnumRole.AdminSchool.ToString()))
            {
                var schoolId = await _userSchoolRepository.GetSchoolIdAsync();
                query = query.Where(x => x.SchoolId.HasValue && x.SchoolId == schoolId);
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
