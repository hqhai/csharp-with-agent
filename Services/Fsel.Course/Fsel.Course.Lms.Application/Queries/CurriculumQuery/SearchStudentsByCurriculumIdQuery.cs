// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.CurriculumQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.QueryModels.Curriculums;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Models.ShareModels.CampusModel;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class SearchStudentsByCurriculumIdQuery : SearchStudentsByCurriculumIdQueryModel, IRequest<MethodResult<PagingItemsModel<StudentCampusModel>>>
    {
    }

    public class SearchStudentsByCurriculumIdQueryHandler : IRequestHandler<SearchStudentsByCurriculumIdQuery, MethodResult<PagingItemsModel<StudentCampusModel>>>
    {
        private readonly ICurriculumRepository _curriculumRepository;
        private readonly ICurriculumStudentRepository _curriculumStudentRepository;
        private readonly IUserService _userService;
        private readonly IMediator _mediator;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly ICourseRepository _courseRepository;

        public SearchStudentsByCurriculumIdQueryHandler(ICurriculumRepository curriculumRepository, ICurriculumStudentRepository curriculumStudentRepository, IUserService userService, IMediator mediator, ICourseResultRepository courseResultRepository, ILessonResultRepository lessonResultRepository, ICourseRepository courseRepository)
        {
            _curriculumRepository = curriculumRepository;
            _curriculumStudentRepository = curriculumStudentRepository;
            _userService = userService;
            _mediator = mediator;
            _courseResultRepository = courseResultRepository;
            _lessonResultRepository = lessonResultRepository;
            _courseRepository = courseRepository;
        }

        public async Task<MethodResult<PagingItemsModel<StudentCampusModel>>> Handle(SearchStudentsByCurriculumIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<StudentCampusModel>>();

            var studentIds = await (from baseQuery in _curriculumStudentRepository.Queryable
                                    join c in _curriculumRepository.Queryable on baseQuery.CurriculumId equals c.Id
                                    where c.Id == request.CurriculumId
                                    select baseQuery.StudentId).ToListAsync(cancellationToken);

            var studentResults = await _userService.SearchStudentsByStudentIds(new SearchStudentsCampusByStudentIdsQueryModel()
            {
                SchoolClassId = request.SchoolClassId,
                Keyword = request.Keyword,
                StudentIds = studentIds
            });

            var students = studentResults.Content?.Result;

            if (students == null)
            {
                return methodResult;
            }

            var curriculumResult = await _mediator.Send(new GetCurriculumByIdQuery() { Id = request.CurriculumId }, cancellationToken);
            var curriculum = curriculumResult.Result;
            if (curriculum == null)
            {
                methodResult.AddError(curriculumResult.ErrorMessages);
                return methodResult;
            }

            studentIds = students.Items?.Select(x => x.StudentId).ToList();

            var studentsLearningProgress = await _mediator.Send(new GetStudentsLearningProgressQuery() { StudentIds = studentIds }, cancellationToken);

            var studentsLearningProgressModel = studentsLearningProgress.Result;

            students.Items?.ForEach(p =>
            {
                p.LearningProgresses = studentsLearningProgressModel?.Where(x => x.StudentId == p.StudentId && x.CurriculumId == curriculum.Id).ToList();
            });

            methodResult.Result = students;
            return methodResult;
        }
    }
}
