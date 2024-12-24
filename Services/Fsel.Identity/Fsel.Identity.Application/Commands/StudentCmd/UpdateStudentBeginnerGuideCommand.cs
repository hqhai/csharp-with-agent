// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.StudentCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Identity.Application.Commands.AdminCmd;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.Students;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateStudentBeginnerGuideCommand : UpdateStudentBeginnerGuideCommandModel, IRequest<MethodResult<StudentModel>>
    {
    }

    public class UpdateStudentBeginnerGuideCommandHandler : IRequestHandler<UpdateStudentBeginnerGuideCommand, MethodResult<StudentModel>>
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;
        private readonly IMediator _mediator;

        public UpdateStudentBeginnerGuideCommandHandler(IStudentRepository studentRepository, IMapper mapper, AuthContext authContext, IMediator mediator)
        {
            _studentRepository = studentRepository;
            _mapper = mapper;
            _authContext = authContext;
            _mediator = mediator;
        }

        public async Task<MethodResult<StudentModel>> Handle(UpdateStudentBeginnerGuideCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<StudentModel> methodResult = new MethodResult<StudentModel>();

            var student = await _studentRepository.Queryable.Include(x => x.Human)
                                                  .FirstOrDefaultAsync(x => x.Human!.UserId == _authContext.CurrentUserId, cancellationToken: cancellationToken);

            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }

            _mapper.Map(request, student);
            await _studentRepository.ExecuteTransactionAsync(async () =>
            {
                student = _studentRepository.Update(student);
                await _mediator.Send(new ToolUpdateBeginnerGuideStudentCommand { StudentId = student.Id }, cancellationToken);
                await _studentRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<StudentModel>(student);
                return methodResult;
            });
            return methodResult;
        }
    }
}
