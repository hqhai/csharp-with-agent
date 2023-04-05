// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.HomeWorkCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.HomeWorks;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Repositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateHomeWorkCommand : UpdateHomeWorkCommandModel, IRequest<MethodResult<HomeWorkModel>>
    {
    }

    public class UpdateHomeWorkCommandHandler : IRequestHandler<UpdateHomeWorkCommand, MethodResult<HomeWorkModel>>
    {
        private readonly IMapper _mapper;
        private readonly IHomeWorkRepository _homeWorkRepository;
        private readonly IQuestionRepository _questionRepository;

        public UpdateHomeWorkCommandHandler(IMapper mapper
            , IHomeWorkRepository homeWorkRepository
            , IQuestionRepository questionRepository)
        {
            _mapper = mapper;
            _homeWorkRepository = homeWorkRepository;
            _questionRepository = questionRepository;
        }

        public async Task<MethodResult<HomeWorkModel>> Handle(UpdateHomeWorkCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<HomeWorkModel> methodResult = new MethodResult<HomeWorkModel>();
            var homeWork = await _homeWorkRepository.Queryable
                            .Include(x => x.HomeWorkQuestions)
                            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken: cancellationToken);
            if (homeWork == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumHomeWorkErrorCode.HomeWorksNull),
                                               nameof(request.Id), request.Id);
                return methodResult;
            }
            _mapper.Map(request, homeWork);

            if (request.QuestionIds == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumQuestionErrorCode.QusetionIdNotCorrect), nameof(request.QuestionIds), request.QuestionIds);
                return methodResult;
            }
            if (_questionRepository.IsIdsInValid(request.QuestionIds))
            {
                methodResult.AddErrorBadRequest(nameof(EnumQuestionErrorCode.QuestionNotCorrect), nameof(request.QuestionIds), request.QuestionIds);
                return methodResult;
            }
            if (!homeWork.IsValid())
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddResultFromErrorList(homeWork.ErrorMessages);
                return methodResult;
            }
            await _homeWorkRepository.ExecuteTransactionAsync(async () =>
            {
                homeWork = _homeWorkRepository.Update(homeWork);
                await _homeWorkRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<HomeWorkModel>(homeWork);
                return methodResult;
            });

            return methodResult;
        }
    }
}
