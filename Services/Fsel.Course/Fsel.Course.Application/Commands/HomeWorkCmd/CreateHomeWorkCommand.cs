// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.HomeWorkCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.HomeWorks;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class CreateHomeWorkCommand : CreateHomeWorkCommandModel, IRequest<MethodResult<HomeWorkModel>>
    {
    }

    public class CreateHomeWorkCommandHandler : IRequestHandler<CreateHomeWorkCommand, MethodResult<HomeWorkModel>>
    {
        private readonly IMapper _mapper;
        private readonly QuestionConverter _questionConverter;
        private readonly IHomeWorkRepository _homeWorkRepository;
        private readonly ISkillRepository _skillRepository;

        public CreateHomeWorkCommandHandler(IMapper mapper
            , QuestionConverter questionConverter
            , IHomeWorkRepository homeWorkRepository
            , ISkillRepository skillRepository)
        {
            _mapper = mapper;
            _questionConverter = questionConverter;
            _homeWorkRepository = homeWorkRepository;
            _skillRepository = skillRepository;
        }

        public async Task<MethodResult<HomeWorkModel>> Handle(CreateHomeWorkCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<HomeWorkModel> methodResult = new MethodResult<HomeWorkModel>();

            HomeWork homeWork = _mapper.Map<HomeWork>(request);

            if (request.Questions == null || request.Questions.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Questions));
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

            foreach (var question in request.Questions)
            {
                if (question == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Questions));
                    return methodResult;
                }
                else
                {
                    var newQuestion = _mapper.Map<Question>(question);
                    var method = _questionConverter.HandleQuestion(newQuestion, true);
                    if (!method.IsOK)
                    {
                        methodResult.AddErrorBadRequest(method.ErrorMessages);
                        return methodResult;
                    }
                    homeWork.HomeWorkQuestions.Add(new HomeWorkQuestion
                    {
                        Question = method.Result
                    });
                }
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
