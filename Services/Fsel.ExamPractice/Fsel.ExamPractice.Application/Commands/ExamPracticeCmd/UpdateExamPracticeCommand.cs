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
    public class UpdateExamPracticeCommand : UpdateExamPracticeCommandModel, IRequest<MethodResult<ExamPracticeModel>>
    {
    }

    public class UpdateExamPracticeCommandHandler : IRequestHandler<UpdateExamPracticeCommand, MethodResult<ExamPracticeModel>>
    {
        private readonly IExamPracticeRepository _repo;
        private readonly IMapper _mapper;
        private readonly ExamPracticeHelper _helper;
        private readonly IExamPracticeResultRepository _resultRepo;
        private readonly ISystemService _systemService;

        public UpdateExamPracticeCommandHandler(
            IExamPracticeRepository examPracticeRepository,
            IMapper mapper,
            ExamPracticeHelper examPracticeHelper,
            IExamPracticeResultRepository examPracticeResultRepository,
            ISystemService systemService)
        {
            _repo = examPracticeRepository;
            _mapper = mapper;
            _helper = examPracticeHelper;
            _resultRepo = examPracticeResultRepository;
            _systemService = systemService;
        }

        public async Task<MethodResult<ExamPracticeModel>> Handle(UpdateExamPracticeCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var result = new MethodResult<ExamPracticeModel>();

            // 1) Load + guard status
            var entity = await _repo.GetByIdAsync(request.Id);
            if (!EnsureEntityCanBeUpdated(entity, request, result))
            {
                return result;
            }
            // 2) Validate code basic + unique (exclude current)
            if (!await ValidateCodeAsync(request, entity!, result, cancellationToken))
            {
                return result;
            }
            // 3) Validate type/subtype
            if (!ValidateTypeAndSubType(request, result))
            {
                return result;
            }
            // 4) Resolve province name (nếu có)
            if (!await TryResolveProvinceAsync(request, entity!, result))
            {
                return result;
            }
            // 5) Validate business rules for ExamPractice (gom toàn bộ rule trùng vào 1 chỗ)
            if (!ValidateExamPracticeRules(request, entity!, result))
            {
                return result;
            }
            // 6) Check clone logic (has result + change active fields)
            var shouldClone = await ShouldCloneAsync(entity!, request, cancellationToken);
            if (shouldClone)
            {
                // Khi clone: chỉ validate leaf sections đủ question rồi set status cloned
                if (!EnsureAllLeafSectionsHaveQuestion(request, result))
                {
                    return result;
                }

                entity!.Status = EnumExamPracticeStatus.Cloned;
            }
            else
            {
                // 7) Apply update to entity + validate entity
                ApplyRequestToEntity(request, entity!);
                if (!ValidateEntity(entity!, result))
                {
                    return result;
                }

                // 8) Publish rule (không draft) cho ExamPractice
                if (!ApplyPublishRuleIfNeeded(request, entity!, result))
                {
                    return result;
                }

                // 9) Map sections + delete old + assign new
                if (!await ReplaceSectionsAsync(entity!, request, result))
                {
                    return result;
                }
            }

            // 10) Persist
            await PersistAsync(entity!, result, cancellationToken);

            // 11) Post: update score
            if (result.Result != null)
            {
                await _helper.UpdateExamPracticeSectionScoreAsync(entity!).ConfigureAwait(false);
            }
            return result;
        }

        // -------------------- Guards & Validations --------------------

        private static bool EnsureEntityCanBeUpdated(Domain.Entities.ExamPractice? entity, UpdateExamPracticeCommand request, MethodResult<ExamPracticeModel> result)
        {
            if (entity == null)
            {
                result.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(entity), request.Id);
                return false;
            }

            if (entity.Status == EnumExamPracticeStatus.Cloned)
            {
                result.AddErrorBadRequest(nameof(EnumExamPracticeErrorCode.LockedClonedStatus), nameof(entity.Status), entity.Status);
                return false;
            }

            if (string.IsNullOrWhiteSpace(request.Code))
            {
                result.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Code), request.Code);
                return false;
            }

            return true;
        }

        private async Task<bool> ValidateCodeAsync(UpdateExamPracticeCommand request, Domain.Entities.ExamPractice entity, MethodResult<ExamPracticeModel> result, CancellationToken ct)
        {
            var existCode = await _repo.Queryable.AnyAsync(x => x.Code == request.Code && x.Id != entity.Id, ct);
            if (existCode)
            {
                result.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(request.Code), request.Code);
                return false;
            }
            return true;
        }

        private static bool ValidateTypeAndSubType(UpdateExamPracticeCommand request, MethodResult<ExamPracticeModel> result)
        {
            // Nếu bạn đã có request.Type.GetSubTypes() như Create thì dùng giống Create là đẹp nhất.
            // Ở đây giữ đúng logic gốc (IELTS/ExamPractice list cụ thể) để không đổi behavior.
            if (request.Type == EnumExamPracticeType.IELTS &&
                !new[] { EnumExamPracticeSubType.FullMockTest, EnumExamPracticeSubType.SkillMockTest }.Contains(request.SubType))
            {
                result.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(request.Type), request.SubType);
                return false;
            }

            if (request.Type == EnumExamPracticeType.ExamPractice &&
                !new[] { EnumExamPracticeSubType.Practice, EnumExamPracticeSubType.UniversityEntrance, EnumExamPracticeSubType.HighschoolEntrance }.Contains(request.SubType))
            {
                result.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(request.Type), request.SubType);
                return false;
            }

            return true;
        }

        private async Task<bool> TryResolveProvinceAsync(UpdateExamPracticeCommand request, Domain.Entities.ExamPractice entity, MethodResult<ExamPracticeModel> result)
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

        private static bool ValidateExamPracticeRules(UpdateExamPracticeCommand request, Domain.Entities.ExamPractice entity, MethodResult<ExamPracticeModel> result)
        {
            if (request.Type != EnumExamPracticeType.ExamPractice)
            {
                return true;
            }

            // rule chung: nếu có đủ start/end thì start < end
            if (request.StartDate.HasValue && request.EndDate.HasValue && request.StartDate >= request.EndDate)
            {
                result.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(request.StartDate), nameof(request.EndDate));
                return false;
            }

            // rule: Province chỉ hợp lệ với Practice/HighschoolEntrance
            if (request.ProvinceId.HasValue &&
                request.SubType is not (EnumExamPracticeSubType.Practice or EnumExamPracticeSubType.HighschoolEntrance))
            {
                result.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(request.ProvinceId), request.SubType);
                return false;
            }

            // rule: không phải Practice thì SchoolGrade phải rỗng
            if (request.SubType != EnumExamPracticeSubType.Practice && !string.IsNullOrEmpty(request.SchoolGrade))
            {
                result.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(request.SchoolGrade), request.SubType);
                return false;
            }

            // rule riêng khi entity đang Active (giữ đúng logic gốc)
            if (entity.Status == EnumExamPracticeStatus.Active)
            {
                if (!request.StartDate.HasValue)
                {
                    result.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.StartDate), request.StartDate);
                    return false;
                }

                if (!request.EndDate.HasValue)
                {
                    result.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.EndDate), request.EndDate);
                    return false;
                }

                // Khi Active: Practice/HighschoolEntrance thì bắt buộc có ProvinceId (logic gốc)
                if (request.SubType is EnumExamPracticeSubType.Practice or EnumExamPracticeSubType.HighschoolEntrance)
                {
                    if (!request.ProvinceId.HasValue)
                    {
                        result.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(request.ProvinceId), request.SubType);
                        return false;
                    }
                }

                // Khi Active: Practice phải có SchoolGrade (logic gốc)
                if (request.SubType == EnumExamPracticeSubType.Practice && string.IsNullOrEmpty(request.SchoolGrade))
                {
                    result.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(request.SchoolGrade), request.SubType);
                    return false;
                }
            }

            return true;
        }

        private async Task<bool> ShouldCloneAsync(Domain.Entities.ExamPractice entity, UpdateExamPracticeCommand request, CancellationToken ct)
        {
            var existResult = await _resultRepo.Queryable.AnyAsync(x => x.ExamPracticeId == request.Id, ct);
            if (!existResult)
            {
                return false;
            }

            return await _helper.IsChangeValueActive(entity, request);
        }

        private bool EnsureAllLeafSectionsHaveQuestion(UpdateExamPracticeCommand request, MethodResult<ExamPracticeModel> result)
        {
            var leafSections = _helper.GetLeafSections(request.ExamPracticeSections);
            if (_helper.AllSectionsHaveAtLeastOneQuestion(leafSections))
            {
                return true;
            }

            result.AddErrorBadRequest(nameof(EnumExamPracticeErrorCode.MissingRequiredData),
                nameof(Domain.Entities.ExamPractice.Status),
                EnumExamPracticeStatus.Active);
            return false;
        }

        private void ApplyRequestToEntity(UpdateExamPracticeCommand request, Domain.Entities.ExamPractice entity)
        {
            // Province đã resolve vào entity ở trên (nếu có)
            _mapper.Map(request, entity);
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

        private bool ApplyPublishRuleIfNeeded(UpdateExamPracticeCommand request, Domain.Entities.ExamPractice entity, MethodResult<ExamPracticeModel> result)
        {
            if (request.IsDraft || request.Type != EnumExamPracticeType.ExamPractice)
            {
                return true;
            }

            if (!EnsureAllLeafSectionsHaveQuestion(request, result))
            {
                return false;
            }

            if (entity.Status == EnumExamPracticeStatus.Draft)
            {
                entity.Status = EnumExamPracticeStatus.Active;
                entity.ActivatedAt = DateTime.UtcNow;
            }

            return true;
        }

        // -------------------- Sections Replace --------------------

        private async Task<bool> ReplaceSectionsAsync(
            Domain.Entities.ExamPractice entity,
            UpdateExamPracticeCommand request,
            MethodResult<ExamPracticeModel> result)
        {
            var newSections = new List<ExamPracticeSection>();

            var mapResult = await _helper.MapSectionsRecursively(entity, request.ExamPracticeSections, newSections);
            if (!mapResult.IsOK)
            {
                result.AddErrorBadRequest(mapResult.ErrorMessages);
                return false;
            }

            await _helper.DeleteExamPracticeSectionsAsync(request);
            entity.ExamPracticeSections = newSections;
            return true;
        }

        // -------------------- Persist --------------------

        private async Task PersistAsync(Domain.Entities.ExamPractice entity, MethodResult<ExamPracticeModel> result, CancellationToken ct)
        {
            await _repo.ExecuteTransactionAsync(async () =>
            {
                _repo.Update(entity);
                await _repo.UnitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);
                result.Result = _mapper.Map<ExamPracticeModel>(entity);
                return result;
            });
        }
    }
}
