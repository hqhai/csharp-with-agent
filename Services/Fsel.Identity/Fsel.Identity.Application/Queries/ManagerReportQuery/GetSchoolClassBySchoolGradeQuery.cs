// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.ManagerReportQuery
{
    using System.Text;
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
            var targetRoles = new List<string> { EnumRole.AdminSchool.ToString(), EnumRole.TeacherCampus.ToString(), EnumRole.AdminCampus.ToString() };
            var hasMatchedRole = _authContext.Roles != null && _authContext.Roles.Any(r => targetRoles.Contains(r));
            if (hasMatchedRole)
            {
                var schoolId = await _userSchoolRepository.GetSchoolIdAsync();
                query = query.Where(x => x.SchoolId.HasValue && x.SchoolId == schoolId);
            }

            var schoolClass = await query.Select(x => x.SchoolClass!).Distinct().ToListAsync(cancellationToken);

            methodResult.Result = schoolClass.OrderBy(x => FirstInt(x))
            .ThenBy(y =>
            {
                // Tách phần chữ cái sau số
                var letterPart = new string(y.SkipWhile(char.IsDigit).ToArray());
                return letterPart;
            }).ToList();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private static long? FirstInt(string? s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return null;
            }
            var sp = s.Normalize(NormalizationForm.FormKC).AsSpan(); // "lớp ９a7" -> "lớp 9a7"
            int i = 0;
            while (i < sp.Length && !char.IsDigit(sp[i]))
            {
                i++;       // tìm cụm số đầu tiên
            }
            if (i == sp.Length)
            {
                return null;
            }
            long n = 0;
            while (i < sp.Length && char.IsDigit(sp[i]))
            {
                n = n * 10 + (long)char.GetNumericValue(sp[i]);        // hỗ trợ mọi chữ số Unicode
                if (n > long.MaxValue)
                {
                    return long.MaxValue;            // clamp an toàn
                }
                i++;
            }
            return n;
        }
    }
}
