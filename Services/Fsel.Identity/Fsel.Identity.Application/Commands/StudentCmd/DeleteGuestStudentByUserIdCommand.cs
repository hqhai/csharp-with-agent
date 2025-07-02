// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.StudentCmd
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class DeleteGuestStudentByUserIdCommand : IRequest<VoidMethodResult>
    {
        public Guid? Id { get; set; }
    }

    public class DeleteGuestStudentByUserIdCommandHandler : IRequestHandler<DeleteGuestStudentByUserIdCommand, VoidMethodResult>
    {
        private readonly UserManager<User> _userManager;
        private readonly IStudentRepository _studentRepository;

        public DeleteGuestStudentByUserIdCommandHandler(UserManager<User> userManager, IStudentRepository studentRepository)
        {
            _userManager = userManager;
            _studentRepository = studentRepository;
        }

        public async Task<VoidMethodResult> Handle(DeleteGuestStudentByUserIdCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            VoidMethodResult methodResult = new VoidMethodResult();

            var user = await _userManager.Users.Include(x => x.Student).Where(x => x.Id == request.Id).FirstOrDefaultAsync(cancellationToken);
            if (user == null)
            {
                return methodResult;
            }

            await _userManager.DeleteAsync(user);
            if (user.Student != null)
            {
                await _studentRepository.DeleteAsync(user.Student);
                await _studentRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
