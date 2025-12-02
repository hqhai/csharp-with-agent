// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.ClassQuery
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Training.Application.Services.SystemServices;
    using Fsel.Training.Application.Services.UserServices;
    using Fsel.Training.Application.Services.UserServices.Models;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetListClassByCsoQuery : IRequest<MethodResult<IList<ClassModel>>>
    {
    }

    public class GetListClassByCsoQueryHandler : IRequestHandler<GetListClassByCsoQuery, MethodResult<IList<ClassModel>>>
    {
        private readonly IClassRepository _classRepository;
        private readonly ISystemService _systemService;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;

        public GetListClassByCsoQueryHandler(IClassRepository classRepository, ISystemService systemService, IUserService userService, AuthContext authContext)
        {
            _classRepository = classRepository;
            _systemService = systemService;
            _userService = userService;
            _authContext = authContext;
        }

        public async Task<MethodResult<IList<ClassModel>>> Handle(GetListClassByCsoQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var methodResult = new MethodResult<IList<ClassModel>>();
            var cso = await _userService.GetCsoByUserIdAsync(_authContext.CurrentUserId);
            if (!cso.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(cso));
                return methodResult;
            }
            var csoId = cso?.Content?.Result?.Id;
            var classQuery = await _classRepository.Queryable
                                .Where(x => x.CsoId == csoId && x.LiveTimeFrameId != null)
                                .Select(x => new ClassModel

                                {
                                    Id = x.Id,
                                    Name = x.Name,
                                    Code = x.Code,
                                    TeacherId = x.TeacherId,
                                    CreatedDate = x.CreatedDate,
                                    CourseId = x.CourseId,
                                    CsoId = x.CsoId,
                                    Status = x.Status,
                                    LiveTimeFrameId = x.LiveTimeFrameId,
                                    StartDate = x.StartDate!.Value,
                                    LiveDays = x.LiveDays,
                                    EndDate = x.EndDate!.Value,
                                    PackageId = x.PackageId,
                                }).ToListAsync(cancellationToken);

            var teacherResult = await _userService.GetTeacherByIdsAsync(new GetTeacherByIdsQueryModel { Ids = classQuery.Select(x => x.TeacherId ?? default).Distinct().ToList() });
            var teachers = teacherResult.Content?.Result;

            foreach (var item in classQuery)
            {
                var teacher = teachers!.FirstOrDefault(x => x.Id == item.TeacherId);
                item.TeacherName = teacher?.User?.FullName;
            }
            var timeFramesResult = await _systemService.GetLiveTimeFramesAsync();
            var timeFrames = timeFramesResult.Content?.Result;

            foreach (var item in classQuery)
            {
                var timeFrame = timeFrames?.FirstOrDefault(x => x.Id == item.LiveTimeFrameId);
                item.TimeFrameStartTime = timeFrame?.StartTime;
                item.TimeFrameEndTime = timeFrame?.EndTime;
            }
            methodResult.Result = classQuery;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
