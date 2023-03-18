namespace Fsel.Identity.Application.Commands.ParentCmd
{
    using System.Transactions;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Identity.Application.Commands.AuthCmd;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.Parents;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;

    public class CreateStudentByParentCommand : CreateStudentByParentCommandModel, IRequest<MethodResult<UserModel>>
    {
    }

    public class CreateStudentByParentCommandHandler : IRequestHandler<CreateStudentByParentCommand, MethodResult<UserModel>>
    {
        private readonly UserManager<User> _userManager;
        private readonly AuthContext _authContext;
        private readonly IParentRepository _parentRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public CreateStudentByParentCommandHandler(UserManager<User> userManager,
            AuthContext authContext,
            IParentRepository parentRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _userManager = userManager;
            _authContext = authContext;
            _parentRepository = parentRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<MethodResult<UserModel>> Handle(CreateStudentByParentCommand request, CancellationToken cancellationToken)
        {
            MethodResult<UserModel> methodResult = new MethodResult<UserModel>();

            var isUser = await _userManager.Users.AnyAsync(e => e.UserName == request.UserName, cancellationToken: cancellationToken);
            if (isUser)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddErrorMessage(
                    nameof(EnumParentErrorCode.PA05C),
                    new[] { MethodHelper.GenerateErrorResult(nameof(request.UserName), request?.UserName) });
                return methodResult;
            }

            var userParent = await _userManager.FindByNameAsync(request?.UserName ?? string.Empty);
            User user = new();
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    IdentityResult result;
                    var parent = await _parentRepository.Queryable.Include(e => e.ParentStudents)
                                                                  .FirstOrDefaultAsync(e => e.Id == _authContext.CurrentUserId, cancellationToken: cancellationToken);
                    if (parent != null && parent.ParentStudents.Count <= 2)
                    {
                        user = _mapper.Map<User>(request);

                        //user.Human = _mapper.Map<Human>(request);

                        //user.Human.Student = _mapper.Map<Student>(request);
                        //user.Human.Student.ParentStudents = (ICollection<ParentStudent>)parent.ParentStudents.Select(e => new ParentStudent
                        //{
                        //    ParentId = parent.Id
                        //});
                        result = await _userManager.CreateAsync(user, request?.Password ?? string.Empty);

                        if (!result.Succeeded)
                        {
                            methodResult.StatusCode = StatusCodes.Status400BadRequest;
                            methodResult.AddErrorMessage(nameof(EnumParentErrorCode.PA04V));
                            return methodResult;
                        }
                        await _userManager.AddToRoleAsync(user, "Student");
                    }

                    var sendResult = await _mediator.Send(new SendOTPCommand { Email = userParent?.Email ?? string.Empty }, cancellationToken).ConfigureAwait(false);

                    if (!sendResult.IsOK)
                    {
                        methodResult.StatusCode = StatusCodes.Status400BadRequest;
                        methodResult.AddErrorMessage(nameof(EnumParentErrorCode.PA05V));
                        return methodResult;
                    }
                    scope.Complete();
                }
                catch
                {
                    methodResult.StatusCode = StatusCodes.Status400BadRequest;
                    methodResult.AddErrorMessage(nameof(EnumParentErrorCode.PA06V));
                    scope.Dispose();
                }
            }
            methodResult.StatusCode = StatusCodes.Status201Created;
            methodResult.Result = _mapper.Map<UserModel>(user);
            return methodResult;
        }
    }
}
