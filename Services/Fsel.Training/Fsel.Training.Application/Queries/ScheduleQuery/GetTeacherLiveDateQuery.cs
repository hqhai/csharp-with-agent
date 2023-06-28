// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.ScheduleQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Training.Application.Queries.ClassQuery;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetTeacherLiveDateQuery : IRequest<MethodResult<IList<TeacherFreeTimeModel>>>
    {
    }

    public class GetTeacherLiveDateQueryHandler : IRequestHandler<GetTeacherLiveDateQuery, MethodResult<IList<TeacherFreeTimeModel>>>
    {
        private readonly ITeacherFreeTimeRepository _teacherFreeTimeRepository;
        private readonly IClassRepository _classRepository;

        public async Task<MethodResult<IList<TeacherFreeTimeModel>>> Handle(GetTeacherLiveDateQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<TeacherFreeTimeModel>> methodResult = new MethodResult<IList<TeacherFreeTimeModel>>();

            var classQuery = await _classRepository.Queryable
                                    .Select(x => new ClassModel
                                    {
                                        Id = x.Id,
                                        LiveTimeFrameId = x.LiveTimeFrameId,
                                        LiveDays = x.LiveDays,
                                    }).FirstOrDefaultAsync(cancellationToken);

            var teacherFreeTimeQuery = await _teacherFreeTimeRepository.Queryable
                                    .Select(x => new TeacherFreeTimeModel
                                    {
                                        Id = x.Id,
                                        LiveTimeFrameId = x.LiveTimeFrameId,
                                        DayOfWeek = x.DayOfWeek,
                                    }).ToListAsync(cancellationToken);
        }
    }
}
