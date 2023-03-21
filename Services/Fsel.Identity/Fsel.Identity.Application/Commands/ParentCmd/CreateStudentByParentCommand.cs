// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.ParentCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums;
    using Fsel.Common.Helpers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.Models.CommandModels.Parents;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;

    public class CreateStudentByParentCommand : CreateStudentByParentCommandModel, IRequest<MethodResult<UserModel>>
    {
        public Guid Ids { get; set; }
    }

    public class CreateStudentByParentCommandHandler : IRequestHandler<CreateStudentByParentCommand, MethodResult<UserModel>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;

        public CreateStudentByParentCommandHandler(UserManager<User> userManager,
            IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<MethodResult<UserModel>> Handle(CreateStudentByParentCommand request, CancellationToken cancellationToken)
        {
            MethodResult<UserModel> methodResult = new MethodResult<UserModel>();
            if (request != null)
            {
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
                                                     .Where(e => e.Id == request.Ids.ToString())
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
                user.Human = new Human
                {
                    FullName = request.Name,
                    AvatarPath = request.AvatarPath,
                    CreatedFullName = userparent.FullName ?? string.Empty,
                };

                user.Human.Student = new Student
                {
                    School = request.School,
                    CreatedFullName = userparent.FullName ?? string.Empty,
                    CourseLevel = EnumCourseLevel.A2
                };

                user.Human.Student.ParentStudents = parent.ParentStudents.Select(e => new ParentStudent
                {
                    ParentId = parent.Id
                }).ToList();

                result = await _userManager.CreateAsync(user, request.Password ?? string.Empty);

                if (!result.Succeeded)
                {
                    methodResult.StatusCode = StatusCodes.Status400BadRequest;
                    methodResult.AddError(nameof(EnumParentErrorCode.PA04V));
                    return methodResult;
                }
                await _userManager.AddToRoleAsync(user, EnumRoleRegister.Student.ToString());

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<UserModel>(user);
            }
            methodResult.StatusCode = StatusCodes.Status400BadRequest;
            methodResult.AddError(nameof(EnumParentErrorCode.PA08V));
            return methodResult;
        }
    }
}
