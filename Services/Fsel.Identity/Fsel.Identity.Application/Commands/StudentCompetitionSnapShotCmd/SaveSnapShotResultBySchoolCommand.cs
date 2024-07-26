// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.StudentCompetitionSnapShotCmd

{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Constants;
    using MediatR;
    using Fsel.Shared.Models.ShareModels;
    using Newtonsoft.Json;
    using Fsel.Identity.Application.Queries.StudentRanking;
    using Fsel.Shared.Enums;

    public class SaveSnapShotResultBySchoolCommand : BaseQueryModel, IRequest<MethodResult<bool>>
    {
    }

    public class SaveSnapShotResultBySchoolCommandHandler : IRequestHandler<SaveSnapShotResultBySchoolCommand, MethodResult<bool>>
    {
        private readonly IMediator _mediator;
        public SaveSnapShotResultBySchoolCommandHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<MethodResult<bool>> Handle(SaveSnapShotResultBySchoolCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();
            string competitionConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ResourceSettings.SchoolEventRules);
            string jsonData = File.ReadAllText(competitionConfigPath);
            var listSchoolEventRules = JsonConvert.DeserializeObject<IList<SchoolEventRule>>(jsonData);

            foreach (var item in listSchoolEventRules!)
            {
                await _mediator.Send(new GetStudentCompetitionSchoolQuery
                {
                    CourseType = EnumCourseType.Academic,
                    SchoolCode = item.SchoolCode,
                    WeekNumber = item.WeekEvents!.FirstOrDefault(x => DateTime.UtcNow.Date == x.EndDate.Date)?.WeekNumber ?? 1 // mặc định tuần 1
                }, cancellationToken);
            }

            return methodResult;
        }
    }
}
