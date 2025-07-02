// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.GoogleSheetQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Shared.Constants;
    using Fsel.Identity.Application.Services.GoogleSheetServices;
    using Fsel.Identity.Infrastructure.ValueSettings;
    using MediatR;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Identity.Domain.IRepositories;
    using Microsoft.EntityFrameworkCore;

    public class GetListStudentSchoolQuery : IRequest<MethodResult<IList<StudentJoinCompetitionModel>>>
    {
        public string? SchoolCode { get; set; }
    }

    public class GetListStudentSchoolQueryHandler : IRequestHandler<GetListStudentSchoolQuery, MethodResult<IList<StudentJoinCompetitionModel>>>
    {
        private readonly IGoogleSheetService _googleSheetService;
        private readonly IStudentRepository _studentRepository;
        private readonly AppSetting _appSetting;

        public GetListStudentSchoolQueryHandler(IStudentRepository studentRepository, AppSetting appSetting)
        {
            _googleSheetService = new GoogleSheetService(ResourceSettings.StudentCredentialsFilePath);
            _studentRepository = studentRepository;
            _appSetting = appSetting;
        }

        public async Task<MethodResult<IList<StudentJoinCompetitionModel>>> Handle(GetListStudentSchoolQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<StudentJoinCompetitionModel>>();

            var googleSheetId = _appSetting.GoogleSheetConfig?.SchoolStudentSheetId;

            if (string.IsNullOrEmpty(googleSheetId))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            if (string.IsNullOrEmpty(request.SchoolCode))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }
            var students = new List<StudentJoinCompetitionModel>();
            try
            {
                IList<IList<object>> dataStudent = _googleSheetService.ReadDataFromSheet(googleSheetId, request.SchoolCode);

                if (dataStudent == null || dataStudent.Count < 2)
                {
                    return methodResult;
                }

                // Lấy headers từ dòng đầu tiên
                var headers = dataStudent[0].Select(h => h.ToString()).ToList();


                for (int i = 1; i < dataStudent.Count; i++)
                {
                    var row = dataStudent[i];
                    //if (row.Count != headers.Count)
                    //{
                    //    continue; // Bỏ qua dòng nếu số cột không khớp
                    //}
                    //var a = headers.IndexOf("SchoolName");
                    var student = new StudentJoinCompetitionModel
                    {
                        FullName = row[headers.IndexOf("FullName")].ToString(),
                        Email = row.Count > 1 ? row[headers.IndexOf("Email")].ToString() : string.Empty,
                        Grade = row.Count > 2 ? (double.TryParse(row[headers.IndexOf("Grade")].ToString(), out double grade) ? grade : 0) : 0,
                        SchoolName = row.Count > 3 ? row[headers.IndexOf("SchoolName")].ToString() : string.Empty
                    };

                    if (!students.Any(x => x.Email == student.Email))
                    {
                        students.Add(student);
                    }
                }

                var existsStudent = _studentRepository.Queryable.Include(x => x.User)
                                                             .Where(x => x.User != null && students.Select(y => y.Email).Contains(x.User!.Email))
                                                             .Select(x => new
                                                             {
                                                                 StudentId = x.Id,
                                                                 UserId = x!.User!.Id,
                                                                 Email = x!.User!.Email
                                                             }).ToList();

                // Gán StudentId và UserId vào danh sách students
                students.ForEach(student =>
                {
                    var match = existsStudent.FirstOrDefault(es => es.Email == student.Email);
                    if (match != null)
                    {
                        student.StudentId = match.StudentId;
                        student.UserId = match.UserId;
                    }
                });
            }
            catch (Exception ex)
            {
                methodResult.AddErrorBadRequest(ex.Message);
                return methodResult;
            }

            methodResult.Result = students;
            return methodResult;
        }
    }
}
