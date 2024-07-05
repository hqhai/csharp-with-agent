// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.TechieCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.System.Application.Services.UserServices;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.Techie;
    using global::System.Globalization;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class CreateStudentTechieCommand : SaveStudentTechieCommandModel, IRequest<MethodResult<StudentTechie>>
    {
    }

    public class CreateStudentTechieCommandHandler : IRequestHandler<CreateStudentTechieCommand, MethodResult<StudentTechie>>
    {
        private readonly IMapper _mapper;
        private readonly ITechieActionRepository _techieActionRepository;
        private readonly IStudentTechieRepository _studentTechieRepository;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;


        public CreateStudentTechieCommandHandler(IMapper mapper, ITechieActionRepository techieActionRepository, IStudentTechieRepository studentTechieRepository, IUserService userService, AuthContext authContext)
        {
            _mapper = mapper;
            _techieActionRepository = techieActionRepository;
            _studentTechieRepository = studentTechieRepository;
            _userService = userService;
            _authContext = authContext;
        }

        public async Task<MethodResult<StudentTechie>> Handle(CreateStudentTechieCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<StudentTechie>();


            var techieAction = _techieActionRepository.Queryable.FirstOrDefault(x => x.Action == request.Actions && x.Feature == request.TechieFeature && (x.Config!.StartTime <= request!.Config!.StartTime && x.Config.EndTime >= request.Config.EndTime));

            if (techieAction == null)
            {
                return methodResult;
            }

            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            var studentId = studentResult?.Content?.Result?.Id;

            StudentTechie studentTechie = new StudentTechie
            {
                Message = string.Format(CultureInfo.InvariantCulture, techieAction.TemplateMessage!, request?.Config?.Value ?? default),
                StudentId = (Guid)studentId!,
                Config = request?.Config ?? default,
                TechieActionId = techieAction.Id,
                TechieAction = techieAction,
            };





            methodResult.StatusCode = StatusCodes.Status201Created;
            // methodResult.Result = _mapper.Map<StudentTechieModel>();
            return methodResult;

        }
    }
}
