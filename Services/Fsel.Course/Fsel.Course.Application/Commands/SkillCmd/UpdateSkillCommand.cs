// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.SkillCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.Skills;
    using Fsel.Course.Domain.Models.EntityModels.SkillModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class UpdateSkillCommand : UpdateSkillCommandModel, IRequest<MethodResult<SkillModel>>
    {
    }

    public class UpdateSkillCommandHandler : IRequestHandler<UpdateSkillCommand, MethodResult<SkillModel>>
    {
        private readonly ISkillRepository _skillRepository;
        private readonly IMapper _mapper;

        public UpdateSkillCommandHandler(ISkillRepository skillRepository,
            IMapper mapper)
        {
            _skillRepository = skillRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<SkillModel>> Handle(UpdateSkillCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<SkillModel> methodResult = new MethodResult<SkillModel>();

            var skill = await _skillRepository.GetByIdAsync(request.Id);
            if (skill == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Id), request.Id);
                return methodResult;
            }

            var isDuplicateCode = await _skillRepository.IsDuplicateFieldValueAsync(skill.Id, nameof(request.Code), request.Code);
            var isDuplicateName = await _skillRepository.IsDuplicateFieldValueAsync(skill.Id, nameof(request.Name), request.Name);

            if (isDuplicateCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(request.Code), request.Code);
            }
            if (isDuplicateName)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(request.Name), request.Name);
            }
            if (!methodResult.IsOK)
            {
                return methodResult;
            }

            _mapper.Map(request, skill);
            if (!skill.IsValid())
            {
                methodResult.AddErrorBadRequest(skill.ErrorMessages);
                return methodResult;
            }

            await _skillRepository.ExecuteTransactionAsync(async () =>
            {
                skill = _skillRepository.Update(skill);
                await _skillRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<SkillModel>(skill);
                return methodResult;
            });
            return methodResult;
        }
    }
}
