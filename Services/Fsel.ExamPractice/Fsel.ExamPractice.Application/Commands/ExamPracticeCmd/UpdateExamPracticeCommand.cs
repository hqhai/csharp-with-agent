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
        private readonly IExamPracticeRepository _examPracticeRepository;
        private readonly IMapper _mapper;
        private readonly ExamPracticeHelper _examPracticeHelper;
        private readonly IExamPracticeResultRepository _examPracticeResultRepository;
        private readonly ISystemService _systemService;

        public UpdateExamPracticeCommandHandler(IExamPracticeRepository examPracticeRepository,
            IMapper mapper,
            ExamPracticeHelper examPracticeHelper,
            IExamPracticeResultRepository examPracticeResultRepository,
            ISystemService systemService)
        {
            _examPracticeRepository = examPracticeRepository;
            _mapper = mapper;
            _examPracticeHelper = examPracticeHelper;
            _examPracticeResultRepository = examPracticeResultRepository;
            _systemService = systemService;
        }

        public async Task<MethodResult<ExamPracticeModel>> Handle(UpdateExamPracticeCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ExamPracticeModel>();
            var examPractice = await _examPracticeRepository.GetByIdAsync(request.Id);
            if (examPractice == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(examPractice), request.Id);
                return methodResult;
            }
            if (examPractice.Status == EnumExamPracticeStatus.Cloned)
            {
                methodResult.AddErrorBadRequest(nameof(EnumExamPracticeErrorCode.LockedClonedStatus), nameof(examPractice.Status), examPractice.Status);
                return methodResult;
            }
            if (string.IsNullOrEmpty(request.Code))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Code), request.Code);
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
            var existCode = await _examPracticeRepository.Queryable.AnyAsync(x => x.Code == request.Code && x.Id != examPractice.Id, cancellationToken);
            if (existCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(request.Code), request.Code);
                return methodResult;
            }
            if (request.Type == EnumExamPracticeType.IELTS && !new[] { EnumExamPracticeSubType.FullMockTest, EnumExamPracticeSubType.SkillMockTest }.Any(x => x == request.SubType))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(request.Type), request.SubType);
                return methodResult;
            }
            if (request.Type == EnumExamPracticeType.ExamPractice && !new[] { EnumExamPracticeSubType.Practice, EnumExamPracticeSubType.UniversityEntrance, EnumExamPracticeSubType.HighschoolEntrance }.Any(x => x == request.SubType))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(request.Type), request.SubType);
                return methodResult;
            }
            if (examPractice.Status == EnumExamPracticeStatus.Active && request.Type == EnumExamPracticeType.ExamPractice)
            {
                if (!request.StartDate.HasValue)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.StartDate), request.StartDate);
                    return methodResult;
                }
                if (!request.EndDate.HasValue)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.EndDate), request.EndDate);
                    return methodResult;
                }

                if (request.StartDate >= request.EndDate)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(request.StartDate), nameof(request.EndDate));
                    return methodResult;
                }
                if (new[] { EnumExamPracticeSubType.Practice, EnumExamPracticeSubType.HighschoolEntrance }.Any(x => x == request.SubType) && !request.ProvinceId.HasValue)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(request.ProvinceId), request.SubType);
                    return methodResult;
                }
                if (request.SubType == EnumExamPracticeSubType.Practice && string.IsNullOrEmpty(request.SchoolGrade))
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(request.SchoolGrade), request.SubType);
                    return methodResult;
                }
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
            }

            var existResult = await _examPracticeResultRepository.Queryable.AnyAsync(x => x.ExamPracticeId == request.Id, cancellationToken);
            if (existResult && await _examPracticeHelper.IsChangeValueActive(examPractice, request))
            {
                var examPracticeSectionCurrents = _examPracticeHelper.GetLeafSections(request.ExamPracticeSections);
                if (!_examPracticeHelper.AllSectionsHaveAtLeastOneQuestion(examPracticeSectionCurrents))
                {
                    methodResult.AddErrorBadRequest(nameof(EnumExamPracticeErrorCode.MissingRequiredData), nameof(examPractice.Status), EnumExamPracticeStatus.Active);
                    return methodResult;
                }
                examPractice.Status = EnumExamPracticeStatus.Cloned;
            }
            else
            {
                _mapper.Map(request, examPractice);
                if (!examPractice.IsValid())
                {
                    methodResult.AddErrorBadRequest(examPractice.ErrorMessages);
                    return methodResult;
                }
                if (!request.IsDraft && request.Type == EnumExamPracticeType.ExamPractice)
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

                var examPracticeSections = new List<ExamPracticeSection>();
                var method = await _examPracticeHelper.MapSectionsRecursively(examPractice, request.ExamPracticeSections, examPracticeSections);
                if (!method.IsOK)
                {
                    methodResult.AddErrorBadRequest(method.ErrorMessages);
                    return methodResult;
                }

                await _examPracticeHelper.DeleteExamPracticeSectionsAsync(request);
                examPractice.ExamPracticeSections = examPracticeSections;
            }
            await _examPracticeRepository.ExecuteTransactionAsync(async () =>
            {
                _examPracticeRepository.Update(examPractice);
                await _examPracticeRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.Result = _mapper.Map<ExamPracticeModel>(examPractice);
                return methodResult;
            });

            await _examPracticeHelper.UpdateExamPracticeSectionScoreAsync(examPractice).ConfigureAwait(false);
            return methodResult;
        }
    }
}
