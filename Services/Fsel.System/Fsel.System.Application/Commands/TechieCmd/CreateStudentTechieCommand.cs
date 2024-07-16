// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.TechieCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
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


            var techieActionFilter = techieActions.FirstOrDefault(x => x.Config!.StartTime <= request!.Config!.StartTime && x.Config.EndTime >= request.Config.EndTime);
            var techieActionModel = _mapper.Map<TechieActionModel>(techieActionFilter);

            if (techieActions == null || techieActionModel == null)
            {
                return methodResult;
            }

            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            var studentId = studentResult?.Content?.Result?.Id;

            StudentTechie studentTechie = new StudentTechie
            {
                Message = string.Format(CultureInfo.InvariantCulture, techieActionModel!.TemplateMessage!, request?.Config?.Value ?? default),
                StudentId = (Guid)studentId!,
                Config = request?.Config ?? default,
                TechieActionId = techieActionModel.Id,
            };

            #region Validate
            bool isExistsTechieGreeting = _studentTechieRepository.Queryable.Any(x => x.TechieActionId == techieActionModel.Id && x.TechieAction!.Feature == request!.TechieFeature && x.TechieAction.Action == request.Actions && x.CreatedUserId == _authContext.CurrentUserId);

            if (isExistsTechieGreeting && request!.TechieFeature == EnumTechieFeature.Greeting)
            {
                return methodResult;
            }
            #endregion

            await _studentTechieRepository.ExecuteTransactionAsync(async () =>
            {
                _studentTechieRepository.Add(studentTechie);

                await _studentTechieRepository.UnitOfWork.SaveEntitiesAsync().ConfigureAwait(false);

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
