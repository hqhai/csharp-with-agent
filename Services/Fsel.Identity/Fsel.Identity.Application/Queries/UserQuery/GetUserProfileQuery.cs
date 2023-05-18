// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.UserQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;

    public class GetUserProfileQuery : IRequest<MethodResult<UserProfileModel>>
    {
    }

    public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, MethodResult<UserProfileModel>>
    {
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;
        private readonly UserManager<User> _userManager;

        public GetUserProfileQueryHandler(IMapper mapper, AuthContext authContext, UserManager<User> userManager,ITraining)
        {
            _mapper = mapper;
            _authContext = authContext;
            _userManager = userManager;
        }

        public async Task<MethodResult<UserProfileModel>> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<UserProfileModel> methodResult = new MethodResult<UserProfileModel>();

            var user = await _userManager.FindByIdAsync(_authContext.CurrentUserId.ToString());

            if (user == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumUserErrorCode.UserNotExist));
                return methodResult;
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            User? userView = null;
            if (userRoles.FirstOrDefault() == EnumRole.CSO.ToString())
            {
                userView = await _userManager.Users.Include(x => x.Human)
                                                   .ThenInclude(x => x!.CSO)
                                                   .FirstOrDefaultAsync(x => x.Id == _authContext.CurrentUserId.ToString(), cancellationToken);
            }
            else if (userRoles.FirstOrDefault() == EnumRole.Teacher.ToString())
            {
                userView = await _userManager.Users.Include(x => x.Human)
                                                   .ThenInclude(x => x!.Teacher)
                                                   .ThenInclude(x => x!.TeacherBankAccounts)
                                                   .FirstOrDefaultAsync(x => x.Id == _authContext.CurrentUserId.ToString(), cancellationToken);
            }
            else if (userRoles.FirstOrDefault() == EnumRole.Student.ToString())
            {

                userView = await _userManager.Users.Include(x => x.Human)
                                                   .ThenInclude(x => x!.Student)
                                                   .ThenInclude(x => x!.ParentStudents)
                                                   .FirstOrDefaultAsync(x => x.Id == _authContext.CurrentUserId.ToString(), cancellationToken);
                var student = userView!.Human!.Student!;
                if (userView!.Human!.Student!.CreatedByParent == false && student.ParentStudents != null && student.ParentStudents.Count > 0)
                {
                    userView = await _userManager.Users.Include(x => x.Human)
                                                  .ThenInclude(x => x!.Student)
                                                  .ThenInclude(x => x!.ParentStudents)
                                                  .ThenInclude(x => x!.Parent)
                                                  .ThenInclude(x => x!.Human)
                                                  .FirstOrDefaultAsync(x => x.Id == _authContext.CurrentUserId.ToString(), cancellationToken);
                }
            }
            else if (userRoles.FirstOrDefault() == EnumRole.Parent.ToString())
            {
                userView = await _userManager.Users.Include(x => x.Human)
                                                   .ThenInclude(x => x!.Parent)
                                                   .ThenInclude(x => x!.ParentStudents)
                                                   .FirstOrDefaultAsync(x => x.Id == _authContext.CurrentUserId.ToString(), cancellationToken);
                if (userView!.Human!.Parent!.ParentStudents != null && userView!.Human!.Parent!.ParentStudents.Count > 0)
                {
                    userView = await _userManager.Users.Include(x => x.Human)
                                                                      .ThenInclude(x => x!.Parent)
                                                                      .ThenInclude(x => x!.ParentStudents)
                                                                      .ThenInclude(x => x.Student)
                                                                      .ThenInclude(x => x!.Human)
                                                                      .FirstOrDefaultAsync(x => x.Id == _authContext.CurrentUserId.ToString(), cancellationToken);
                }
            }
            else if (userRoles.FirstOrDefault() == EnumRole.Moderator.ToString() || userRoles.FirstOrDefault() == EnumRole.MasterAdmin.ToString() || userRoles.FirstOrDefault() == EnumRole.Admin.ToString())
            {
                userView = await _userManager.Users.Include(x => x.Human)
                                                 .FirstOrDefaultAsync(x => x.Id == _authContext.CurrentUserId.ToString(), cancellationToken);
            }

            var userModel = _mapper.Map<UserProfileModel>(userView ?? user);
            _mapper.Map(userView!.Human, userModel);

            if (userRoles.FirstOrDefault() == EnumRole.Student.ToString() && userView!.Human!.Student!.CreatedByParent == false && userView!.Human!.Student!.ParentStudents.Count > 0)
            {
                userModel!.Parent = _mapper.Map<ParentModel>(userView!.Human!.Student!.ParentStudents!.FirstOrDefault()!.Parent);
                _mapper.Map(userView!.Human!.Student, userModel);
            }

            if (userRoles.FirstOrDefault() == EnumRole.Parent.ToString() && userView!.Human!.Parent!.ParentStudents != null && userView!.Human!.Parent!.ParentStudents.Count > 0)
            {
                userModel!.Students = _mapper.Map<List<StudentModel>>(userView!.Human!.Parent!.ParentStudents.Select(x => x.Student).ToList());
                _mapper.Map(userView!.Human!.Parent, userModel);
            }

            if (userRoles.FirstOrDefault() == EnumRole.Teacher.ToString())
            {
                userModel!.TeacherBankAccounts = _mapper.Map<IList<TeacherBankAccountModel>>(userView!.Human!.Teacher!.TeacherBankAccounts);
                _mapper.Map(userView!.Human!.Teacher, userModel);
            }

            if (userRoles.FirstOrDefault() == EnumRole.CSO.ToString())
            {
                _mapper.Map(userView!.Human!.CSO, userModel);
            }

            userModel.Roles = userRoles;
            methodResult.Result = userModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
