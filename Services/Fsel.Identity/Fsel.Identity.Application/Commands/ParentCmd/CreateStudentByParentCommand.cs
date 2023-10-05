// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.ParentCmd
{
    using System.Threading;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.Parents;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Identity.Infrastructure.Repositories;
    using Fsel.Shared.Enums;
    using MediatR;
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
        private readonly IPlatformRepository _platformRepository;

        public CreateStudentByParentCommandHandler(UserManager<User> userManager,
            IMapper mapper,
            AuthContext authContext,
            IParentRepository parentRepository,
            IHumanRepository humanRepository,
            IPlatformRepository platformRepository)
        {
            _userManager = userManager;
            _mapper = mapper;
            _authContext = authContext;
            _parentRepository = parentRepository;
            _humanRepository = humanRepository;
            _platformRepository = platformRepository;
        }

        public async Task<MethodResult<UserModel>> Handle(CreateStudentByParentCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<UserModel> methodResult = new MethodResult<UserModel>();

            #region Validation

            var isUsernameExist = await _userManager.Users.AnyAsync(e => e.UserName == request.UserName, cancellationToken: cancellationToken);
            if (isUsernameExist)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(request.UserName), request.UserName);
                return methodResult;
            }

            var parent = await _parentRepository.Queryable.Include(x => x.Human)
                                                .Include(x => x.ParentStudents.Where(n => !n.IsDeleted))
                                                .FirstOrDefaultAsync(x => x.Human!.UserId == _authContext.CurrentUserId.ToString(), cancellationToken);

            if (parent == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(parent));
                return methodResult;
            }

            var countStudentPr = parent.ParentStudents.Count;
            if (countStudentPr >= 2)
            {
                methodResult.AddErrorBadRequest(nameof(EnumParentErrorCode.ParentHadMoreTwoStudents));
                return methodResult;
            }

            #endregion Validation

            var user = await CreateUserStudentAsync(request, parent, cancellationToken);
            if (user == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumParentErrorCode.CreateStudentFail));
                return methodResult;
            }

            methodResult.Result = _mapper.Map<UserModel>(user);
            return methodResult;
        }

        private async Task<User?> CreateUserStudentAsync(CreateStudentByParentCommandModel request, Parent parent, CancellationToken cancellationToken)
        {
            var user = _mapper.Map<User>(request);

            #region Add Platform to User
            var platform = await _platformRepository.GetPlatformAsync(EnumPlatformCode.LMS, cancellationToken);
            if (platform != null)
            {
                user.UserPlatforms.Add(new UserPlatform
                {
                    PlatformId = platform.Id
                });
            }
            #endregion

            var identityResult = await _userManager.CreateAsync(user, request.Password ?? string.Empty);
            if (!identityResult.Succeeded)
            {
                return null;
            }

            await _userManager.AddToRoleAsync(user, EnumRoleRegister.Student.ToString());

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            await _userManager.ConfirmEmailAsync(user, token);

            await CreateHumanAsync(request, user, parent);

            return user;
        }

        private async Task CreateHumanAsync(CreateStudentByParentCommandModel request, User user, Parent parent)
        {
            var human = new Human
            {
                UserId = user.Id,
                FullName = request.FullName,
                AvatarPath = request.AvatarPath,
                Student = new Student
                {
                    School = request.School,
                    CreatedByParent = true,
                    CourseLevel = EnumCourseLevel.A2,
                    ParentStudents = new List<ParentStudent> { new ParentStudent { ParentId = parent.Id } },
                    Occupation = "Student"
                }
            };

            _humanRepository.Add(human);
            await _humanRepository.UnitOfWork.SaveEntitiesAsync().ConfigureAwait(false);
        }
    }
}
