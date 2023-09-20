// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.QuestionFormQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetQuestionFormQuery : IRequest<MethodResult<QuestionFormModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetQuestionFormQueryHandler : IRequestHandler<GetQuestionFormQuery, MethodResult<QuestionFormModel>>
    {
        private readonly IQuestionFormRepository _questionFormRepository;
        private readonly IMapper _mapper;

        public GetQuestionFormQueryHandler(IQuestionFormRepository questionFormRepository, IMapper mapper)
        {
            _questionFormRepository = questionFormRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<QuestionFormModel>> Handle(GetQuestionFormQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<QuestionFormModel> methodResult = new MethodResult<QuestionFormModel>();
            var questionForm = await _questionFormRepository.GetByIdAsync(request.Id);

            if (questionForm == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(questionForm));
                return methodResult;
            }

            methodResult.Result = _mapper.Map<QuestionFormModel>(questionForm);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
