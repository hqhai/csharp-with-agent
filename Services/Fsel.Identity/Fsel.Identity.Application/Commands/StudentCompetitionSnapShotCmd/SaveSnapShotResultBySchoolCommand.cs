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
    using Fsel.Identity.Domain.IRepositories;

    public class SaveSnapShotResultBySchoolCommand : BaseQueryModel, IRequest<MethodResult<bool>>
    {
    }

    public class SaveSnapShotResultBySchoolCommandHandler : IRequestHandler<SaveSnapShotResultBySchoolCommand, MethodResult<bool>>
    {
        private readonly IMediator _mediator;
        private readonly ICompetitionEventsRepository _competitionEventsRepository;
        public SaveSnapShotResultBySchoolCommandHandler(IMediator mediator, ICompetitionEventsRepository competitionEventsRepository)
        {
            _mediator = mediator;
            _competitionEventsRepository = competitionEventsRepository;
        }

        public async Task<MethodResult<bool>> Handle(SaveSnapShotResultBySchoolCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();
            string competitionConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ResourceSettings.SchoolEventRules);
            string jsonData = File.ReadAllText(competitionConfigPath);
            var listSchoolEventRules = _competitionEventsRepository.Queryable.ToList();

            foreach (var item in listSchoolEventRules!)
            {
                await _mediator.Send(new GetStudentCompetitionSchoolQuery
                {
                    CourseType = EnumCourseType.Academic,
                    SchoolCode = item.EventCode,
                    WeekNumber = item.EventContent?.WeekEvents?.FirstOrDefault(x => DateTime.UtcNow.Date == x.EndDate.Date)?.WeekNumber ?? 1 // mặc định tuần 1
                }, cancellationToken);
            }

            return methodResult;
        }
    }
}
