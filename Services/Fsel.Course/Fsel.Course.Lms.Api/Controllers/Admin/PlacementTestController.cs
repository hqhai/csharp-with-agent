// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers.Admin
{
    using System.Linq;
    using System.Net;
    using System.Text;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.PlacementTests;
    using Fsel.Course.Lms.Application.Commands.PlacementTestCmd.V1i1;
    using Fsel.Course.Lms.Application.Queries.PlacementTestQuery;
    using Fsel.Shared.Attributes;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersions(ApiSettings.APIVersion1)]
    [Route(Settings.APIDefaultRoute + "/placement-test/admin")]
    [ApiController]
    [Common.Attributes.Permission(roles: new string[] { nameof(EnumRole.Admin), nameof(EnumRole.AdminSchool), nameof(EnumRole.CSO) })]
    public class PlacementTestController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PlacementTestController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// get average pt point
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MethodResult<List<AveragePTPointModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromBody] GetAveragePTPointByIdsQuery query)
        {
            MethodResult<List<AveragePTPointModel>> queryResult = await _mediator.Send(query).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// get average pt point
        /// </summary>
        [HttpPost("get-pt-point-by-ids")]
        [ProducesResponseType(typeof(MethodResult<List<StudentPTPointModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetPTPointByIds([FromBody] IList<Guid>? ids)
        {
            MethodResult<List<StudentPTPointModel>> queryResult = await _mediator.Send(new GetPTPointByIdsQuery { StudentIds = ids }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        /// <summary>
        /// Save PlacementTest Done
        /// </summary>
        [HttpPost("save-done")]
        [ProducesResponseType(typeof(MethodResult<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SavePlacementTestDone([FromBody] SavePlacementTestDoneCommand command)
        {
            MethodResult<bool> queryResult = await _mediator.Send(command).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        [HttpGet("get-pt-results/{userId}")]
        [ProducesResponseType(typeof(MethodResult<List<PtResultDto>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetPtResults(Guid userId)
        {
            var queryResult = await _mediator.Send(new GetPtResultsQuery { UserId = userId }).ConfigureAwait(false);
            return queryResult.GetActionResult();
        }

        [HttpGet("export-pt-results/{userId}")]
        [ProducesResponseType(typeof(FileContentResult), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ExportPtResults(Guid userId)
        {
            var queryResult = await _mediator.Send(new GetPtResultsQuery { UserId = userId }).ConfigureAwait(false);
            if (!queryResult.IsOK || queryResult.Result == null)
            {
                return queryResult.GetActionResult();
            }

            string EscapeCsvField(string field)
            {
                if (string.IsNullOrEmpty(field))
                {
                    return string.Empty;
                }

                if (field.Contains(',') || field.Contains('"') || field.Contains('\n'))
                {
                    return $"\"{field.Replace("\"", "\"\"")}\"";
                }

                return field;
            }

            var allSkillNames = queryResult.Result
                .SelectMany(pr => pr.ModuleResults)
                .SelectMany(mr => mr.SkillResults)
                .Select(sr => sr.SkillName)
                .Distinct()
                .OrderBy(sn => sn)
                .ToList();

            var csv = new StringBuilder();
            var header = new List<string> { "PtResultId", "SubjectName", "CurrentLevel", "SuggestLevel", "ModuleName", "LevelOfModule" };
            header.AddRange(allSkillNames);
            csv.AppendLine(string.Join(",", header.Select(EscapeCsvField)));

            foreach (var ptResult in queryResult.Result)
            {
                if (ptResult.ModuleResults.Any())
                {
                    foreach (var moduleResult in ptResult.ModuleResults)
                    {
                        var row = new List<string>
                        {
                            ptResult.Id.ToString(),
                            ptResult.SubjectName,
                            ptResult.CurrentLevel,
                            ptResult.SuggestLevel,
                            moduleResult.ModuleName,
                            moduleResult.LevelOfModule
                        };

                        var skillScores = moduleResult.SkillResults.ToDictionary(sr => sr.SkillName, sr => sr.Percent.ToString());

                        foreach (var skillName in allSkillNames)
                        {
                            row.Add(skillScores.TryGetValue(skillName, out var score) ? score : string.Empty);
                        }
                        csv.AppendLine(string.Join(",", row.Select(EscapeCsvField)));
                    }
                }
                else
                {
                    var row = new List<string>
                    {
                        ptResult.Id.ToString(),
                        ptResult.SubjectName,
                        ptResult.CurrentLevel,
                        ptResult.SuggestLevel,
                        string.Empty,
                        string.Empty
                    };
                    row.AddRange(Enumerable.Repeat(string.Empty, allSkillNames.Count));
                    csv.AppendLine(string.Join(",", row.Select(EscapeCsvField)));
                }
            }

            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"pt-results-{userId}.csv");
        }
    }
}
