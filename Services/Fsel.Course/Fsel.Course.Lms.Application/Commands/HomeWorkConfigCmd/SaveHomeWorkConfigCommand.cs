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
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.HomeWorkConfigs;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class SaveHomeWorkConfigCommand : SaveHomeWorkConfigCommandModel, IRequest<MethodResult<HomeWorkConfigModel>>
    {
    }

    public class SaveHomeWorkConfigCommandHandler : IRequestHandler<SaveHomeWorkConfigCommand, MethodResult<HomeWorkConfigModel>>
    {
        private readonly IHomeWorkConfigRepository _homeWorkConfigRepository;
        private readonly IMapper _mapper;
        private readonly ICurriculumRepository _curriculumRepository;
        private readonly IHomeWorkRepository _homeWorkRepository;

        public SaveHomeWorkConfigCommandHandler(IHomeWorkConfigRepository homeWorkConfigRepository, IMapper mapper, ICurriculumRepository curriculumRepository, IHomeWorkRepository homeWorkRepository)
        {
            _homeWorkConfigRepository = homeWorkConfigRepository;
            _mapper = mapper;
            _curriculumRepository = curriculumRepository;
            _homeWorkRepository = homeWorkRepository;
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

            if (request.StartDate < currentDate || request.StartDate > request.EndDate || request.StartDate < curriculum.StartDate || request.EndDate > curriculum.EndDate)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(request.StartDate), request.StartDate);
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
                await _homeWorkConfigRepository.BulkMergeAsync(homeworkConfigs);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<HomeWorkConfigModel>(homeworkConfig);
                return methodResult;
            });

            return methodResult;
        }
    }
}
