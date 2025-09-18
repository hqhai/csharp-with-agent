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
                Class = request.Class,
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

            var lessonResults = await _lessonResultRepository.Queryable.WhereBulkContains(studentIds, p => p.StudentId).Where(p => p.CourseId == curriculum.CourseCloneId).ToListAsync(cancellationToken);

            var course = await _courseRepository.Queryable.Include(p => p.CourseUnitMockTests).ThenInclude(p => p.Unit).ThenInclude(p => p.UnitLessons).ThenInclude(p => p.Lesson).FirstOrDefaultAsync(p => p.Id == curriculum.CourseCloneId, cancellationToken);

            var courseResults = await _courseResultRepository.Queryable.WhereBulkContains(studentIds, p => p.StudentId).Where(p => p.CourseId == curriculum.CourseCloneId).ToListAsync(cancellationToken);

            var totalLesson = course?.CourseUnitMockTests.Where(p => p.Unit != null).Select(p => p.Unit).Where(p => p.UnitLessons != null && p.UnitLessons.Any()).SelectMany(p => p.UnitLessons).Count();

            var currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);

            students.Items.ForEach(p =>
            {
                var studentCampusLearningModel = new StudentCampusLearningModel()
                {
                    CourseLevel = curriculum.CourseLevel,
                    CourseType = curriculum.CourseType,
                    CourseName = curriculum.CourseName
                };

                var courseResult = courseResults.FirstOrDefault(x => x.StudentId == p.StudentId);
                var lessonResult = lessonResults.Where(x => x.Status != EnumResultStatus.Unfinished).OrderByDescending(p => p.CreatedDate).FirstOrDefault();

                if (curriculum.StartDate > currentDate)
                {
                    studentCampusLearningModel.ProgressStatus = EnumStudentCampusLearningStatus.NotStarted;
                }
                else if (courseResult == null)
                {
                    studentCampusLearningModel.ProgressStatus = EnumStudentCampusLearningStatus.NotJoined;
                }
                else
                {
                    if (courseResult.Status == EnumResultStatus.Done)
                    {
                        studentCampusLearningModel.ProgressStatus = EnumStudentCampusLearningStatus.Completed;
                    }
                    else
                    {
                        var lesson = course?.CourseUnitMockTests.Select(u => u.Unit).SelectMany(ul => ul.UnitLessons).FirstOrDefault(x => x.LessonId == lessonResult?.LessonId)?.Lesson;
                        studentCampusLearningModel.LessonName = lesson?.Name;
                        studentCampusLearningModel.ProgressStatus = EnumStudentCampusLearningStatus.InProgress;
                    }
                }

                studentCampusLearningModel.TotalLessonDone = lessonResults.Where(x => x.StudentId == p.StudentId && x.Status == EnumResultStatus.Done).Count();
                studentCampusLearningModel.TotalLesson = totalLesson ?? 0;

                p.Students = new List<StudentCampusLearningModel>() { studentCampusLearningModel };
            });

            methodResult.Result = students;
            return methodResult;
        }
    }
}
