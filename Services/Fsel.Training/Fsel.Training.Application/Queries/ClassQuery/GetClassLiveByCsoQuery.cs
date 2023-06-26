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
                                        Class = new ClassModel
                                        {
                                            Id = x.Class!.Id,
                                            TeacherId = x.Class!.TeacherId,
                                            Code = x.Class!.Code,
                                            Name = x.Class!.Name,
                                            ClassStudents = x.Class!.ClassStudents.Select(x => new ClassStudentModel
                                            {
                                                StudentId = x.StudentId
                                            }).ToList()
                                        }
                                    }).FirstOrDefaultAsync(cancellationToken);
            var teacherResult = await _userService.GetTeacherByIdAsync(classLiveModel!.Class?.TeacherId ?? default);
            var teacher = teacherResult.Content?.Result;
            classLiveModel.Class!.TeacherName = teacher?.Human?.FullName;

            var studentResult = await _userService.GetStudentsByStudentIdsAsync(classLiveModel.Class.ClassStudents!.Select(x => x.StudentId).ToList());
            var students = studentResult.Content?.Result;
            if (classLiveModel.Class.ClassStudents != null)
            {
                foreach (var item in classLiveModel.Class!.ClassStudents)
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
