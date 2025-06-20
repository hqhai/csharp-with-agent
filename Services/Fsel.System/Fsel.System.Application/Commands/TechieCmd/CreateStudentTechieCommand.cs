// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.TechieCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Application.Queues.Publisher;
    using Fsel.System.Application.Services.UserServices;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.Techie;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System.Globalization;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    public class CreateStudentTechieCommand : SaveStudentTechieCommandModel, IRequest<MethodResult<StudentTechieModel>>
    {
    }

    public class CreateStudentTechieCommandHandler : IRequestHandler<CreateStudentTechieCommand, MethodResult<StudentTechieModel>>
    {
        private readonly IMapper _mapper;
        private readonly ITechieActionRepository _techieActionRepository;
        private readonly IStudentTechieRepository _studentTechieRepository;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;
        private readonly TechieSendMessagePublisher _techieSendMessagePublisher;
        private readonly ILogger<object> _logger;

        public CreateStudentTechieCommandHandler(IMapper mapper, ITechieActionRepository techieActionRepository, IStudentTechieRepository studentTechieRepository, IUserService userService, AuthContext authContext, TechieSendMessagePublisher techieSendMessagePublisher, ILogger<object> logger)
        {
            _mapper = mapper;
            _techieActionRepository = techieActionRepository;
            _studentTechieRepository = studentTechieRepository;
            _userService = userService;
            _authContext = authContext;
            _techieSendMessagePublisher = techieSendMessagePublisher;
            _logger = logger;
        }

        public async Task<MethodResult<StudentTechieModel>> Handle(CreateStudentTechieCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<StudentTechieModel>();

            var techieActions = _techieActionRepository.Queryable.Where(x => x.Action == request.Actions && x.Feature == request.TechieFeature).ToList();
            if (request.Config == null || request.Config.StartTime < 0 || request.Config.EndTime < 0)
            {
                return methodResult;
            }

            var techieActionFilter = techieActions.FirstOrDefault(x => (x.Config != null && x.Config.StartTime >= 0 && x.Config.EndTime >= 0) &&
                                                                       ((x.Config.StartTime <= request.Config.StartTime && x.Config.EndTime >= request.Config.EndTime) ||
                                                                           (x.Action == EnumTechieAction.GoodLateNight && x.Config.EndTime < request.Config.EndTime ||
                                                                            x.Config.StartTime > request.Config.StartTime)
                                                                       ));

            var techieActionModel = _mapper.Map<TechieActionModel>(techieActionFilter);
            if (techieActions == null || techieActionModel == null || string.IsNullOrEmpty(techieActionModel.TemplateMessage))
            {
                return methodResult;
            }

            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            var studentId = studentResult?.Content?.Result?.Id;
            if (studentId == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentId), studentId);
                return methodResult;
            }
            StudentTechie studentTechie = new StudentTechie
            {
                Message = string.Format(CultureInfo.InvariantCulture, techieActionModel.TemplateMessage, request?.Config?.Value ?? default),
                StudentId = (Guid)studentId,
                Config = request?.Config ?? default,
                TechieActionId = techieActionModel.Id,
            };

            #region Validate

            if (request == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentId), studentId);
                return methodResult;
            }

            // Check xem techie chào ngày mới đã tồn tại hay chưa
            bool isExistsTechieGreeting = _studentTechieRepository.Queryable.Any(x => x.TechieAction != null &&
                                                                                      x.TechieActionId == techieActionModel.Id &&
                                                                                      x.TechieAction.Feature == request.TechieFeature &&
                                                                                      x.TechieAction.Action == request.Actions &&
                                                                                      x.CreatedUserId == _authContext.CurrentUserId);

            if (isExistsTechieGreeting && request.TechieFeature == EnumTechieFeature.Greeting)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(studentId), studentId);
                return methodResult;
            }

            #endregion Validate

            await _studentTechieRepository.ExecuteTransactionAsync(async () =>
            {
                await _studentTechieRepository.BulkMergeAsync(new List<StudentTechie> { studentTechie });

                StudentTechieMessageModel socketModel = new StudentTechieMessageModel
                {
                    Message = studentTechie.Message,
                    StudentId = studentTechie.StudentId,
                    Feature = techieActionModel.Feature,
                    Action = techieActionModel.Action,
                    Priority = techieActionModel.Priority,
                };

                _logger.LogInformation("Send Techie To Socket");
                await _techieSendMessagePublisher.Publish(socketModel, cancellationToken);
                return methodResult;
            });

            methodResult.StatusCode = StatusCodes.Status201Created;
            return methodResult;
        }
    }
}
