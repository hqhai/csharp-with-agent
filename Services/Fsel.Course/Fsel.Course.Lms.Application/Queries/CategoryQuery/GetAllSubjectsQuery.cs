// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.CategoryQuery
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.EntityModels.PlacementTestModels;
    using Fsel.Course.Domain.Models.EntityModels.UserNavigationActionModels;
    using Fsel.Course.Lms.Application.Queries.CourseChangeQuery;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.ChangeCourse;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums.ErrorCodes;
    using MassTransit.Mediator;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using IMediator = MediatR.IMediator;

    public class GetAllSubjectsQuery : IRequest<MethodResult<IList<SubjectModel>>>
    {
    }

    public class GetAllSubjectsQueryHandler : IRequestHandler<GetAllSubjectsQuery, MethodResult<IList<SubjectModel>>>
    {
        private readonly IChangeCourseService _changeCourseService;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;
        private readonly MediatR.IMediator _mediator;

        public GetAllSubjectsQueryHandler(IUserService userService,
            IChangeCourseService changeCourseService,
            AuthContext authContext,
            IMediator mediator)
        {
            _changeCourseService = changeCourseService;
            _userService = userService;
            _authContext = authContext;
            _mediator = mediator;
        }

        public async Task<MethodResult<IList<SubjectModel>>> Handle(GetAllSubjectsQuery request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<IList<SubjectModel>>();
            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }

            var student = studentResult.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }

            var subjectAggregate = await _changeCourseService.GetChangeSubjectAggreate(student, cancellationToken);
            var subjectModels = subjectAggregate.GetSubjectTree();

            var getUserNavigationResult = await _mediator.Send(new GetUserNavigationQuery(), cancellationToken);
            if (getUserNavigationResult.IsOK && getUserNavigationResult?.Result?.Status == EnumNavigateActionStatus.ChooseProgram)
            {
                var lastestLearningProgramId = getUserNavigationResult.Result.FromInfo?.ProgramId;
                if (lastestLearningProgramId.HasValue)
                {
                    foreach (var model in subjectModels)
                    {
                        SetLastestLearnedPromgram(model, lastestLearningProgramId.Value);
                    }
                }
            }

            return new MethodResult<IList<SubjectModel>>() { Result = subjectModels, StatusCode = 200 };
        }

        private static void SetLastestLearnedPromgram(SubjectModel subjectModel, Guid lastestId)
        {
            if (subjectModel.ChildSubjects.Any())
            {
                foreach (var subject in subjectModel.ChildSubjects)
                {
                    SetLastestLearnedPromgram(subject, lastestId);
                }
                subjectModel.IsLastestLearned = subjectModel.ChildSubjects.Any(x => x.IsLastestLearned);
            }
            else
            {
                subjectModel.IsLastestLearned = subjectModel.Id == lastestId;
            }
        }
    }
}
