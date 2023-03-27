// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.ParentCmd
{
    using System.Threading;
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
        private readonly IParentRepository _parentRepository;

        public CreateStudentByParentCommandHandler(UserManager<User> userManager,
            IMapper mapper,
            AuthContext authContext,
            IParentRepository parentRepository,
            IHumanRepository humanRepository)
        {
            _userManager = userManager;
            _mapper = mapper;
            _authContext = authContext;
            _parentRepository = parentRepository;
            _humanRepository = humanRepository;
        }

        public async Task<MethodResult<UserModel>> Handle(CreateStudentByParentCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<UserModel> methodResult = new MethodResult<UserModel>();

            #region Validation

            var isUsernameExist = await _userManager.Users.AnyAsync(e => e.UserName == request.UserName, cancellationToken: cancellationToken);
            if (isUsernameExist)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddError(
                    nameof(EnumAuthErrorCode.OldPasswordIncorrect),
                    new[] { MethodHelper.GenerateErrorResult(nameof(request.UserName), request.UserName) });
                return methodResult;
            }

            var parent = await _parentRepository.Queryable.Include(x => x.Human)
                                                .Include(x => x.ParentStudents.Where(n => !n.IsDeleted))
                                                .FirstOrDefaultAsync(x => x.Human != null && x.Human.UserId == _authContext.CurrentUserId.ToString(), cancellationToken);

            var userparent = await _userManager.Users.Include(e => e.Human)
                                             .ThenInclude(e => e != null ? e.Parent : default)
                                             .ThenInclude(e => e != null ? e.ParentStudents : default)
                                             .Where(e => e.Id == _authContext.CurrentUserId.ToString())
                                             .FirstOrDefaultAsync(cancellationToken: cancellationToken);

            if (parent == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumParentErrorCode.ParentNull));
                return methodResult;
            }

            var countStudentPr = parent.ParentStudents.Count;
            if (countStudentPr >= 2)
            {
                methodResult.AddErrorBadRequest(nameof(EnumParentErrorCode.ParentHadMoreTwoStudents));
                return methodResult;
            }

            #endregion Validation

            var user = await CreateUserStudentAsync(request, parent);
            if (user == null)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddError(nameof(EnumParentErrorCode.CreateStudentFail));
                return methodResult;
            }

            methodResult.Result = _mapper.Map<UserModel>(user);
            return methodResult;
        }

        private async Task<User?> CreateUserStudentAsync(CreateStudentByParentCommandModel request, Parent parent)
        {
            var user = _mapper.Map<User>(request);
            user.FullName = request.Name;
            user.EmailConfirmed = true;
            await _userManager.AddToRoleAsync(user, EnumRoleRegister.Student.ToString());

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            await _userManager.ConfirmEmailAsync(user, token);

            var identityResult = await _userManager.CreateAsync(user, request.Password ?? string.Empty);
            await CreateHumanAsync(request, user, parent);

            if (!identityResult.Succeeded)
            {
                return null;
            }
            return user;
        }

        private async Task CreateHumanAsync(CreateStudentByParentCommandModel request, User user, Parent parent)
        {
            var human = new Human
            {
                UserId = user.Id,
                FullName = request.Name,
                Gender = request.Gender,
                Birthday = request.Birthday,
                AvatarPath = request.AvatarPath,
                Student = new Student
                {
                    School = request.School,
                    CourseLevel = EnumCourseLevel.A2,
                    ParentStudents = new List<ParentStudent> { new ParentStudent { ParentId = parent.Id } }
                }
            };

            _humanRepository.Add(human);
            await _humanRepository.UnitOfWork.SaveEntitiesAsync().ConfigureAwait(false);
        }
    }
}
