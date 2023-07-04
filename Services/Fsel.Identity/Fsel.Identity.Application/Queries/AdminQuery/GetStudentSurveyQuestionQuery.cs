// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.AdminQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetStudentSurveyQuestionQuery : IRequest<MethodResult<IList<StudentSurveyQuestionModel>>>
    {
        public Guid StudentId { get; set; }
    }

    public class GetStudentSurveyQuestionQueryHandler : IRequestHandler<GetStudentSurveyQuestionQuery, MethodResult<IList<StudentSurveyQuestionModel>>>
    {
        private readonly IMapper _mapper;
        private readonly IStudentRepository _studentRepository;

        public GetStudentSurveyQuestionQueryHandler(IMapper mapper,IStudentRepository studentRepository)
        {
            _mapper = mapper;
            _studentRepository = studentRepository;
        }

        public async Task<MethodResult<IList<StudentSurveyQuestionModel>>> Handle(GetStudentSurveyQuestionQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<StudentSurveyQuestionModel>> methodResult = new MethodResult<IList<StudentSurveyQuestionModel>>();

            var student = await _studentRepository.GetByIdAsync(request.StudentId);
            if (student == null)
            {

            }
            methodResult.Result = userModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
