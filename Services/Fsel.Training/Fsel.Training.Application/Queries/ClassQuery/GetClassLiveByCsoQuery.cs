// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.ClassQuery
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Application.Services.UserServices.Models;
    using Fsel.Training.Application.Services.UserServices;
    using Fsel.Training.Domain.Entities;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetClassLiveByCsoQuery : IRequest<MethodResult<ClassModel>>
    {
        public Guid? Id { get; set; }
    }

    public class GetClassLiveByCsoQueryHandler : IRequestHandler<GetClassLiveByCsoQuery, MethodResult<ClassModel>>
    {
        private readonly IClassRepository _classRepository;
        private readonly IUserService _userService;

        public GetClassLiveByCsoQueryHandler(IClassRepository classRepository, IUserService userService)
        {
            _classRepository = classRepository;
            _userService = userService;
        }

        public async Task<MethodResult<ClassModel>> Handle(GetClassLiveByCsoQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            MethodResult<ClassModel> methodResult = new MethodResult<ClassModel>();

            var classLiveModel = await _classRepository.Queryable
                                    .Include(x => x.ClassStudents)
                                    .Include(x => x.ClassLiveCalendars)
                                    .Where(x => x.Id == request.Id)
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
            var teacherResult = await _userService.GetTeacherByIdAsync(classLiveModel!.TeacherId ?? default);
            var teacher = teacherResult.Content?.Result;
            classLiveModel.TeacherName = teacher?.Human?.FullName;

            var studentResult = await _userService.GetStudentByUserIdsAsync(classLiveModel.ClassStudents!.Select(x => x.StudentId).ToList());
            var students = studentResult.Content?.Result;
            if (classLiveModel.ClassStudents != null)
            {
                foreach (var item in classLiveModel.ClassStudents)
                {
                    var student = students?.FirstOrDefault(x => x.Id == item.StudentId);
                    item.StudentName = student?.Human?.FullName;
                    item.PhoneNumber = student?.Human?.PhoneNumber;
                    item.Email = student?.Human?.Email;
                }
            }

            methodResult.Result = classLiveModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
