// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Queries.SurveyQuestionQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Interaction.Application.Services.UserServices;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetSurveyQuestionBySurveyFormType : IRequest<MethodResult<IList<SurveyQuestionModel>>>
    {
        public EnumSurveyFormType SurveyFormType { get; set; }
    }

    public class GetSurveyQuestionBySurveyFormTypeHandler : IRequestHandler<GetSurveyQuestionBySurveyFormType, MethodResult<IList<SurveyQuestionModel>>>
    {
        private readonly ISurveyQuestionRepository _surveyQuestionRepository;
        private readonly AuthContext _authContext;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public GetSurveyQuestionBySurveyFormTypeHandler(ISurveyQuestionRepository surveyQuestionRepository,
                                                        AuthContext authContext,
                                                        IMapper mapper,
                                                        IUserService userService)
        {
            _surveyQuestionRepository = surveyQuestionRepository;
            _authContext = authContext;
            _mapper = mapper;
            _userService = userService;
        }

        public async Task<MethodResult<IList<SurveyQuestionModel>>> Handle(GetSurveyQuestionBySurveyFormType request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<SurveyQuestionModel>> methodResult = new MethodResult<IList<SurveyQuestionModel>>();

            var surveyQuestions = await _surveyQuestionRepository.Queryable
                                                                 .Include(x => x.Translations)
                                                                 .Include(x => x.CustomerSurveys.Where(x => x.UserId == _authContext.CurrentUserId))
                                                                 .Where(x => x.IsPilot == false && x.SurveyFormType == request.SurveyFormType)
                                                                 .OrderBy(x => x.DisplayLevel)
                                                                 .ThenBy(x => x.DisplayOrder)
                                                                 .ToListAsync(cancellationToken);

            if (surveyQuestions == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(surveyQuestions));
                return methodResult;
            }

            if (request.SurveyFormType == EnumSurveyFormType.Event)
            {
                var eventResults = await _userService.GetEventByUserId(_authContext.CurrentUserId);
                if (!eventResults.IsSuccessStatusCode)
                {
                    methodResult.AddError(eventResults.Error);
                    return methodResult;
                }

                var eventResult = eventResults.Content?.Result?.FirstOrDefault();

                surveyQuestions = surveyQuestions.Where(x => x.CompetitionEventId == eventResult?.Id).ToList();
            }

            methodResult.Result = _mapper.Map<IList<SurveyQuestionModel>>(surveyQuestions);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
