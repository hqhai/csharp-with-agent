// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.ChangeLiveSessionQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Training.Application.Services.SystemServices;
    using Fsel.Training.Application.Services.UserServices;
    using Fsel.Training.Application.Services.UserServices.Models;
    using Fsel.Training.Domain.Enums.ErrorCodes;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetChangeLiveSessionQuery : IRequest<MethodResult<ChangeLiveSessionInfoModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetChangeLiveSessionQueryHandler : IRequestHandler<GetChangeLiveSessionQuery, MethodResult<ChangeLiveSessionInfoModel>>
    {
        private readonly IClassLiveWorkFlowRepository _classLiveWorkFlowRepository;
        private readonly IMapper _mapper;
        private readonly ISystemService _systemService;
        private readonly IUserService _userService;

        public GetChangeLiveSessionQueryHandler(IClassLiveWorkFlowRepository classLiveWorkFlowRepository
            , IMapper mapper
            , ISystemService systemService
            , IUserService userService)
        {
            _classLiveWorkFlowRepository = classLiveWorkFlowRepository;
            _mapper = mapper;
            _systemService = systemService;
            _userService = userService;
        }

        public async Task<MethodResult<ChangeLiveSessionInfoModel>> Handle(GetChangeLiveSessionQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var methodResult = new MethodResult<ChangeLiveSessionInfoModel>();
            var liveSessionInformation = new ChangeLiveSessionInfoModel();
            var classLiveWorkFlow = await _classLiveWorkFlowRepository.GetIncludeByIdAsync(request.Id);
            if (classLiveWorkFlow == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassLiveWorkFlowErrorCode.ClassLiveWorkFlowNotExits));
                return methodResult;
            }
            var @class = classLiveWorkFlow.ClassLiveCalendar?.Class;
            var studentIds = @class?.ClassStudents.Select(p => p.StudentId).ToList();
            IList<StudentModel>? students = new List<StudentModel>();
            if (studentIds?.Count > 0)
            {
                var studentsResult = await _userService.GetStudentsByStudentIdsAsync(studentIds!);
                if (!studentsResult.IsSuccessStatusCode)
                {
                    methodResult.AddError(studentsResult.Error);
                    return methodResult;
                }
                students = studentsResult.Content?.Result;
                if (students?.Count > 0)
                {
                    liveSessionInformation.Students = students?.Select(x => new StudentInfoModel
                    {
                        FullName = x.Human?.FullName,
                        PhoneNumber = x.Human?.PhoneNumber,
                        Email = x.Human?.Email,
                    }).ToList();
                }
            }

            liveSessionInformation.ClassName = @class?.Name;
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
                liveSessionInformation.TeacherName = teacher?.Human?.FullName;
            }
            var classWorkFlowPlans = classLiveWorkFlow.ClassLiveWorkFlowPlans.Where(x => x.IsActive).ToList();
            var classWorkFlowPlansModel = _mapper.Map<IList<ClassLiveWorkFlowPlanModel>>(classWorkFlowPlans);
            var timeFramesResult = await _systemService.GetLiveTimeFramesAsync();
            var timeFrames = timeFramesResult.Content?.Result;
            foreach (var item in classWorkFlowPlansModel)
            {
                var isCheck = classWorkFlowPlans.Sum(x => x.VoteNumber) != 0 && item.VoteNumber != 0;
                item.Percent = isCheck ? item.VoteNumber / classWorkFlowPlans.Sum(x => x.VoteNumber) : 0;
                var liveTimeFrame = timeFrames?.FirstOrDefault(x => x.Id == item.LiveTimeFrameId);
                item.StartTime = liveTimeFrame?.StartTime;
                item.EndTime = liveTimeFrame?.EndTime;
            }
            liveSessionInformation.ClassLiveWorkFlowPlans = classWorkFlowPlansModel;
            methodResult.Result = liveSessionInformation;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
