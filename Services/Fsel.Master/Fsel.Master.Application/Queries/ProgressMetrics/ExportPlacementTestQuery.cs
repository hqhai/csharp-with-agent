// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Application.Queries.ProgressMetrics
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Master.Domain.Entities;
    using Fsel.Master.Domain.IRepositories;
    using Fsel.Master.Domain.Models.QueryModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using OfficeOpenXml;

    public class ExportPlacementTestQuery : GetPlacementTestStudentQueryModel, IRequest<MethodResult<byte[]>>
    {
    }

    public class ExportPlacementTestQueryHandler : IRequestHandler<ExportPlacementTestQuery, MethodResult<byte[]>>
    {
        private readonly IMediator _mediator;
        private readonly IMasterBaseRepository<Skill> _skillRepository;

        public ExportPlacementTestQueryHandler(IMediator mediator, IMasterBaseRepository<Skill> skillRepository)
        {
            _mediator = mediator;
            _skillRepository = skillRepository;
        }

        public async Task<MethodResult<byte[]>> Handle(ExportPlacementTestQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<byte[]>();

            var searchQuery = new GetPlacementTestStudentQuery()
            {
                Keyword = request.Keyword,
                DistrictIds = request.DistrictIds,
                LevelIds = request.LevelIds,
                ProgramIds = request.ProgramIds,
                ProvinceIds = request.ProvinceIds,
                SchoolIds = request.SchoolIds,
                SubjectId = request.SubjectId,
            };

            searchQuery.SetIsQueryAll(true);

            var searchResult = await _mediator.Send(searchQuery, cancellationToken).ConfigureAwait(false);
            if (!searchResult.IsOK)
            {
                methodResult.AddError(searchResult.ErrorMessages);
                return methodResult;
            }

            var students = searchResult.Result?.Items;

            if (students == null || students.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var skillEntities = await _skillRepository.Queryable.ToListAsync(cancellationToken);

            var skillIds = students
                .SelectMany(p => p.SkillScores ?? Enumerable.Empty<SkillScore>())
                .Where(p => p.SkillId.HasValue)
                .Select(p => p.SkillId!.Value)
                .ToHashSet();

            var skillDict = skillEntities
                .Where(p => skillIds.Contains(p.SkillId))
                .ToDictionary(x => x.SkillId, x => x.SkillName);

            var usedSkillSet = students
                .SelectMany(s => s.SkillScores ?? new List<SkillScore>())
                .Where(sc => sc.SkillId.HasValue)
                .Select(sc => sc.SkillId!.Value)
                .Where(id => skillDict.ContainsKey(id))
                .Distinct()
                .ToList();

            var allSkills = usedSkillSet
                .Select(id => new { Id = id, Name = skillDict[id] })
                .ToList();

            var skillIndexDict = allSkills
                .Select((s, index) => new { s.Id, index })
                .ToDictionary(x => x.Id, x => x.index);

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var memoryStream = new MemoryStream();          // ✅ bỏ using
            var excelPackage = new ExcelPackage();          // ✅ bỏ using

            var ws = excelPackage.Workbook.Worksheets.Add("Students");

            int startRow = 2;
            int baseColumnCount = 7;
            int totalColumns = baseColumnCount + allSkills.Count;

            int col = 1;

            ws.Cells[1, col++].Value = "Họ và tên";
            ws.Cells[1, col++].Value = "Email";
            ws.Cells[1, col++].Value = "Số điện thoại";
            ws.Cells[1, col++].Value = "Tỉnh/ Thành phố";
            ws.Cells[1, col++].Value = "Quận/ Huyện";
            ws.Cells[1, col++].Value = "Trường";
            ws.Cells[1, col++].Value = "Trình độ hiện tại";

            foreach (var skill in allSkills)
            {
                ws.Cells[1, col++].Value = skill.Name;
            }

            var results = new object[students.Count][];

            Parallel.For(0, students.Count, i =>
            {
                var student = students[i];
                var rowValues = new object[totalColumns];

                rowValues[0] = student.FullName ?? string.Empty;
                rowValues[1] = student.Email ?? string.Empty;
                rowValues[2] = student.PhoneNumber ?? string.Empty;
                rowValues[3] = student.Province ?? string.Empty;
                rowValues[4] = student.District ?? string.Empty;
                rowValues[5] = student.School ?? string.Empty;
                rowValues[6] = student.LevelName ?? string.Empty;

                if (student.SkillScores != null)
                {
                    foreach (var sc in student.SkillScores)
                    {
                        if (sc.SkillId.HasValue &&
                            skillIndexDict.TryGetValue(sc.SkillId.Value, out int idx))
                        {
                            rowValues[baseColumnCount + idx] = sc.Percent ?? (object)string.Empty;
                        }
                    }
                }

                results[i] = rowValues;
            });

            for (int i = 0; i < results.Length; i++)
            {
                var row = startRow + i;
                var values = results[i];

                for (int c = 0; c < values.Length; c++)
                {
                    ws.Cells[row, c + 1].Value = values[c];
                }
            }

            ws.Cells.AutoFitColumns();

            excelPackage.SaveAs(memoryStream);

            var bytes = memoryStream.ToArray();

            methodResult.Result = bytes;
            return methodResult;
        }
    }
}
