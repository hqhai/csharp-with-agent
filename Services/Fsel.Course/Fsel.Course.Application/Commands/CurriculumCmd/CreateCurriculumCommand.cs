// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.CurriculumCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.Curriculums;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateCurriculumCommand : CreateCurriculumCommandModel, IRequest<MethodResult<CurriculumModel>>
    {
    }

    public class CreateCurriculumCommandHandler : IRequestHandler<CreateCurriculumCommand, MethodResult<CurriculumModel>>
    {
        private readonly ICurriculumRepository _curriculumRepository;
        private readonly IMapper _mapper;

        public CreateCurriculumCommandHandler(ICurriculumRepository curriculumRepository, IMapper mapper)
        {
            _curriculumRepository = curriculumRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<CurriculumModel>> Handle(CreateCurriculumCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<CurriculumModel> methodResult = new MethodResult<CurriculumModel>();

            #region validation
            if (string.IsNullOrWhiteSpace(request.CurriculumName))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required), nameof(request.CurriculumName), request.CurriculumName);
                return methodResult;
            }

            var existingCurriculum = await _curriculumRepository.Queryable.FirstOrDefaultAsync(c => c.CurriculumName == request.CurriculumName, cancellationToken);
            if (existingCurriculum != null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCurriculumErrorCode.AlreadyExistCurriculumName), nameof(request.CurriculumName), request.CurriculumName);
                return methodResult;
            }
            #endregion

            var curriculums = _mapper.Map<CurriculumConfig>(request);
            curriculums.CurriculumStatus = EnumCurriculumStatus.NotProgress;

            await _curriculumRepository.ExecuteTransactionAsync(async () =>
            {
                _curriculumRepository.Add(curriculums);
                await _curriculumRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<CurriculumModel>(curriculums);
                return methodResult;
            });

            return methodResult;
        }
    }

}
