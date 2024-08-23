// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.StudentFocusTimeCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Identity.Application.Queues.Publishers;
    using Fsel.Identity.Application.Services.SystemService;
    using Fsel.Identity.Application.Services.TrainingService;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.StudentFocusTime;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateStudentFocusTimeCommand : CreateStudentFocusTimeCommandModel, IRequest<MethodResult<StudentFocusTimeModel>>
    {
    }

    public class CreateStudentFocusTimeCommandHandler : IRequestHandler<CreateStudentFocusTimeCommand, MethodResult<StudentFocusTimeModel>>
    {
        private readonly IMapper _mapper;
        private readonly IStudentFocusTimeRepository _studentFocusTimeRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly AuthContext _authContext;
        private readonly ISystemService _systemService;
        private readonly QuestBoardPublisher _questBoardPublisher;
        private readonly ITrainingService _trainingService;

        public CreateStudentFocusTimeCommandHandler(IMapper mapper, IStudentFocusTimeRepository studentFocusTimeRepository, IStudentRepository studentRepository, AuthContext authContext, ISystemService systemService, QuestBoardPublisher questBoardPublisher,
            ITrainingService trainingService)
        {
            _mapper = mapper;
            _studentFocusTimeRepository = studentFocusTimeRepository;
            _studentRepository = studentRepository;
            _authContext = authContext;
            _systemService = systemService;
            _questBoardPublisher = questBoardPublisher;
            _trainingService = trainingService;
        }

        public async Task<MethodResult<StudentFocusTimeModel>> Handle(CreateStudentFocusTimeCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<StudentFocusTimeModel> methodResult = new MethodResult<StudentFocusTimeModel>();

            if (_authContext.CurrentUserId == Guid.Empty)
            {
                _authContext.CurrentUserId = request.UserId;
            }
            var student = _studentRepository.Queryable.Include(x => x.Human).FirstOrDefault(x => x.Human!.UserId == _authContext.CurrentUserId);
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(_authContext.CurrentUserId), _authContext.CurrentUserId);
                return methodResult;
            }
            var studentFocusTime = _studentFocusTimeRepository.Queryable.FirstOrDefault(x => x.StudentId == student.Id && x.CreatedDate.Date == DateTime.UtcNow.Date);

            var studentFocusTimeNeareast = _studentFocusTimeRepository.Queryable.FirstOrDefault(x => x.StudentId == student.Id && x.CreatedDate.Date == DateTime.UtcNow.Date.AddDays(-1));

            var systemConfig = await _systemService.GetFocusTimeConfig();
            var systemConfigResult = systemConfig?.Content?.Result;
            if (systemConfigResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            //Thực hiện các hành động lưu xuống database , gửi lên websocket
            await _studentFocusTimeRepository.ExecuteTransactionAsync(async () =>
            {
                if (studentFocusTime == null)
                {
                    studentFocusTime = _mapper.Map<StudentFocusTime>(request);
                    studentFocusTime.TargetTime = studentFocusTimeNeareast != null ? studentFocusTimeNeareast.TargetTime : request.TargetTime;
                    studentFocusTime.StudentId = student.Id;
                    studentFocusTime.IsEstablished = false;

                    _studentFocusTimeRepository.Add(studentFocusTime);
                }
                else
                {
                    // Set targetTime
                    bool confitionChangeTarget = !studentFocusTime!.IsEstablished && request.TargetTime != studentFocusTime.TargetTime && request.TargetTime != 0;
                    if (confitionChangeTarget)
                    {
                        studentFocusTime.TargetTime = request.TargetTime;
                        studentFocusTime.IsEstablished = confitionChangeTarget;
                    }

                    // Set AccessTime And NumberOfToken
                    var systemConfigMap = systemConfigResult!.FirstOrDefault(x => x.TargetTime == studentFocusTime.TargetTime);
                    studentFocusTime.ExecuteTime += request.ExecuteTime;

                    _studentFocusTimeRepository.Update(studentFocusTime);
                }

                await _studentFocusTimeRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<StudentFocusTimeModel>(studentFocusTime);
                return methodResult;
            });

            return methodResult;
        }
    }
}
