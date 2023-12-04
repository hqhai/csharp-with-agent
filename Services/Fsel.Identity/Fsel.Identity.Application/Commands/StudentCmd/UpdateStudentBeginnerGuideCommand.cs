// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.StudentCmd
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
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

        public UpdateStudentBeginnerGuideCommandHandler(IStudentRepository studentRepository, IMapper mapper, AuthContext authContext)
        {
            _studentRepository = studentRepository;
            _mapper = mapper;
            _authContext = authContext;
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
                await _studentRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<StudentModel>(student);
                return methodResult;
            });
            return methodResult;
        }
    }
}
