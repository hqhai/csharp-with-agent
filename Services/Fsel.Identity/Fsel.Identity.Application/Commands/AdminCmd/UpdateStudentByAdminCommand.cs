// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.AdminCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.Students;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;

    public class UpdateStudentByAdminCommand : UpdateStudentByAdminCommandModel, IRequest<MethodResult<UserModel>>
    {
    }

    public class UpdateStudentByAdminCommandHandler : IRequestHandler<UpdateStudentByAdminCommand, MethodResult<UserModel>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        private readonly IHumanRepository _humanRepository;

        public UpdateStudentByAdminCommandHandler(UserManager<User> userManager,
            IMapper mapper,
            IHumanRepository humanRepository)
        {
            _userManager = userManager;
            _mapper = mapper;
            _humanRepository = humanRepository;
        }

        public async Task<MethodResult<UserModel>> Handle(UpdateStudentByAdminCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<UserModel>();
            var userView = await _userManager.Users.Include(x => x.Human)
                                                   .ThenInclude(x => x!.Student)
                                                   .ThenInclude(x => x!.ParentStudents)
                                                   .FirstOrDefaultAsync(x => x.Human != null && x.Human.Student != null && x.Human.Student.Id == request.Id, cancellationToken);
            if (userView == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumUserErrorCode.UserNotExist));
                return methodResult;
            }
            var student = userView.Human!.Student!;
            if (request.Parent != null)
            {
                if (student.ParentStudents == null || student.ParentStudents.Count == 0)
                {
                    if (string.IsNullOrEmpty(request.Parent.FullName))
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumParentErrorCode.ParentFullNameNotNull));
                        return methodResult;
                    }

                    Human newHuman = _mapper.Map<Human>(request.Parent);
                    newHuman.Parent = _mapper.Map<Parent>(request.Parent);
                    newHuman.Parent.ParentStudents.Add(new ParentStudent
                    {
                        Student = student
                    });
                    _humanRepository.Add(newHuman);
                    await _humanRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    userView = await _userManager.Users.Include(x => x.Human)
                                               .ThenInclude(x => x!.Student)
                                               .ThenInclude(x => x!.ParentStudents)
                                               .ThenInclude(x => x.Parent)
                                               .ThenInclude(x => x!.Human)
                                               .FirstOrDefaultAsync(x => x.Human != null && x.Human.Student != null && x.Human.Student.Id == request.Id, cancellationToken);
                    var human = userView?.Human?.Student?.ParentStudents.FirstOrDefault()?.Parent?.Human;
                    if (human != null)
                    {
                        _mapper.Map(request.Parent, human);
                        _mapper.Map(request.Parent, human.Parent);
                        _humanRepository.Update(human);
                        await _humanRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                    }
                }
            }
            if (userView != null)
            {
                _mapper.Map(request, userView.Human?.Student);
                _mapper.Map(request, userView);
                _mapper.Map(request, userView.Human);
                await _userManager.UpdateAsync(userView);
            }
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = _mapper.Map<UserModel>(userView);
            return methodResult;
        }
    }
}
