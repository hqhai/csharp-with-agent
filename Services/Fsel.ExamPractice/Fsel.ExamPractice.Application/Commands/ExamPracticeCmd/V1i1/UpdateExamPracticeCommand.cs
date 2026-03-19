// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Application.Commands.ExamPracticeCmd.V1i1
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.Interfaces;
    using Fsel.ExamPractice.Application.Services.SystemServices;
    using Fsel.ExamPractice.Application.Services.SystemServices.QueryModels;
    using Fsel.ExamPractice.Domain.Entities;
    using Fsel.ExamPractice.Domain.Enums;
    using Fsel.ExamPractice.Domain.IRepositories;
    using Fsel.ExamPractice.Domain.Models.CommandModels.ExamPractices;
    using Fsel.ExamPractice.Domain.Models.EntityModels.ExamPractices;
    using Fsel.ExamPractice.Infrastructure.Common;
    using Fsel.ExamPractice.Infrastructure.Common.ExamPracticeHelpers;
    using Fsel.ExamPractice.Infrastructure.Common.Processors;
    using Fsel.ExamPractice.Infrastructure.Common.Validators;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateExamPracticeCommand : UpdateExamPracticeCommandModel, IRequest<MethodResult<ExamPracticeModel>>
    { }

    public class UpdateExamPracticeCommandHandler : IRequestHandler<UpdateExamPracticeCommand, MethodResult<ExamPracticeModel>>
    {
        private readonly IExamPracticeRepository _repo;
        private readonly IExamPracticeResultRepository _resultRepo;
        private readonly IMapper _mapper;
        private readonly IVersionEntityUpdater<ExamPractice> _versionUpdater;
        private readonly ExamPracticeHelper _helper;
        private readonly ISystemService _systemService;

        private readonly Dictionary<EnumExamPracticeType, IUpdateExamPracticeValidator> _validators;
        private readonly Dictionary<EnumExamPracticeType, IUpdateExamPracticeProcessor> _processors;

        public UpdateExamPracticeCommandHandler(
            IExamPracticeRepository examPracticeRepository,
            IExamPracticeResultRepository examPracticeResultRepository,
            IMapper mapper,
            IVersionEntityUpdater<ExamPractice> versionEntityUpdater,
            ExamPracticeHelper examPracticeHelper,
            ISystemService systemService,
            IEnumerable<IUpdateExamPracticeValidator> validators,
            IEnumerable<IUpdateExamPracticeProcessor> processors)
        {
            _repo = examPracticeRepository;
            _resultRepo = examPracticeResultRepository;
            _mapper = mapper;
            _versionUpdater = versionEntityUpdater;
            _helper = examPracticeHelper;
            _systemService = systemService;

            _validators = validators.ToDictionary(v => v.ExamPracticeType);
            _processors = processors.ToDictionary(p => p.ExamPracticeType);
        }

        public async Task<MethodResult<ExamPracticeModel>> Handle(UpdateExamPracticeCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var result = new MethodResult<ExamPracticeModel>();

            // 1) Load entity with includes
            var oldEntity = await GetEntityWithIncludesAsync(request.Id, cancellationToken);
            if (oldEntity == null)
            {
                result.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Id), request.Id);
                return result;
            }

            // 2) Resolve external data (province name) BEFORE validation
            var validationContext = await ResolveValidationContextAsync(request, cancellationToken);

            // 3) Get type-specific validator
            if (!_validators.TryGetValue(request.Type, out var validator))
            {
                result.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(request.Type), request.Type);
                return result;
            }

            // 4) Validate using type-specific validator (pass resolved context, not service)
            var validateResult = await validator.ValidateAsync(request, oldEntity, _repo, _resultRepo, validationContext, _helper, cancellationToken);
            if (!validateResult.IsOK)
            {
                result.AddErrorBadRequest(validateResult.ErrorMessages);
                return result;
            }

            // 5) Get type-specific processor
            if (!_processors.TryGetValue(request.Type, out var processor))
            {
                result.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(request.Type), request.Type);
                return result;
            }

            // 6) Check clone logic
            var shouldClone = await CheckCloneAsync(oldEntity, request, cancellationToken);
            if (shouldClone)
            {
                var cloneValidation = processor.ValidateForClone(request, _helper);
                if (!cloneValidation.IsOK)
                {
                    result.AddErrorBadRequest(cloneValidation.ErrorMessages);
                    return result;
                }
                oldEntity.Status = EnumExamPracticeStatus.Cloned;
            }

            // 7) Execute update via type-specific processor
            var isUsedByClient = await _repo.IsUsingByClient(oldEntity.Id);
            var newEntity = ExamPracticeFactory.Create(request, _mapper).Build();

            await _versionUpdater.UpdateEntity(
                oldEntity,
                newEntity,
                (_, __) => Task.FromResult(isUsedByClient),
                (oldExamPractice, newExamPractice) =>
                    processor.ApplyUpdateAsync(request, oldExamPractice, newExamPractice, isUsedByClient, shouldClone, _mapper, _helper, result, cancellationToken),
                null,
                (oldExamPractice, newExamPractice) => SetResultAsync(oldExamPractice, newExamPractice, result, cancellationToken));

            result.StatusCode = StatusCodes.Status200OK;
            return result;
        }

        private async Task<ExamPracticeValidationContext> ResolveValidationContextAsync(UpdateExamPracticeCommand request, CancellationToken ct)
        {
            var context = new ExamPracticeValidationContext();

            // Resolve province name via SystemService
            if (request.ProvinceId.HasValue)
            {
                var locationResults = await _systemService.GetLocationByIdsAsync(
                    new GetLocationsByIdsQueryModel { IdsStr = request.ProvinceId.Value.ToString() });

                if (locationResults.IsSuccessStatusCode && locationResults.Content?.Result?.Any() == true)
                {
                    context.ProvinceName = locationResults.Content.Result.First().Name;
                }
            }

            return context;
        }

        private async Task<ExamPractice?> GetEntityWithIncludesAsync(Guid id, CancellationToken ct)
        {
            return await _repo.Queryable.FirstOrDefaultAsync(x => x.Id == id, ct);
        }

        private async Task<bool> CheckCloneAsync(ExamPractice entity, UpdateExamPracticeCommand request, CancellationToken ct)
        {
            var existResult = await _resultRepo.Queryable.AnyAsync(x => x.ExamPracticeId == request.Id, ct);
            if (!existResult)
                return false;
            return await _helper.IsChangeValueActive(entity, request);
        }

        private async Task SetResultAsync(ExamPractice oldEntity, ExamPractice newEntity, MethodResult<ExamPracticeModel> result, CancellationToken ct)
        {
            var entityToReturn = newEntity.Id != Guid.Empty ? newEntity : oldEntity;
            result.Result = _mapper.Map<ExamPracticeModel>(entityToReturn);
            if (result.Result != null)
            {
                await _helper.UpdateExamPracticeSectionScoreAsync(entityToReturn).ConfigureAwait(false);
            }
        }
    }
}
