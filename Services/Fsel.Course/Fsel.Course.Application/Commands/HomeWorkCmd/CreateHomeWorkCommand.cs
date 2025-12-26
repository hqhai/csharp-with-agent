// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.HomeWorkCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.HomeWorks;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Infrastructure.Common.HomeworkHelper;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateHomeWorkCommand : UpdateHomeWorkCommandModel, IRequest<MethodResult<HomeWorkModel>>
    {
    }

    public class CreateHomeWorkCommandHandler : IRequestHandler<CreateHomeWorkCommand, MethodResult<HomeWorkModel>>
    {
        private readonly IMapper _mapper;
        private readonly IHomeWorkRepository _homeWorkRepository;
        private readonly ISkillRepository _skillRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILevelRepository _levelRepository;
        private readonly QuestionConverter _questionConverter;

        public CreateHomeWorkCommandHandler(IMapper mapper, IHomeWorkRepository homeWorkRepository, ISkillRepository skillRepository, ICategoryRepository categoryRepository, ILevelRepository levelRepository, QuestionConverter questionConverter)
        {
            _mapper = mapper;
            _homeWorkRepository = homeWorkRepository;
            _skillRepository = skillRepository;
            _categoryRepository = categoryRepository;
            _levelRepository = levelRepository;
            _questionConverter = questionConverter;
        }

        public async Task<MethodResult<HomeWorkModel>> Handle(CreateHomeWorkCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<HomeWorkModel> methodResult = new MethodResult<HomeWorkModel>();

            if (request.Questions == null || request.Questions.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Questions));
                return methodResult;
            }

            if (await _homeWorkRepository.Queryable.AnyAsync(p => p.Code == request.Code, cancellationToken))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(request.Code), request.Code);
                return methodResult;
            }

            if (request.SkillId.HasValue)
            {
                var skillExists = await _skillRepository.AnyGuidAsync(request.SkillId.Value);
                if (!skillExists)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.SkillId), request.SkillId);
                    return methodResult;
                }
            }

            if (request.ProgramId.HasValue)
            {
                var skillExists = await _categoryRepository.AnyGuidAsync(request.ProgramId.Value);
                if (!skillExists)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.ProgramId), request.ProgramId);
                    return methodResult;
                }
            }

            if (request.LevelId.HasValue)
            {
                var skillExists = await _levelRepository.AnyGuidAsync(request.LevelId.Value);
                if (!skillExists)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.LevelId), request.LevelId);
                    return methodResult;
                }
            }

            var build = HomeWorkFactory.Create(request, _mapper, _questionConverter).Build(version: 0, originalId: Guid.NewGuid(), true, methodResult);
            if (!build.Item1)
            {
                return methodResult;
            }

            var homeWork = build.Item2;

            if (homeWork == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(homeWork));
                return methodResult;
            }

            if (!homeWork.IsValid())
            {
                methodResult.AddErrorBadRequest(homeWork.ErrorMessages);
                return methodResult;
            }

            await _homeWorkRepository.ExecuteTransactionAsync(async () =>
            {
                homeWork = _homeWorkRepository.Add(homeWork);
                await _homeWorkRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<HomeWorkModel>(homeWork);
                return methodResult;
            });

            return methodResult;
        }
    }
}
