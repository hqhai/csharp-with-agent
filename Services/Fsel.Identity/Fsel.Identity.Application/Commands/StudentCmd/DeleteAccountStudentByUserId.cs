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

    public class DeleteAccountStudentByUserId : IRequest<MethodResult<bool>>
    {
        public Guid UserId { get; set; }
    }

    public class DeleteAccountStudentByUserIdHandler : IRequestHandler<DeleteAccountStudentByUserId, MethodResult<bool>>
    {
        private readonly IHumanRepository _humanRepository;
        private readonly UserManager<User> _userManager;
        public DeleteAccountStudentByUserIdHandler(IHumanRepository humanRepository
                                                 , UserManager<User> userManager)
        {
            _humanRepository = humanRepository;
            _userManager = userManager;
        }
        public async Task<MethodResult<bool>> Handle(DeleteAccountStudentByUserId request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();
            var human = await _humanRepository.Queryable
                                              .Where(x => x.UserId == request.UserId)
                                              .FirstOrDefaultAsync(cancellationToken);

            if (human != null)
            {
                // delete human
                await _humanRepository.DeleteAsync(human);
                await _humanRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
            }

            // delete user
            var user = await _userManager.Users.Where(x => x.Id == request.UserId).FirstOrDefaultAsync(cancellationToken);
            await _userManager.DeleteAsync(user!);


            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = true;
            return methodResult;
        }
    }
}
