// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.ClassQuery
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Training.Application.Services.UserServices;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetClassLiveByCsoQuery : IRequest<MethodResult<ClassLiveCalendarModel>>
    {
        public Guid? Id { get; set; }
    }

    public class GetClassLiveByCsoQueryHandler : IRequestHandler<GetClassLiveByCsoQuery, MethodResult<ClassLiveCalendarModel>>
    {
        private readonly IClassLiveCalendarRepository _classLiveCalenderRepository;
        private readonly IUserService _userService;

        public GetClassLiveByCsoQueryHandler(IClassLiveCalendarRepository classLiveCalenderRepository, IUserService userService)
        {
            _classLiveCalenderRepository = classLiveCalenderRepository;
            _userService = userService;
        }

        public async Task<MethodResult<ClassLiveCalendarModel>> Handle(GetClassLiveByCsoQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            MethodResult<ClassLiveCalendarModel> methodResult = new MethodResult<ClassLiveCalendarModel>();

            var classLiveModel = await _classLiveCalenderRepository.Queryable
                                    .Include(x => x.Class)
                                    .ThenInclude(x => x.ClassStudents)
                                    .Where(x => x.Id == request.Id)
                                    .Select(x => new ClassLiveCalendarModel
                                    {
                                        Id = x.Id,
                                        AccessLink = x.AccessLink,
                                        Note = x.Note,
                                        TeacherId = x.Class!.TeacherId,
                                        ClassName = x.Class.Name,
                                        ClassCode = x.Class.Code,
                                        ClassStudents = x.Class.ClassStudents.Select(x => new ClassStudentModel
                                        {
                                            StudentId = x.StudentId
                                        }).ToList()
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
