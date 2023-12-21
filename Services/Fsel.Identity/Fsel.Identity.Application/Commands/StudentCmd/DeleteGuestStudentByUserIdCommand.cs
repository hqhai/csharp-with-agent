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
        private readonly IHumanRepository _humanRepository;

        public DeleteGuestStudentByUserIdCommandHandler(UserManager<User> userManager, IHumanRepository humanRepository)
        {
            _userManager = userManager;
            _humanRepository = humanRepository;
        }

        public async Task<VoidMethodResult> Handle(DeleteGuestStudentByUserIdCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            VoidMethodResult methodResult = new VoidMethodResult();

            var user = await _userManager.Users.Include(x => x.Human).ThenInclude(x => x.Student).Where(x => x.Id == request.Id).FirstOrDefaultAsync(cancellationToken);

            await _userManager.DeleteAsync(user!);

            await _humanRepository.DeleteAsync(user?.Human ?? new Human());
            await _humanRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
