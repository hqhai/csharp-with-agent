// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Queries.SurveyConfigQuery
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Interaction.Application.Services.UserServices;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentSurveySummaryChartQuery : IRequest<MethodResult<StudentSurveySummaryChartModels>>
    {
        public Guid SurveyConfigId { get; set; }
    }

    public class GetStudentSurveySummaryChartQueryHandler : IRequestHandler<GetStudentSurveySummaryChartQuery, MethodResult<StudentSurveySummaryChartModels>>
    {
        private readonly ISurveyConfigRepository _surveyConfigRepository;
        private readonly ISurveyQuestionRepository _surveyQuestionRepository;
        private readonly ICustomerSurveyRepository _customerSurveyRepository;
        private readonly ICustomerSurveyGroupRepository _customerSurveyGroupRepository;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public GetStudentSurveySummaryChartQueryHandler(ISurveyConfigRepository surveyConfigRepository, ISurveyQuestionRepository surveyQuestionRepository, ICustomerSurveyRepository customerSurveyRepository, ICustomerSurveyGroupRepository customerSurveyGroupRepository, IUserService userService, IMapper mapper)
        {
            _surveyConfigRepository = surveyConfigRepository;
            _surveyQuestionRepository = surveyQuestionRepository;
            _customerSurveyRepository = customerSurveyRepository;
            _customerSurveyGroupRepository = customerSurveyGroupRepository;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<MethodResult<StudentSurveySummaryChartModels>> Handle(GetStudentSurveySummaryChartQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<StudentSurveySummaryChartModels>();

            var surveyConfig = await _surveyConfigRepository.Queryable.Include(p => p.SurveyQuestions).FirstOrDefaultAsync(p => p.Id == request.SurveyConfigId, cancellationToken);
            if (surveyConfig == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(surveyConfig));
                return methodResult;
            }

            var surveyQuestionIds = surveyConfig.SurveyQuestions.Select(p => p.Id).ToList();

            var customerSurveyEntities = await (from cs in _customerSurveyRepository.Queryable.WhereBulkContains(surveyQuestionIds, p => p.SurveyQuestionId)
                                                join csg in _customerSurveyGroupRepository.Queryable on cs.CustomerSurveyGroupId equals csg.Id
                                                where csg.Status == EnumSurveyGroupStatus.Done
                                                select cs).ToListAsync(cancellationToken);

            var userIds = customerSurveyEntities.Select(p => p.UserId).Distinct().ToList();
            var studentResults = await _userService.GetStudentByUserIdsAsync(userIds);
            var students = studentResults.Content?.Result;

            surveyConfig.SurveyQuestions = surveyConfig.SurveyQuestions.OrderBy(p => p.DisplayLevel).ThenBy(p => p.DisplayOrder).ToList();

            var results = new StudentSurveySummaryChartModels();
            results.TotalUser = userIds.Count;

            foreach (var surveyQuestion in surveyConfig.SurveyQuestions)
            {
                var customerSurveys = customerSurveyEntities.Where(p => p.SurveyQuestionId == surveyQuestion.Id).Where(p => p.AnswerStr != null).ToList();

                var answers = ConvertHelper.Deserialize<IList<IList<AnswerSurveyModel>>>(customerSurveys.Select(p => p.Answer));

                int totalCount = answers?.SelectMany(innerList => innerList).Where(x => !string.IsNullOrEmpty(x.Content)).Count() ?? 0;

                var studentSurvey = new StudentSurveySummaryChartModel()
                {
                    Question = surveyQuestion.Question,
                    TotalAnswer = totalCount,
                    QuestionType = surveyQuestion.Type
                };

                if (surveyQuestion.Type != EnumSurveyQuestion.ShortAnswer)
                {
                    var answersQuestion = ConvertHelper.Deserialize<IList<AnswerSurveyModel>>(surveyQuestion.Answers);
                    if (answersQuestion != null && answersQuestion.Any())
                    {
                        foreach (var answerQuestion in answersQuestion.OrderBy(p => p.Id))
                        {
                            if (!answerQuestion.IsOther)
                            {
                                var countAnswer = answers?.Where(p => p.Select(x => x.Id).Contains(answerQuestion.Id)).Count() ?? 0;
                                studentSurvey.StudentSurveySummaries.Add(new StudentSurveySummaryAnswerChartModel()
                                {
                                    Answer = answerQuestion.Content,
                                    TotalAnswer = countAnswer,
                                    PercentAnswer = ((double)countAnswer / totalCount) * 100,
                                });
                            }
                            else
                            {
                                var answersOther = answers?.Where(p => p.Any(x => x.IsOther)).ToList();
                                if (answersOther != null && answersOther.Any())
                                {
                                    foreach (var item in answersOther)
                                    {
                                        var answerOther = item.FirstOrDefault(x => x.IsOther);
                                        if (answerOther != null && !string.IsNullOrEmpty(answerOther.Content))
                                        {
                                            studentSurvey.StudentSurveySummaries.Add(new StudentSurveySummaryAnswerChartModel()
                                            {
                                                Answer = answerOther.Other ?? answerOther.Content,
                                                TotalAnswer = 1,
                                                PercentAnswer = (1.0 / totalCount) * 100,
                                            });
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    foreach (var customerSurvey in customerSurveys)
                    {
                        var student = students?.FirstOrDefault(p => p.Human != null && p.Human.UserId == customerSurvey.UserId);

                        var answer = ConvertHelper.Deserialize<IList<AnswerSurveyModel>>(customerSurvey.Answer)?.FirstOrDefault();
                        if (answer != null && !string.IsNullOrEmpty(answer.Content))
                        {
                            studentSurvey.StudentSurveyShortAnswers.Add(new StudentSurveyShortAnswerModel()
                            {
                                Answer = answer.Content,
                                FullName = student?.Human?.FullName ?? "Not Found",
                                Email = student?.Human?.FullName ?? null,
                                PhoneNumber = student?.Human?.User?.PhoneNumber ?? null
                            });
                        }
                    }
                }

                results.StudentSurveySummaries.Add(studentSurvey);
            }
            methodResult.Result = results;
            return methodResult;
        }
    }
}
