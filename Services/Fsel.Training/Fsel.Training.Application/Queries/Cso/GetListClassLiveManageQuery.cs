// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.Cso
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Application.Services.UserServices.Models;
    using Fsel.Training.Application.Services.SystemServices;
    using Fsel.Training.Application.Services.UserServices;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.EntityModels;
    using Fsel.Training.Domain.Models.QueryModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetListClassLiveManageQuery : GetListClassLiveManageQueryModel, IRequest<MethodResult<IList<ClassModel>>>
    {
    }

    public class GetListClassLiveManageQueryHandler : IRequestHandler<GetListClassLiveManageQuery, MethodResult<IList<ClassModel>>>
    {
        private readonly IClassRepository _classRepository;
        private readonly ISystemService _systemService;
        private readonly IUserService _userService;

        public GetListClassLiveManageQueryHandler(IClassRepository classRepository, ISystemService systemService, IUserService userService)
        {
            _classRepository = classRepository;
            _systemService = systemService;
            _userService = userService;
        }

        public async Task<MethodResult<IList<ClassModel>>> Handle(GetListClassLiveManageQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            MethodResult<IList<ClassModel>> methodResult = new MethodResult<IList<ClassModel>>();

            var classQuery = await _classRepository.Queryable
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
                                    StartTime = x.StartTime,
                                    LiveDays = x.LiveDays,
                                    EndTime = x.EndTime,
                                    PackageId = x.PackageId,
                                }).ToListAsync(cancellationToken);

            /*if (request.ClassCode != null)
            {
                classQuery = classQuery.Where(m => m.Code == request.ClassCode);
            }*/
            var teacherResult = await _userService.GetTeacherByIdsAsync(new GetTeacherByIdsQueryModel { Ids = classQuery.Select(x => x.TeacherId ?? default).Distinct().ToList() });
            var teachers = teacherResult.Content?.Result;

            foreach (var item in classQuery)
            {
                var teacher = teachers!.FirstOrDefault(x => x.Id == item.TeacherId);
                item.TeacherName = teacher?.Human?.FullName;
            }
            var timeFrameResult = await _systemService.GetTimeFramByIdsAsync(classQuery.Select(x => x.LiveTimeFrameId ?? default).ToList());
            var timeFrames = timeFrameResult.Content?.Result;

            foreach (var item in classQuery)
            {
                item.TimeFrameEndTime = timeFrames!.EndTime;
                item.TimeFrameStartTime = timeFrames!.StartTime;
            }
            methodResult.Result = classQuery;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
