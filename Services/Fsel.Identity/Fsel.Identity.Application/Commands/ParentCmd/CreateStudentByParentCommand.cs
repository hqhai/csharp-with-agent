// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.ParentCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
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
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;
        private readonly IHumanRepository _humanRepository;

        public CreateStudentByParentCommandHandler(UserManager<User> userManager,
            IMapper mapper,
            AuthContext authContext,
            IHumanRepository humanRepository)
        {
            _userManager = userManager;
            _mapper = mapper;
            _authContext = authContext;
            _humanRepository = humanRepository;
        }

        public async Task<MethodResult<UserModel>> Handle(CreateStudentByParentCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<UserModel> methodResult = new MethodResult<UserModel>();
            var isUser = await _userManager.Users.AnyAsync(e => e.UserName == request.UserName, cancellationToken: cancellationToken);
            if (isUser)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddError(
                    nameof(EnumAuthErrorCode.AU05V),
                    new[] { MethodHelper.GenerateErrorResult(nameof(request.UserName), request.UserName) });
                return methodResult;
            }

            User user = new();
            IdentityResult result;
            var userparent = await _userManager.Users.Include(e => e.Human)
                                             .ThenInclude(e => e != null ? e.Parent : default)
                                             .ThenInclude(e => e != null ? e.ParentStudents : default)
                                             .Where(e => e.Id == _authContext.CurrentUserId.ToString())
                                             .FirstOrDefaultAsync(cancellationToken: cancellationToken);

            if (userparent == null)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddError(nameof(EnumParentErrorCode.PA03V));
                return methodResult;
            }

            var countStudentPr = userparent.Human?.Parent?.ParentStudents.Count;
            if (countStudentPr >= 2)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddError(nameof(EnumParentErrorCode.PA07V));
                return methodResult;
            }

            var parent = userparent.Human?.Parent;
            if (parent == null)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddError(nameof(EnumParentErrorCode.PA03V));
                return methodResult;
            }

            user = _mapper.Map<User>(request);
            user.FullName = request.Name;
            user.EmailConfirmed = true;
            var human = new Human
            {
                UserId = user.Id,
                FullName = request.Name,
                AvatarPath = request.AvatarPath,
                CreatedFullName = userparent.FullName ?? string.Empty,
                Student = new Student
                {
                    School = request.School,
                    CreatedFullName = userparent.FullName ?? string.Empty,
                    CourseLevel = EnumCourseLevel.A2,
                    ParentStudents = new List<ParentStudent> { new ParentStudent { ParentId = parent.Id } }
                }
            };
            result = await _userManager.CreateAsync(user, request.Password ?? string.Empty);
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddError(nameof(EnumParentErrorCode.PA04V));
                return methodResult;
            }
            await _userManager.AddToRoleAsync(user, EnumRoleRegister.Student.ToString());

            await _humanRepository.ExecuteTransactionAsync(async () =>
            {
                _humanRepository.Add(human);
                await _humanRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<UserModel>(user);
                return methodResult;
            });

            return methodResult;
        }
    }
}
