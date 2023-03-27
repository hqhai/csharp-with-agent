// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.StudentCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.Students;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;

    public class UpdateStudentByClassCommand : UpdateStudentByClassCommandModel, IRequest<MethodResult<StudentModel>>
    {
    }

    public class UpdateStudentByClassCommandHandler : IRequestHandler<UpdateStudentByClassCommand, MethodResult<StudentModel>>
    {
        private readonly UserManager<User> _userManager;
        private readonly AuthContext _authContext;
        private readonly IStudentRepository _studentRepository;
        private readonly IMapper _mapper;

        public UpdateStudentByClassCommandHandler(UserManager<User> userManager,
            AuthContext authContext,
            IStudentRepository studentRepository,
            IMapper mapper)
        {
            _userManager = userManager;
            _authContext = authContext;
            _studentRepository = studentRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<StudentModel>> Handle(UpdateStudentByClassCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<StudentModel> methodResult = new MethodResult<StudentModel>();

            var user = await _userManager.Users.Include(x => x.Human)
                                                  .ThenInclude(x => x.Student)
                                                  .FirstOrDefaultAsync(x => x.Id == request.UserId.ToString(), cancellationToken: cancellationToken);
            if (user == null)
            {
                methodResult.AddErrorBadRequest(
                   nameof(EnumStudentErrorCode.StudentNull),
                   nameof(request.UserId), request.UserId);
                return methodResult;
            }
            else
            {
                var student = user.Human?.Student;
                if (student == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumStudentErrorCode.UserNull), nameof(request.UserId), request.UserId);
                    return methodResult;
                }
                student.ClassId = request.ClassId;
                await _studentRepository.ExecuteTransactionAsync(async () =>
                {
                    student = _studentRepository.Update(student);

                    await _studentRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                    methodResult.StatusCode = StatusCodes.Status201Created;
                    methodResult.Result = _mapper.Map<StudentModel>(student);
                    return methodResult;
                });
            }
            return methodResult;
        }
    }
}
