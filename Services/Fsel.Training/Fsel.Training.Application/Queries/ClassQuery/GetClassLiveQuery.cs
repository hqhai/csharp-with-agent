// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.ClassQuery
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
    using Microsoft.EntityFrameworkCore;

    public class GetClassLiveQuery : IRequest<MethodResult<ClassModel>>
    {
        public Guid? Id { get; set; }
    }

    public class GetClassLiveQueryHandler : IRequestHandler<GetClassLiveQuery, MethodResult<ClassModel>>
    {
        private readonly IClassRepository _classRepository;

        public async Task<MethodResult<ClassModel>> Handle(GetClassLiveQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            MethodResult<ClassModel> methodResult = new MethodResult<ClassModel>();

            var classLive = await _classRepository.Queryable
                                    .Include(x => x.ClassStudents)
                                    .Include(x => x.ClassLiveCalendars)
                                    .Select(x => new ClassModel
                                    {
                                        Id = x.Id,
                                        TeacherId = x.TeacherId,
                                        Code = x.Code,
                                        ClassStudents = x.ClassStudents.Select(x => new ClassStudentModel
                                        {
                                            StudentId = x.StudentId,
                                        }).ToList(),
                                        ClassLiveCalendars = x.ClassLiveCalendars.Select(x => new ClassLiveCalendarModel
                                        {
                                            AccessLink = x.AccessLink,
                                            Note = x.Note,
                                        }).ToList(),
                                    }).FirstOrDefaultAsync(cancellationToken);
        }
    }
}
