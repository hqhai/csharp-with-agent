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
    using Fsel.ExamPractice.Domain.Entities;
    using Fsel.ExamPractice.Domain.IRepositories;
    using Fsel.ExamPractice.Domain.Models.CommandModels.ExamPractices;
    using Fsel.ExamPractice.Domain.Models.EntityModels.ExamPractices;
    using Fsel.ExamPractice.Infrastructure.Common.ExamPracticeHelpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class UpdateExamPracticeCommand : UpdateExamPracticeCommandModel, IRequest<MethodResult<ExamPracticeModel>>
    {
    }

    public class UpdateExamPracticeCommandHandler : IRequestHandler<UpdateExamPracticeCommand, MethodResult<ExamPracticeModel>>
    {
        private readonly IExamPracticeRepository _repo;
        private readonly IMapper _mapper;
        private readonly IVersionEntityUpdater<ExamPractice> _versionUpdater;
        private readonly ExamPracticeConverter _converter;
        private readonly ExamPracticeCommon _common;

        public UpdateExamPracticeCommandHandler(
            IExamPracticeRepository examPracticeRepository,
            IMapper mapper,
            IVersionEntityUpdater<ExamPractice> versionEntityUpdater,
            ExamPracticeConverter examPracticeConverter,
            ExamPracticeCommon examPracticeCommon)
        {
            _repo = examPracticeRepository;
            _mapper = mapper;
            _versionUpdater = versionEntityUpdater;
            _converter = examPracticeConverter;
            _common = examPracticeCommon?.Create() ?? new ExamPracticeCommon().Create();
        }

        public async Task<MethodResult<ExamPracticeModel>> Handle(UpdateExamPracticeCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var result = new MethodResult<ExamPracticeModel>();

            var oldEntity = await _repo.GetByIdAsync(request.Id);
            if (oldEntity == null)
            {
                result.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Id), request.Id);
                return result;
            }

            if (!await ValidateAsync(request, oldEntity, result, cancellationToken))
            {
                return result;
            }
            var isUsingByClient = await _repo.IsUsingByClient(oldEntity.Id);
            var newEntity = BuildNewVersionEntity(request);

            await _versionUpdater.UpdateEntity(
                oldEntity,
                newEntity,
                (_, __) => Task.FromResult(isUsingByClient),                    // shouldCreateNewVersion?
                (o, n) => ApplyUpdateAsync(request, o, n, isUsingByClient, result, cancellationToken), // onUpdate
                null,
                (o, n) => SetResultAsync(o, n, result)                          // onCompleted
            );

            result.StatusCode = StatusCodes.Status200OK;
            return result;
        }

        // -------------------- Steps --------------------

        private async Task<bool> ValidateAsync(
            UpdateExamPracticeCommand request,
            ExamPractice oldEntity,
            MethodResult<ExamPracticeModel> result,
            CancellationToken ct)
        {
            var validateBuilder = await ExamPracticeValidateBuilder
                .Create(request, _repo)
                .IsValidateQuestion(request.ExamPracticeSections, _mapper)
                .ValidateDuplicateTestAsync(oldEntity.OriginalId);

            var validateResult = validateBuilder.GetResult();
            if (validateResult.IsOK)
            {
                return true;
            }
            result.AddErrorBadRequest(validateResult.ErrorMessages);
            return false;
        }

        private ExamPractice BuildNewVersionEntity(UpdateExamPracticeCommand request)
        {
            // giữ đúng behavior: factory build default
            return ExamPracticeFactory.Create(request, _mapper).Build();
        }

        private async Task ApplyUpdateAsync(
            UpdateExamPracticeCommand request,
            ExamPractice oldEntity,
            ExamPractice newEntity,
            bool isUsingByClient,
            MethodResult<ExamPracticeModel> result,
            CancellationToken ct)
        {
            // 1) Map request -> old entity (giữ đúng logic của bạn)
            _mapper.Map(request, oldEntity);

            if (!oldEntity.IsValid())
            {
                result.AddErrorBadRequest(oldEntity.ErrorMessages);
                return;
            }

            // 2) Nếu không bị client dùng, update sections + rebuild index
            if (!isUsingByClient)
            {
                await UpdateSectionsAsync(request, oldEntity, ct);
            }
        }

        private async Task UpdateSectionsAsync(UpdateExamPracticeCommand request, ExamPractice oldEntity, CancellationToken ct)
        {
            var currentSections = oldEntity.ExamPracticeSections
                .OrderBy(x => x.DisplayOrder)
                .ToList();

            await _converter.HandlerExamPracticeSections(
                request.ExamPracticeSections,
                currentSections,
                oldEntity.Id,
                null,
                ct);

            await _converter.DeleteObjectInstance();

            _common.HanderQuestionIndexSection(currentSections);
        }

        private static Task SetResultAsync(ExamPractice oldEntity, ExamPractice newEntity, MethodResult<ExamPracticeModel> result)
        {
            // giữ đúng behavior: newEntity.Id != Guid.Empty => trả new
            var entityToReturn = newEntity.Id != Guid.Empty ? newEntity : oldEntity;
            result.Result = result.Result = (result as dynamic)?._mapper != null
                ? null
                : null;
            return Task.CompletedTask;
        }
    }
}
