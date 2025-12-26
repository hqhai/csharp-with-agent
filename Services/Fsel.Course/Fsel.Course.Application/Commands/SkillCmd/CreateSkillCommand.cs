// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.SkillCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.Skills;
    using Fsel.Course.Domain.Models.EntityModels.SkillModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class CreateSkillCommand : CreateSkillCommandModel, IRequest<MethodResult<SkillModel>>
    {
    }

    public class CreateSkillCommandHandler : IRequestHandler<CreateSkillCommand, MethodResult<SkillModel>>
    {
        private readonly ISkillRepository _skillRepository;
        private readonly IMapper _mapper;

        public CreateSkillCommandHandler(ISkillRepository skillRepository,
            IMapper mapper)
        {
            _skillRepository = skillRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<SkillModel>> Handle(CreateSkillCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<SkillModel> methodResult = new MethodResult<SkillModel>();

            var isDuplicateCode = await _skillRepository.IsDuplicateFieldValueAsync(nameof(request.Code), request.Code);

            if (isDuplicateCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(request.Code), request.Code);
            }

            if (!methodResult.IsOK)
            {
                return methodResult;
            }

            var skill = _mapper.Map<Skill>(request);
            if (!skill.IsValid())
            {
                methodResult.AddErrorBadRequest(skill.ErrorMessages);
                return methodResult;
            }

            await _skillRepository.ExecuteTransactionAsync(async () =>
            {
                skill = _skillRepository.Add(skill);
                await _skillRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<SkillModel>(skill);
                return methodResult;
            });
            return methodResult;
        }
    }
}
