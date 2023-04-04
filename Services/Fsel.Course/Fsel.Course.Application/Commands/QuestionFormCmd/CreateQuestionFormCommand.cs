// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.QuestionFormCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.QuestionForms;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class CreateQuestionFormCommand : CreateQuestionFormCommandModel, IRequest<MethodResult<QuestionFormModel>>
    {
    }

    public class CreateQuestionFormCommandHandler : IRequestHandler<CreateQuestionFormCommand, MethodResult<QuestionFormModel>>
    {
        private readonly IQuestionFormRepository _questionFormRepository;
        private readonly IMapper _mapper;

        public CreateQuestionFormCommandHandler(IQuestionFormRepository questionFormRepository, IMapper mapper)
        {
            _questionFormRepository = questionFormRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<QuestionFormModel>> Handle(CreateQuestionFormCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<QuestionFormModel> methodResult = new MethodResult<QuestionFormModel>();

            QuestionForm questionForm = _mapper.Map<QuestionForm>(request);

            if (!questionForm.IsValid())
            {
                methodResult.AddErrorBadRequest(questionForm.ErrorMessages);
                return methodResult;
            }

            await _questionFormRepository.ExecuteTransactionAsync(async () =>
            {
                questionForm = _questionFormRepository.Add(questionForm);
                await _questionFormRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<QuestionFormModel>(questionForm);
                return methodResult;
            });

            return methodResult;
        }
    }
}
