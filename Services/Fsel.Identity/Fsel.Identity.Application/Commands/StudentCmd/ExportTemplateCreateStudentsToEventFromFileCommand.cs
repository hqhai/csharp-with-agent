// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.StudentCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Application.Queues.Publishers;
    using Fsel.Identity.Application.Services.SystemService;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.Students;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ExportTemplateCreateStudentsToEventFromFileCommand : IRequest<MethodResult<Stream>>
    {
    }

    public class ExportTemplateCreateStudentsToEventFromFileCommandHandler : IRequestHandler<ExportTemplateCreateStudentsToEventFromFileCommand, MethodResult<Stream>>
    {
        private readonly CreateStudentsFromFilePublisher _createStudentsFromFilePublisher;
        private readonly UserManager<User> _userManager;
        private readonly AuthContext _authContext;
        private readonly ISystemService _systemService;
        private readonly ICompetitionEventsRepository _competitionEventsRepository;
        private readonly CreateStudentsAndParentsFromFilePublisher _createStudentsAndParentsFromFilePublisher;
        private const string EventCode = "EVHoChiMinh";

        public ExportTemplateCreateStudentsToEventFromFileCommandHandler(CreateStudentsFromFilePublisher createStudentsFromFilePublisher, UserManager<User> userManager, AuthContext authContext, ISystemService systemService, ICompetitionEventsRepository competitionEventsRepository, CreateStudentsAndParentsFromFilePublisher createStudentsAndParentsFromFilePublisher)
        {
            _createStudentsFromFilePublisher = createStudentsFromFilePublisher;
            _userManager = userManager;
            _authContext = authContext;
            _systemService = systemService;
            _competitionEventsRepository = competitionEventsRepository;
            _createStudentsAndParentsFromFilePublisher = createStudentsAndParentsFromFilePublisher;
        }

        public async Task<MethodResult<Stream>> Handle(ExportTemplateCreateStudentsToEventFromFileCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<Stream>();

            bool flag = false;

            var currentUser = await _userManager.Users.Include(p => p.UserSchools).FirstOrDefaultAsync(p => p.Id == _authContext.CurrentUserId, cancellationToken);
            if (currentUser != null)
            {
                var schoolId = currentUser.UserSchools.FirstOrDefault()?.SchoolId;
                if (schoolId.HasValue)
                {
                    var competitionEvents = await _competitionEventsRepository.Queryable.ToListAsync(cancellationToken);
                    var competitionEvent = competitionEvents.Where(p => p.SchoolIds != null && p.SchoolIds.Contains(schoolId.Value) && p.Category == EnumCompetitionEventCategory.Student).OrderByDescending(p => p.CreatedDate).FirstOrDefault();
                    if (competitionEvent != null)
                    {
                        if (!string.IsNullOrEmpty(competitionEvent.EventCode) && competitionEvent.EventCode.Contains(EventCode, StringComparison.CurrentCultureIgnoreCase))
                        {
                            flag = true;
                        }
                    }
                }
            }

            if (flag)
            {
                return await Task.Run(() =>
                {
                    var template = new List<string>();
                    var stream = template.ExportExcelTemplate<CreateStudentAndParentToEventFromFileModel>();
                    methodResult.Result = stream;
                    methodResult.StatusCode = StatusCodes.Status200OK;
                    return methodResult;
                }, cancellationToken);
            }
            else
            {
                return await Task.Run(() =>
                {
                    var template = new List<string>();
                    var stream = template.ExportExcelTemplate<CreateStudentToEventFromFileModel>();
                    methodResult.Result = stream;
                    methodResult.StatusCode = StatusCodes.Status200OK;
                    return methodResult;
                }, cancellationToken);
            }
        }
    }
}
