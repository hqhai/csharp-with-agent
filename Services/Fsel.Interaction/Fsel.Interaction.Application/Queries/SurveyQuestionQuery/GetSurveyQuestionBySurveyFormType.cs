// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Queries.SurveyQuestionQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Interaction.Application.Services.UserServices;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetSurveyQuestionBySurveyFormType : IRequest<MethodResult<IList<SurveyQuestionModel>>>
    {
    }

    public class GetSurveyQuestionBySurveyFormTypeHandler : IRequestHandler<GetSurveyQuestionBySurveyFormType, MethodResult<IList<SurveyQuestionModel>>>
    {
        private readonly ISurveyQuestionRepository _surveyQuestionRepository;
        private readonly AuthContext _authContext;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly ISurveyConfigRepository _surveyConfigRepository;

        public GetSurveyQuestionBySurveyFormTypeHandler(ISurveyQuestionRepository surveyQuestionRepository,
                                                        AuthContext authContext,
                                                        IMapper mapper,
                                                        IUserService userService,
                                                        ISurveyConfigRepository surveyConfigRepository)
        {
            _surveyQuestionRepository = surveyQuestionRepository;
            _authContext = authContext;
            _mapper = mapper;
            _userService = userService;
            _surveyConfigRepository = surveyConfigRepository;
        }

        public async Task<MethodResult<IList<SurveyQuestionModel>>> Handle(GetSurveyQuestionBySurveyFormType request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<SurveyQuestionModel>> methodResult = new MethodResult<IList<SurveyQuestionModel>>();

            var eventResults = await _userService.GetEventByUserId(_authContext.CurrentUserId);
            if (!eventResults.IsSuccessStatusCode)
            {
                methodResult.AddError(eventResults.Error);
                return methodResult;
            }

            var @event = eventResults.Content?.Result?.FirstOrDefault();

            EnumSurveyFormType formType = EnumSurveyFormType.Default;
            Guid? competitionEventId = null;

            if (@event != null)
            {
                formType = EnumSurveyFormType.Event;
                competitionEventId = @event.Id;
            }

            var currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);

            var query = await (from sc in _surveyConfigRepository.Queryable.Where(p => p.StartDate <= currentDate && p.EndDate >= currentDate)
                               join sq in _surveyQuestionRepository.Queryable on sc.Id equals sq.SurveyConfigId
                               where sc.StartDate <= currentDate && sc.EndDate >= currentDate
                               select new
                               {
                                   SurveyConfig = sc,
                                   SurveyQuestion = sq
                               }).ToListAsync(cancellationToken);

            query = query.Where(p => p.SurveyConfig.ApplicablePrograms != null && p.SurveyConfig.ApplicablePrograms.Contains(formType)).ToList();

            if (competitionEventId.HasValue)
            {
                query = query.Where(p => p.SurveyConfig.CompetitionEventIds != null && p.SurveyConfig.CompetitionEventIds.Contains(competitionEventId.Value)).ToList();
            }

            //var surveyQuestions = await _surveyQuestionRepository.Queryable
            //                                                     .Include(x => x.Translations)
            //                                                     .Include(x => x.CustomerSurveys.Where(x => x.UserId == _authContext.CurrentUserId))
            //                                                     .Where(x => x.IsPilot == false && x.SurveyFormType == request.SurveyFormType)
            //                                                     .OrderBy(x => x.DisplayLevel)
            //                                                     .ThenBy(x => x.DisplayOrder)
            //                                                     .ToListAsync(cancellationToken);

            //if (surveyQuestions == null)
            //{
            //    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(surveyQuestions));
            //    return methodResult;
            //}

            //methodResult.Result = _mapper.Map<IList<SurveyQuestionModel>>(surveyQuestions);
            //methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
