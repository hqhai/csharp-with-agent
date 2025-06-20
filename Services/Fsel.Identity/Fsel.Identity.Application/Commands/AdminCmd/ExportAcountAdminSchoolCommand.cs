// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.AdminCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.Admins;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ExportAcountAdminSchoolCommand : IRequest<MethodResult<Stream>>
    {
        public string? Keyword { get; set; }

        public IList<Guid>? LocationIds { get; set; }

        public IList<string>? EventCodes { get; set; }
    }

    public class ExportAcountAdminSchoolCommandHandler : IRequestHandler<ExportAcountAdminSchoolCommand, MethodResult<Stream>>
    {
        private readonly UserManager<User> _userManager;
        private readonly ICompetitionEventsRepository _competitionEventsRepository;
        private readonly IUserSchoolRepository _userSchoolRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly RoleManager<Role> _roleManager;

        public ExportAcountAdminSchoolCommandHandler(UserManager<User> userManager,
                                                    ICompetitionEventsRepository competitionEventsRepository,
                                                    IUserSchoolRepository userSchoolRepository,
                                                    IUserRoleRepository userRoleRepository,
                                                    RoleManager<Role> roleManager)
        {
            _userManager = userManager;
            _competitionEventsRepository = competitionEventsRepository;
            _userSchoolRepository = userSchoolRepository;
            _userRoleRepository = userRoleRepository;
            _roleManager = roleManager;
        }

        public async Task<MethodResult<Stream>> Handle(ExportAcountAdminSchoolCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<Stream>();

            var userSchools = _userSchoolRepository.Queryable.AsQueryable();
            if (request.LocationIds != null && request.LocationIds.Any() && (request.EventCodes == null || !request.EventCodes.Any()))
            {
                var eventLocations = await _competitionEventsRepository.Queryable.WhereBulkContains(request.LocationIds, x => x.LocationId).ToListAsync(cancellationToken);
                request.EventCodes = eventLocations.Where(x => x.EventCode != null).Select(x => x.EventCode!).ToList();
            }

            if (request.LocationIds != null && (request.EventCodes == null || !request.EventCodes.Any()))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            if (request.EventCodes != null && request.EventCodes.Any())
            {
                var competitionEvents = await GetSchoolByEvent(request.EventCodes, cancellationToken);
                var schoolIds = competitionEvents.Result?.Where(x => x.SchoolIds != null).SelectMany(x => x.SchoolIds!).ToList();
                if (schoolIds != null && schoolIds.Any())
                {
                    userSchools = userSchools.WhereBulkContains(schoolIds, x => x.SchoolId);
                }
            }

            var users = (from a in _userManager.Users
                         join b in _userRoleRepository.Queryable on a.Id equals b.UserId
                         join c in _roleManager.Roles on b.RoleId equals c.Id
                         join m in userSchools on a.Id equals m.UserId
                         where c.Name == EnumRole.AdminSchool.ToString()
                         select new ExportAcountAdminSchoolCommandModel
                         {
                             CreatedDate = a.CreatedDate,
                             DefaultPassword = a.DefaultPassword,
                             UserName = a.UserName,
                             LocalId = m.LocalId,
                             City = m.City,
                             EventCode = m.EventCode,
                             SchoolName = m.SchoolName
                         })
                        .OrderByDescending(x => x.CreatedDate)
                        .AsQueryable();

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                users = users.Where(x => (!string.IsNullOrEmpty(x.UserName) && x.UserName.Trim().Contains(request.Keyword.Trim())));
            }

            var userQuerys = await users.ToListAsync(cancellationToken);

            return await Task.Run(() =>
            {
                var stream = userQuerys.ExportExcel();
                methodResult.Result = stream;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }, cancellationToken);

        }
        private async Task<MethodResult<IList<CompetitionEvent>>> GetSchoolByEvent(IList<string> eventCodes, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(eventCodes);
            MethodResult<IList<CompetitionEvent>> methodResult = new MethodResult<IList<CompetitionEvent>>();

            IList<CompetitionEvent> competitionEventChils = new List<CompetitionEvent>();

            eventCodes = eventCodes.Select(x => x.Trim()).ToList();
            var competitionEvents = await _competitionEventsRepository.Queryable.WhereBulkContains(eventCodes, x => x.EventCode).ToListAsync(cancellationToken);
            var competionEventIds = competitionEvents.Select(x => x.Id).ToList();

            var competitionEventChildrens = await GetTreeEvent(competionEventIds, cancellationToken);

            methodResult.Result = competitionEventChildrens.Result;
            return methodResult;
        }

        private async Task<MethodResult<IList<CompetitionEvent>>> GetTreeEvent(IList<Guid> parentIds, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(parentIds);
            MethodResult<IList<CompetitionEvent>> methodResult = new MethodResult<IList<CompetitionEvent>>();

            List<Guid> parentEventIds = new List<Guid>();

            var competitionEvents = await _competitionEventsRepository.Queryable.WhereBulkContains(parentIds, x => x.Id).ToListAsync(cancellationToken);

            var competionEventParents = await _competitionEventsRepository.Queryable.WhereBulkContains(parentIds, x => x.ParentEventId).ToListAsync(cancellationToken);
            if (competionEventParents.Any())
            {
                var competionEventParentIds = competionEventParents.Select(x => x.Id).ToList();
                parentEventIds.AddRange(competionEventParentIds);

                var competionEventNotParentIds = parentIds.Where(x => !competionEventParents.Select(c => c.ParentEventId).Contains(x)).ToList();
                parentEventIds.AddRange(competionEventNotParentIds);

                var result = await GetTreeEvent(parentEventIds, cancellationToken);
                competitionEvents = result.Result?.ToList();
            }

            methodResult.Result = competitionEvents;
            return methodResult;
        }
    }
}
