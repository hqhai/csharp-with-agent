// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.HomeWorkConfigCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.HomeWorkConfigs;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.CacheServices;
    using Fsel.Shared.ApplicationServices.CacheServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SaveHomeWorkConfigCommand : SaveHomeWorkConfigCommandModel, IRequest<MethodResult<HomeWorkConfigModel>>
    {
    }

    public class SaveHomeWorkConfigCommandHandler : IRequestHandler<SaveHomeWorkConfigCommand, MethodResult<HomeWorkConfigModel>>
    {
        private readonly IHomeWorkConfigRepository _homeWorkConfigRepository;
        private readonly IMapper _mapper;
        private readonly ICurriculumRepository _curriculumRepository;
        private readonly IHomeWorkRepository _homeWorkRepository;
        private readonly IRequestSafeCachingService _requestSafeCachingService;

        public SaveHomeWorkConfigCommandHandler(IHomeWorkConfigRepository homeWorkConfigRepository, IMapper mapper, ICurriculumRepository curriculumRepository, IHomeWorkRepository homeWorkRepository, IRequestSafeCachingService requestSafeCachingService)
        {
            _homeWorkConfigRepository = homeWorkConfigRepository;
            _mapper = mapper;
            _curriculumRepository = curriculumRepository;
            _homeWorkRepository = homeWorkRepository;
            _requestSafeCachingService = requestSafeCachingService;
        }

        public async Task<MethodResult<HomeWorkConfigModel>> Handle(SaveHomeWorkConfigCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<HomeWorkConfigModel>();

            var curriculum = await _curriculumRepository.GetByIdAsync(request.CurriculumId);
            if (curriculum == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(curriculum), request.CurriculumId);
                return methodResult;
            }

            var homework = await _homeWorkRepository.GetByIdAsync(request.HomeWorkId);
            if (homework == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(homework), request.HomeWorkId);
                return methodResult;
            }

            HomeWorkConfig? homeworkConfig = null;

            var currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);

            if (request.StartDate <= currentDate || request.StartDate >= request.EndDate)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCurriculumErrorCode.StartDateCannotBeInThePast), nameof(request.StartDate), request.StartDate);
                return methodResult;
            }

            if (request.StartDate < curriculum.StartDate || request.EndDate > curriculum.EndDate)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCurriculumErrorCode.TimeMustBeWithinCurriculumPeriod), nameof(request.StartDate), request.StartDate);
                return methodResult;
            }

            if (request.Id.HasValue)
            {
                homeworkConfig = await _homeWorkConfigRepository.GetByIdAsync(request.Id.Value);
                if (homeworkConfig == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(homeworkConfig), request.Id);
                    return methodResult;
                }

                _mapper.Map(request, homeworkConfig);
            }
            else
            {
                var homeworkIds = await _homeWorkConfigRepository.Queryable.Where(p => p.CurriculumId == curriculum.Id).Select(p => p.HomeWorkId).ToListAsync(cancellationToken);

                if (homeworkIds.Contains(request.HomeWorkId))
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(request.HomeWorkId), request.HomeWorkId);
                    return methodResult;
                }

                homeworkConfig = _mapper.Map<HomeWorkConfig>(request);
            }

            if (!homeworkConfig.IsValid())
            {
                methodResult.AddError(homeworkConfig.ErrorMessages);
                return methodResult;
            }

            var homeworkConfigs = new List<HomeWorkConfig>() { homeworkConfig };

            await _homeWorkConfigRepository.ExecuteTransactionAsync(async () =>
            {
                await _requestSafeCachingService.SafeRequest<HomeWorkConfig>(
                    key: $"Add_HomeWorkConfig_{homeworkConfig.CurriculumId}_{homeworkConfig.HomeWorkId}",
                    safeFunction: async () =>
                    {
                        await _homeWorkConfigRepository.BulkMergeAsync(homeworkConfigs);
                        return homeworkConfig;
                    });
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<HomeWorkConfigModel>(homeworkConfig);
                return methodResult;
            });

            return methodResult;
        }
    }
}
