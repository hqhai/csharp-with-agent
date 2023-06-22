// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.Schedule
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetListPriorityCalenderQuery : IRequest<MethodResult<IList<TeacherFreeTimeModel>>>
    {
    }

    public class GetListPriorityCalenderQueryHandler : IRequestHandler<GetListPriorityCalenderQuery, MethodResult<IList<TeacherFreeTimeModel>>>
    {
        private readonly ITeacherFreeTimeRepository _teacherFreeTimeRepository;

        public GetListPriorityCalenderQueryHandler(ITeacherFreeTimeRepository teacherFreeTimeRepository)
        {
            _teacherFreeTimeRepository = teacherFreeTimeRepository;
        }

        public async Task<MethodResult<IList<TeacherFreeTimeModel>>> Handle(GetListPriorityCalenderQuery request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<IList<TeacherFreeTimeModel>>();

            var priorityCalenderQuery = await _teacherFreeTimeRepository.Queryable
                                    .Include(x => x.TeacherFreeDate)
                                    .Select(x => new TeacherFreeTimeModel
                                    {
                                        Id = x.Id,
                                        TeacherFreeDateId = x.TeacherFreeDateId,
                                        CreatedDate = x.CreatedDate,
                                        DayOfWeek = x.DayOfWeek,
                                        Priority = x.Priority,
                                        TeacherId = x.TeacherFreeDate!.TeacherId,
                                        StartTime = x.TeacherFreeDate.StartTime,
                                        EndTime = x.TeacherFreeDate.EndTime,
                                    }).ToListAsync(cancellationToken);

            methodResult.Result = priorityCalenderQuery;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
