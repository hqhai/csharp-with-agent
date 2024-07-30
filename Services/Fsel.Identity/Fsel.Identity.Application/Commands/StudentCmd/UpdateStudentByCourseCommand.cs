// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.StudentCmd
{
    using System;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateStudentByCourseCommand : IRequest<MethodResult<StudentModel>>
    {
        public Guid CourseId { get; set; }
    }

    public class UpdateStudentByCourseCommandHandler : IRequestHandler<UpdateStudentByCourseCommand, MethodResult<StudentModel>>
    {
        private readonly IStudentRepository _studentRepository;
        private readonly AuthContext _authContext;
        private readonly IMapper _mapper;

        public UpdateStudentByCourseCommandHandler(IStudentRepository studentRepository, AuthContext authContext, IMapper mapper)
        {
            _studentRepository = studentRepository;
            _authContext = authContext;
            _mapper = mapper;
        }

        public async Task<MethodResult<StudentModel>> Handle(UpdateStudentByCourseCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<StudentModel>();

            var student = await _studentRepository.Queryable
                                                   .FirstOrDefaultAsync(x => x.UserId == _authContext.CurrentUserId, cancellationToken: cancellationToken);
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }
            student.CourseId = request.CourseId;
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
