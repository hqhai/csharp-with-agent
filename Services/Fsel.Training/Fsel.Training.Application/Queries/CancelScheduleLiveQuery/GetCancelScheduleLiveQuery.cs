// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.CancelScheduleLiveQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Shared.Enums;
    using Fsel.Training.Application.Services.SystemServices;
    using Fsel.Training.Application.Services.UserServices;
    using Fsel.Training.Application.Services.UserServices.Models;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetCancelScheduleLiveQuery : IRequest<MethodResult<CancelScheduleLiveInfoModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetCancelScheduleLiveQueryHandler : IRequestHandler<GetCancelScheduleLiveQuery, MethodResult<CancelScheduleLiveInfoModel>>
    {
        private readonly IClassLiveWorkFlowRepository _classLiveWorkFlowRepository;
        private readonly IMapper _mapper;
        private readonly ISystemService _systemService;
        private readonly IUserService _userService;

        public GetCancelScheduleLiveQueryHandler(IClassLiveWorkFlowRepository classLiveWorkFlowRepository
            , IMapper mapper
            , ISystemService systemService
            , IUserService userService)
        {
            _classLiveWorkFlowRepository = classLiveWorkFlowRepository;
            _mapper = mapper;
            _systemService = systemService;
            _userService = userService;
        }

        public async Task<MethodResult<CancelScheduleLiveInfoModel>> Handle(GetCancelScheduleLiveQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var methodResult = new MethodResult<CancelScheduleLiveInfoModel>();
            var liveSessionInformation = new CancelScheduleLiveInfoModel();
            var classLiveWorkFlow = await _classLiveWorkFlowRepository.GetIncludeByIdAsync(request.Id);
            if (classLiveWorkFlow == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(classLiveWorkFlow));
                return methodResult;
            }
            _mapper.Map(classLiveWorkFlow, liveSessionInformation);
            liveSessionInformation.AccessLink = classLiveWorkFlow.ClassLiveCalendar?.AccessLink;
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
                        FullName = x?.User?.FullName,
                        PhoneNumber = x?.User?.PhoneNumber,
                        Email = x?.User?.Email,
                    }).ToList();
                }
            }
            if (classLiveWorkFlow.Status == EnumWorkFlowCancelScheduleStatus.WaitVote.ToString() && classLiveWorkFlow.UpdatedDate != null && classLiveWorkFlow.UpdatedDate.Value.AddDays(2) <= DateTime.UtcNow)
            {
                liveSessionInformation.IsWaitVote = true;
            }
            liveSessionInformation.ClassName = @class?.Name;
            liveSessionInformation.Description = classLiveWorkFlow.Description;
            var teacher = new TeacherModel();
            if (classLiveWorkFlow.TeacherId.HasValue)
            {
                var teacherResult = await _userService.GetTeacherByIdAsync(classLiveWorkFlow.TeacherId ?? default);
                if (!teacherResult.IsSuccessStatusCode)
                {
                    methodResult.AddError(teacherResult.Error);
                    return methodResult;
                }
                teacher = teacherResult.Content?.Result;
                liveSessionInformation.TeacherName = teacher?.User?.FullName;
            }
            var classWorkFlowPlans = classLiveWorkFlow.ClassLiveWorkFlowPlans.Where(x => x.IsActive).ToList();
            var classWorkFlowPlansModel = _mapper.Map<IList<ClassLiveWorkFlowPlanModel>>(classWorkFlowPlans);
            var timeFramesResult = await _systemService.GetLiveTimeFramesAsync();
            var timeFrames = timeFramesResult.Content?.Result;
            foreach (var item in classWorkFlowPlansModel)
            {
                var isCheck = classWorkFlowPlans.Sum(x => x.VoteNumber) != 0 && item.VoteNumber != 0;
                item.Percent = isCheck ? (double)item.VoteNumber / classWorkFlowPlans.Sum(x => x.VoteNumber) * 100 : 0;
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
