// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Application.Commands.ExamPracticeCmd.V1i1
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
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
        private readonly IExamPracticeRepository _examPracticeRepository;
        private readonly IMapper _mapper;
        private readonly IVersionEntityUpdater<ExamPractice> _versionEntityUpdater;
        private readonly ExamPracticeConverter _examPracticeConverter;

        public UpdateExamPracticeCommandHandler(IExamPracticeRepository examPracticeRepository,
                                                IMapper mapper,
                                                IVersionEntityUpdater<ExamPractice> versionEntityUpdater,
                                                ExamPracticeConverter examPracticeConverter)
        {
            _examPracticeRepository = examPracticeRepository;
            _mapper = mapper;
            _versionEntityUpdater = versionEntityUpdater;
            _examPracticeConverter = examPracticeConverter;
        }

        public async Task<MethodResult<ExamPracticeModel>> Handle(UpdateExamPracticeCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ExamPracticeModel>();

            var examPractice = await _examPracticeRepository.GetByIdAsync(request.Id);
            if (examPractice == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Id), request.Id);
                return methodResult;
            }

            var validate = await ExamPracticeValidateBuilder.Create(request, _examPracticeRepository).ValidateRequestData().ValidateDuplicateTestAsync(examPractice.OriginalId);
            var validateResult = validate.GetResult();
            if (!validateResult.IsOK)
            {
                methodResult.AddErrorBadRequest(validateResult.ErrorMessages);
                return methodResult;
            }

            var isUsingByClient = await _examPracticeRepository.IsUsingByClient(examPractice.Id);

            var newVersionExamPractice = ExamPracticeFactory.Create(request, _mapper).Build();

            await _versionEntityUpdater.UpdateEntity(examPractice, newVersionExamPractice,
                async (_, entity) => isUsingByClient,
                async (oldEntity, newEntity) =>
                {
                    _mapper.Map(request, examPractice);
                    if (!examPractice.IsValid())
                    {
                        methodResult.AddErrorBadRequest(examPractice.ErrorMessages);
                    }

                    if (!isUsingByClient)
                    {
                        await _examPracticeConverter.HandlerChildents(request.ExamPracticeSections, oldEntity.ExamPracticeSections, oldEntity.Id, null, cancellationToken);
                        await _examPracticeConverter.DeleteObjectInstance();
                    }

                    await Task.Yield();
                },
                null,
                async (oldEntity, newEntity) =>
                {
                    methodResult.Result = newEntity.Id != Guid.Empty ? _mapper.Map<ExamPracticeModel>(newEntity) : _mapper.Map<ExamPracticeModel>(oldEntity);
                    await Task.CompletedTask;
                }
            );

            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
