// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.UserQuery
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;

    public class GetUserQuery : IRequest<MethodResult<UserProfileModel>>
    {
        public Guid UserId { get; set; }
    }

    public class GetUserQueryHandler : IRequestHandler<GetUserQuery, MethodResult<UserProfileModel>>
    {
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;

        public GetUserQueryHandler(IMapper mapper, UserManager<User> userManager)
        {
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<MethodResult<UserProfileModel>> Handle(GetUserQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<UserProfileModel> methodResult = new MethodResult<UserProfileModel>();

            var user = await _userManager.FindByIdAsync(request.UserId.ToString());

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
                                                   .FirstOrDefaultAsync(x => x.Id == request.UserId.ToString(), cancellationToken);
            }
            else if (userRoles.FirstOrDefault() == EnumRole.Teacher.ToString())
            {
                userView = await _userManager.Users.Include(x => x.Human)
                                                   .ThenInclude(x => x!.Teacher)
                                                   .ThenInclude(x => x!.TeacherBankAccount)
                                                   .FirstOrDefaultAsync(x => x.Id == request.UserId.ToString(), cancellationToken);
            }
            else if (userRoles.FirstOrDefault() == EnumRole.Student.ToString())
            {
                userView = await _userManager.Users.Include(x => x.Human)
                                                   .ThenInclude(x => x!.Student)
                                                   .FirstOrDefaultAsync(x => x.Id == request.UserId.ToString() && x.EmailConfirmed, cancellationToken);
                if (userView?.Human?.Student?.CreatedByParent == false)
                {
                    userView = await _userManager.Users.Include(x => x.Human)
                                                  .ThenInclude(x => x!.Student)
                                                  .ThenInclude(x => x!.ParentStudents)
                                                  .ThenInclude(x => x!.Parent)
                                                  .ThenInclude(x => x!.Human)
                                                  .FirstOrDefaultAsync(x => x.Id == request.UserId.ToString(), cancellationToken);
                }
            }
            else if (userRoles.FirstOrDefault() == EnumRole.Parent.ToString())
            {
                userView = await _userManager.Users.Include(x => x.Human)
                                                   .ThenInclude(x => x!.Parent)
                                                   .ThenInclude(x => x!.ParentStudents.Where(y => !y.IsDeleted))
                                                   .ThenInclude(x => x.Student)
                                                   .FirstOrDefaultAsync(x => x.Id == request.UserId.ToString() && x.EmailConfirmed, cancellationToken);
            }
            else if (userRoles.FirstOrDefault() == EnumRole.Moderator.ToString() || userRoles.FirstOrDefault() == EnumRole.MasterAdmin.ToString() || userRoles.FirstOrDefault() == EnumRole.Admin.ToString())
            {
                userView = await _userManager.Users.Include(x => x.Human)
                                                 .FirstOrDefaultAsync(x => x.Id == request.UserId.ToString(), cancellationToken);
            }
            var userModel = _mapper.Map<UserProfileModel>(userView ?? user);
            if (userView != null)
            {
                _mapper.Map(userView.Human, userModel);

                if (userRoles.FirstOrDefault() == EnumRole.Student.ToString() && userView.Human?.Student?.CreatedByParent == false && userView.Human?.Student?.ParentStudents.Count > 0)
                {
                    userModel!.Parent = _mapper.Map<ParentModel>(userView!.Human!.Student!.ParentStudents!.FirstOrDefault()!.Parent);
                    _mapper.Map(userView!.Human!.Student, userModel);
                }

                if (userRoles.FirstOrDefault() == EnumRole.Parent.ToString() && userView.Human?.Parent?.ParentStudents != null && userView.Human?.Parent?.ParentStudents.Count > 0)
                {
                    userModel!.Students = _mapper.Map<List<StudentModel>>(userView!.Human!.Parent!.ParentStudents.Select(x => x.Student).ToList());
                    _mapper.Map(userView!.Human!.Parent, userModel);
                }

                if (userRoles.FirstOrDefault() == EnumRole.Teacher.ToString() && userView.Human?.Teacher != null)
                {
                    userModel!.TeacherBankAccount = _mapper.Map<TeacherBankAccountModel>(userView!.Human!.Teacher!.TeacherBankAccount);
                    _mapper.Map(userView!.Human!.Teacher, userModel);
                }

                if (userRoles.FirstOrDefault() == EnumRole.CSO.ToString())
                {
                    _mapper.Map(userView!.Human!.CSO, userModel);
                }
                userModel.Roles = userRoles;
            }

            methodResult.Result = userModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
