// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.ClassLiveWorkFlowQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Training.Application.Services.UserServices;
    using Fsel.Training.Application.Services.UserServices.Models;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetClassLiveWorkFlowInfoQuery : IRequest<MethodResult<ClassLiveWorkFlowInfoModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetClassLiveWorkFlowInfoQueryHandler : IRequestHandler<GetClassLiveWorkFlowInfoQuery, MethodResult<ClassLiveWorkFlowInfoModel>>
    {
        private readonly IClassLiveWorkFlowRepository _classLiveWorkFlowRepository;
        private readonly IUserService _userService;

        public GetClassLiveWorkFlowInfoQueryHandler(IClassLiveWorkFlowRepository classLiveWorkFlowRepository, IUserService userService)
        {
            _classLiveWorkFlowRepository = classLiveWorkFlowRepository;
            _userService = userService;
        }

        public async Task<MethodResult<ClassLiveWorkFlowInfoModel>> Handle(GetClassLiveWorkFlowInfoQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var methodResult = new MethodResult<ClassLiveWorkFlowInfoModel>();
            var liveSessionInformation = new ClassLiveWorkFlowInfoModel();
            var classLiveWorkFlow = await _classLiveWorkFlowRepository.GetIncludeByIdAsync(request.Id);
            if (classLiveWorkFlow == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(classLiveWorkFlow));
                return methodResult;
            }
            liveSessionInformation.Type = classLiveWorkFlow.Type;
            var studentIds = classLiveWorkFlow.ClassLiveCalendar?.Class?.ClassStudents.Select(p => p.StudentId).Distinct().ToList();
            if (studentIds != null && studentIds.Count > 0)
            {
                var studentsResult = await _userService.GetStudentsByStudentIdsAsync(studentIds);
                if (!studentsResult.IsSuccessStatusCode)
                {
                    methodResult.AddError(studentsResult.Error);
                    return methodResult;
                }
                var students = studentsResult.Content?.Result?.ToList();
                if (students != null && students.Count > 0)
                {
                    liveSessionInformation.Students = students.Select(x => new StudentInfoModel
                    {
                        FullName = x.User?.FullName,
                        PhoneNumber = x.User?.PhoneNumber,
                        Email = x.User?.Email,
                    }).ToList();
                }
            }
            liveSessionInformation.ClassName = classLiveWorkFlow.ClassLiveCalendar?.Class?.Name;
            liveSessionInformation.Description = classLiveWorkFlow.Description;
            TeacherModel? teacher = new TeacherModel();
            if (classLiveWorkFlow.TeacherId.HasValue)
            {
                var teacherResult = await _userService.GetTeacherByIdAsync(classLiveWorkFlow.TeacherId ?? default);
                if (!teacherResult.IsSuccessStatusCode)
                {
                    methodResult.AddError(teacherResult.Error);
                    return methodResult;
                }
                teacher = teacherResult.Content?.Result;
            }
            liveSessionInformation.TeacherName = teacher?.User?.FullName;
            methodResult.Result = liveSessionInformation;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
