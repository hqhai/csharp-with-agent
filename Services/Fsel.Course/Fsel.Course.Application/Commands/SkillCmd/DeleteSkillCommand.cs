// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.SkillCmd
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.Skills;
    using Fsel.Course.Domain.Models.EntityModels.SkillModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class DeleteSkillCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class DeleteSkillCommandHandler : IRequestHandler<DeleteSkillCommand, MethodResult<bool>>
    {
        private readonly ISkillRepository _skillRepository;
        private readonly ISkillLevelRepository _skillLevelRepository;

        public DeleteSkillCommandHandler(ISkillRepository skillRepository,
            ISkillLevelRepository skillLevelRepository)
        {
            _skillRepository = skillRepository;
            _skillLevelRepository = skillLevelRepository;
        }

        public async Task<MethodResult<bool>> Handle(DeleteSkillCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var skill = await _skillRepository.GetByIdAsync(request.Id);
            if (skill == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Id), request.Id);
                return methodResult;
            }
            var isActive = await _skillLevelRepository.Queryable.AnyAsync(x => x.SkillId == request.Id, cancellationToken);
            if (isActive)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSkillErrorCode.SkillIsActive), nameof(isActive), isActive);
                return methodResult;
            }
            await _skillRepository.ExecuteTransactionAsync(async () =>
            {
                await _skillRepository.DeleteAsync(skill);
                await _skillRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });
            return methodResult;
        }
    }
}
