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
        private readonly IExamPracticeRepository _repo;
        private readonly IMapper _mapper;
        private readonly ExamPracticeHelper _helper;
        private readonly ISystemService _systemService;

        public CreateExamPracticeCommandHandler(
            IExamPracticeRepository examPracticeRepository,
            IMapper mapper,
            ExamPracticeHelper examPracticeHelper,
            ISystemService systemService)
        {
            _repo = examPracticeRepository;
            _mapper = mapper;
            _helper = examPracticeHelper;
            _systemService = systemService;
        }

        public async Task<MethodResult<ExamPracticeModel>> Handle(CreateExamPracticeCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var result = new MethodResult<ExamPracticeModel>();

            // 1) Validate input cơ bản + unique code
            if (!await ValidateCodeAsync(request, result, cancellationToken))
            {
                return result;
            }
            // 2) Validate Type/SubType
            if (!ValidateTypeAndSubType(request, result))
            {
                return result;
            }
            // 3) Map entity + validate entity
            var entity = _mapper.Map<Domain.Entities.ExamPractice>(request);
            if (!ValidateEntity(entity, result))
            {
                return result;
            }
            // 4) Resolve province name (nếu có)
            if (!await TryResolveProvinceAsync(request, entity, result))
            {
                return result;
            }
            // 5) Validate theo từng type (ExamPractice)
            if (!ValidateExamPracticeBusinessRules(request, entity, result))
            {
                return result;
            }
            // 6) Map sections
            if (!TryMapSections(request, entity, result))
            {
                return result;
            }
            // 7) Persist
            await PersistAsync(entity, result, cancellationToken);

            // 8) Post processing (score)
            if (result.Result != null)
            {
                await _helper.UpdateExamPracticeSectionScoreAsync(entity).ConfigureAwait(false);
            }

            return result;
        }

        private async Task<bool> ValidateCodeAsync(CreateExamPracticeCommand request, MethodResult<ExamPracticeModel> result, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request.Code))
            {
                result.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Code), request.Code);
                return false;
            }

            var exist = await _repo.Queryable.AnyAsync(x => x.Code == request.Code, ct);
            if (exist)
            {
                result.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(request.Code), request.Code);
                return false;
            }

            return true;
        }

        private static bool ValidateTypeAndSubType(CreateExamPracticeCommand request, MethodResult<ExamPracticeModel> result)
        {
            // Nếu chỉ có 2 loại cần check subtype như nhau → gom 1 chỗ
            if (request.Type is EnumExamPracticeType.IELTS or EnumExamPracticeType.ExamPractice)
            {
                if (!request.Type.GetSubTypes().Contains(request.SubType))
                {
                    result.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(request.Type), request.SubType);
                    return false;
                }
            }

            return true;
        }

        private static bool ValidateEntity(Domain.Entities.ExamPractice entity, MethodResult<ExamPracticeModel> result)
        {
            if (entity.IsValid())
            {
                return true;
            }
            result.AddErrorBadRequest(entity.ErrorMessages);
            return false;
        }

        private async Task<bool> TryResolveProvinceAsync(
            CreateExamPracticeCommand request,
            Domain.Entities.ExamPractice entity,
            MethodResult<ExamPracticeModel> result)
        {
            if (!request.ProvinceId.HasValue)
            {
                return true;
            }

            var locationResults = await _systemService.GetLocationByIdsAsync(
                new GetLocationsByIdsQueryModel { IdsStr = request.ProvinceId.Value.ToString() });

            if (!locationResults.IsSuccessStatusCode)
            {
                result.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallSystemServiceError));
                return false;
            }

            var locations = locationResults.Content?.Result;
            if (locations == null || !locations.Any())
            {
                result.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(locations), request.ProvinceId);
                return false;
            }

            entity.Province = locations.First().Name;
            return true;
        }

        private bool ValidateExamPracticeBusinessRules(
            CreateExamPracticeCommand request,
            Domain.Entities.ExamPractice entity,
            MethodResult<ExamPracticeModel> result)
        {
            if (request.Type != EnumExamPracticeType.ExamPractice)
            {
                return true;
            }

            if (request.StartDate.HasValue && request.EndDate.HasValue && request.StartDate >= request.EndDate)
            {
                result.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(request.StartDate), nameof(request.EndDate));
                return false;
            }

            // Province chỉ hợp lệ với một số subtype
            if (request.ProvinceId.HasValue &&
                request.SubType is not (EnumExamPracticeSubType.Practice or EnumExamPracticeSubType.HighschoolEntrance))
            {
                result.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(request.ProvinceId), request.SubType);
                return false;
            }

            // Nếu không phải Practice thì SchoolGrade phải rỗng
            if (request.SubType != EnumExamPracticeSubType.Practice && !string.IsNullOrEmpty(request.SchoolGrade))
            {
                result.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(request.SchoolGrade), request.SubType);
                return false;
            }

            // Publish: cần đủ câu hỏi ở leaf sections, chuyển Draft->Active
            if (!request.IsDraft)
            {
                var leafSections = _helper.GetLeafSections(request.ExamPracticeSections);
                if (!_helper.AllSectionsHaveAtLeastOneQuestion(leafSections))
                {
                    result.AddErrorBadRequest(nameof(EnumExamPracticeErrorCode.MissingRequiredData),
                        nameof(entity.Status),
                        EnumExamPracticeStatus.Active);
                    return false;
                }

                if (entity.Status == EnumExamPracticeStatus.Draft)
                {
                    entity.Status = EnumExamPracticeStatus.Active;
                    entity.ActivatedAt = DateTime.UtcNow;
                }
            }

            return true;
        }

        private bool TryMapSections(
            CreateExamPracticeCommand request,
            Domain.Entities.ExamPractice entity,
            MethodResult<ExamPracticeModel> result)
        {
            var sections = new List<ExamPracticeSection>();
            var mapResult = _helper.MapSectionsRecursively(request.ExamPracticeSections, sections);

            if (!mapResult.IsOK)
            {
                result.AddErrorBadRequest(mapResult.ErrorMessages);
                return false;
            }

            entity.ExamPracticeSections = sections;
            return true;
        }

        private async Task PersistAsync(
            Domain.Entities.ExamPractice entity,
            MethodResult<ExamPracticeModel> result,
            CancellationToken ct)
        {
            await _repo.ExecuteTransactionAsync(async () =>
            {
                _repo.Add(entity);
                await _repo.UnitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

                result.Result = _mapper.Map<ExamPracticeModel>(entity);
                return result;
            });
        }
    }
}
