// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.StudentCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Application.Queues.Publishers;
    using Fsel.Identity.Application.Services.SystemService;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class CreateStudentsToEventFromFileCommandModel : BaseImportCommandModel
    {
        public EnumCompetitionEventCategory Category { get; set; }
        public string? Key { get; set; }
    }

    public class CreateStudentsFromFileCommand : CreateStudentsToEventFromFileCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class CreateStudentsFromFileCommandHandler : IRequestHandler<CreateStudentsFromFileCommand, MethodResult<bool>>
    {
        private readonly CreateStudentsFromFilePublisher _createStudentsFromFilePublisher;
        private readonly UserManager<User> _userManager;
        private readonly AuthContext _authContext;
        private readonly ISystemService _systemService;
        private readonly ICompetitionEventsRepository _competitionEventsRepository;
        private readonly CreateStudentsAndParentsFromFilePublisher _createStudentsAndParentsFromFilePublisher;
        private const string EventCode = "EVHoChiMinh";

        public CreateStudentsFromFileCommandHandler(CreateStudentsFromFilePublisher createStudentsFromFilePublisher, UserManager<User> userManager, AuthContext authContext, ISystemService systemService, ICompetitionEventsRepository competitionEventsRepository, CreateStudentsAndParentsFromFilePublisher createStudentsAndParentsFromFilePublisher)
        {
            _createStudentsFromFilePublisher = createStudentsFromFilePublisher;
            _userManager = userManager;
            _authContext = authContext;
            _systemService = systemService;
            _competitionEventsRepository = competitionEventsRepository;
            _createStudentsAndParentsFromFilePublisher = createStudentsAndParentsFromFilePublisher;
        }

        public async Task<MethodResult<bool>> Handle(CreateStudentsFromFileCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            bool flag = false;

            var currentUser = await _userManager.Users.Include(p => p.UserSchools).FirstOrDefaultAsync(p => p.Id == _authContext.CurrentUserId, cancellationToken);
            if (currentUser != null)
            {
                var schoolId = currentUser.UserSchools.FirstOrDefault()?.SchoolId;
                if (schoolId.HasValue)
                {
                    var competitionEvents = await _competitionEventsRepository.Queryable.ToListAsync(cancellationToken);
                    var competitionEvent = competitionEvents.Where(p => p.SchoolIds != null && p.SchoolIds.Contains(schoolId.Value) && p.Category == request.Category).OrderByDescending(p => p.CreatedDate).FirstOrDefault();
                    if (competitionEvent != null)
                    {
                        if (!string.IsNullOrEmpty(competitionEvent.EventCode) && competitionEvent.EventCode.Contains(EventCode, StringComparison.CurrentCultureIgnoreCase))
                        {
                            flag = true;
                        }
                    }
                }
            }

            if (flag && request.Category == EnumCompetitionEventCategory.Student)
            {
                await _createStudentsAndParentsFromFilePublisher.Publish(new CreateStudentsToEventFromByteModel
                {
                    Category = request.Category,
                    Key = request.Key,
                    File = ConvertHelper.FileToByteArray(request.FormFile),
                }, cancellationToken);
            }
            else
            {
                await _createStudentsFromFilePublisher.Publish(new CreateStudentsToEventFromByteModel
                {
                    Category = request.Category,
                    Key = request.Key,
                    File = ConvertHelper.FileToByteArray(request.FormFile),
                }, cancellationToken);
            }

            methodResult.Result = true;
            return methodResult;
        }
    }
}
