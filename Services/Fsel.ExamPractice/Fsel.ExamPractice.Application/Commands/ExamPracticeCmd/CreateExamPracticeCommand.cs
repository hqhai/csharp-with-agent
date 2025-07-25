// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.ExamPractice.Application.Services.SystemServices;
using Fsel.ExamPractice.Application.Services.SystemServices.QueryModels;
using Fsel.ExamPractice.Domain.Entities;
using Fsel.ExamPractice.Domain.Enums;
using Fsel.ExamPractice.Domain.Enums.ErrorCodes;
using Fsel.ExamPractice.Domain.IRepositories;
using Fsel.ExamPractice.Domain.Models.CommandModels.ExamPractices;
using Fsel.ExamPractice.Domain.Models.EntityModels.ExamPractices;
using Fsel.ExamPractice.Infrastructure.Common;
using Fsel.Shared.Enums.ErrorCodes;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fsel.ExamPractice.Application.Commands.ExamPracticeCmd
{
    public class CreateExamPracticeCommand : CreateExamPracticeCommandModel, IRequest<MethodResult<ExamPracticeModel>>
    {
    }

    public class CreateExamPracticeCommandHandler : IRequestHandler<CreateExamPracticeCommand, MethodResult<ExamPracticeModel>>
    {
        private readonly IExamPracticeRepository _examPracticeRepository;
        private readonly IMapper _mapper;
        private readonly ExamPracticeHelper _examPracticeHelper;
        private readonly ISystemService _systemService;

        public CreateExamPracticeCommandHandler(
            IExamPracticeRepository examPracticeRepository,
            IMapper mapper,
            ExamPracticeHelper examPracticeHelper,
            ISystemService systemService)
        {
            _examPracticeRepository = examPracticeRepository;
            _mapper = mapper;
            _examPracticeHelper = examPracticeHelper;
            _systemService = systemService;
        }

        public async Task<MethodResult<ExamPracticeModel>> Handle(CreateExamPracticeCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ExamPracticeModel>();
            if (string.IsNullOrEmpty(request.Code))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Code), request.Code);
                return methodResult;
            }
            var existCode = await _examPracticeRepository.Queryable.AnyAsync(x => x.Code == request.Code, cancellationToken);
            if (existCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(request.Code), request.Code);
                return methodResult;
            }
            if (request.Type == EnumExamPracticeType.IELTS && !request.Type.GetSubTypes().Any(x => x == request.SubType))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(request.Type), request.SubType);
                return methodResult;
            }
            if (request.Type == EnumExamPracticeType.ExamPractice && !request.Type.GetSubTypes().Any(x => x == request.SubType))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(request.Type), request.SubType);
                return methodResult;
            }

            var examPractice = _mapper.Map<Domain.Entities.ExamPractice>(request);
            if (!examPractice.IsValid())
            {
                methodResult.AddErrorBadRequest(examPractice.ErrorMessages);
                return methodResult;
            }
            if (request.ProvinceId.HasValue)
            {
                var locationResults = await _systemService.GetLocationByIdsAsync(new GetLocationsByIdsQueryModel { IdsStr = request.ProvinceId.Value.ToString() });
                if (!locationResults.IsSuccessStatusCode)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallSystemServiceError));
                    return methodResult;
                }
                var locations = locationResults.Content?.Result;
                if (locations == null || !locations.Any())
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(locations), request.ProvinceId);
                    return methodResult;
                }
                examPractice.Province = locations.FirstOrDefault()?.Name;
            }
            if (request.Type == EnumExamPracticeType.ExamPractice)
            {
                if (request.StartDate.HasValue && request.EndDate.HasValue && request.StartDate >= request.EndDate)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(request.StartDate), nameof(request.EndDate));
                    return methodResult;
                }

                if (request.ProvinceId.HasValue && !new[] { EnumExamPracticeSubType.Practice, EnumExamPracticeSubType.HighschoolEntrance }.Any(x => x == request.SubType))
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(request.ProvinceId), request.SubType);
                    return methodResult;
                }

                if (request.SubType != EnumExamPracticeSubType.Practice && !string.IsNullOrEmpty(request.SchoolGrade))
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(request.SchoolGrade), request.SubType);
                    return methodResult;
                }

                if (!request.IsDraft)
                {
                    var examPracticeSectionCurrents = _examPracticeHelper.GetLeafSections(request.ExamPracticeSections);
                    if (!_examPracticeHelper.AllSectionsHaveAtLeastOneQuestion(examPracticeSectionCurrents))
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumExamPracticeErrorCode.MissingRequiredData), nameof(examPractice.Status), EnumExamPracticeStatus.Active);
                        return methodResult;
                    }
                    if (examPractice.Status == EnumExamPracticeStatus.Draft)
                    {
                        examPractice.Status = EnumExamPracticeStatus.Active;
                        examPractice.ActivatedAt = DateTime.UtcNow;
                    }
                }
            }

            var examPracticeSections = new List<ExamPracticeSection>();
            var voidMethod = _examPracticeHelper.MapSectionsRecursively(request.ExamPracticeSections, examPracticeSections);
            if (!voidMethod.IsOK)
            {
                methodResult.AddErrorBadRequest(voidMethod.ErrorMessages);
                return methodResult;
            }

            examPractice.ExamPracticeSections = examPracticeSections;
            await _examPracticeRepository.ExecuteTransactionAsync(async () =>
            {
                _examPracticeRepository.Add(examPractice);
                await _examPracticeRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.Result = _mapper.Map<ExamPracticeModel>(examPractice);
                return methodResult;
            });
            await _examPracticeHelper.UpdateExamPracticeSectionScoreAsync(examPractice).ConfigureAwait(false);
            return methodResult;
        }
    }
}
