// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.CurriculumQuery
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Shared.Models.ShareModels.CampusModel;
    using LinqKit;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentsLearningProgressQuery : GetStudentsLearningProgressQueryModel, IRequest<MethodResult<IList<StudentCampusLearningProgressModel>>>
    {
    }

    public class GetStudentsLearningProgressQueryHandler : IRequestHandler<GetStudentsLearningProgressQuery, MethodResult<IList<StudentCampusLearningProgressModel>>>
    {
        private readonly ICurriculumStudentRepository _curriculumStudentRepository;
        private readonly ICurriculumRepository _curriculumRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ICourseRepository _courseRepository;

        public GetStudentsLearningProgressQueryHandler(ICurriculumStudentRepository curriculumStudentRepository, ICurriculumRepository curriculumRepository, ILessonResultRepository lessonResultRepository, ICourseResultRepository courseResultRepository, ICourseRepository courseRepository)
        {
            _curriculumStudentRepository = curriculumStudentRepository;
            _curriculumRepository = curriculumRepository;
            _lessonResultRepository = lessonResultRepository;
            _courseResultRepository = courseResultRepository;
            _courseRepository = courseRepository;
        }

        public async Task<MethodResult<IList<StudentCampusLearningProgressModel>>> Handle(GetStudentsLearningProgressQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<StudentCampusLearningProgressModel>>();

            var query = from baseQuery in _curriculumStudentRepository.Queryable.WhereBulkContains(request.StudentIds, p => p.StudentId)
                        join cu in _curriculumRepository.Queryable on baseQuery.CurriculumId equals cu.Id
                        join c in _courseRepository.Queryable on cu.CourseId equals c.Id
                        join cc in _courseRepository.Queryable on cu.CourseCloneId equals cc.Id
                        select new
                        {
                            CurriculumStudent = baseQuery,
                            Curriculum = cu,
                            Course = c,
                            CourseClone = cc
                        };

            var courseIds = query.Select(p => p.Curriculum.CourseCloneId);

            var lessonResultEntities = await _lessonResultRepository.Queryable.WhereBulkContains(request.StudentIds, p => p.StudentId).ToListAsync(cancellationToken);

            var courses = await _courseRepository.Queryable.Include(p => p.CourseUnitMockTests).ThenInclude(p => p.Unit).ThenInclude(p => p.UnitLessons).ThenInclude(p => p.Lesson).ToListAsync(cancellationToken);

            var courseResultEntities = await _courseResultRepository.Queryable.WhereBulkContains(request.StudentIds, p => p.StudentId).ToListAsync(cancellationToken);

            var currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);

            var studentCampusLearningModels = new List<StudentCampusLearningProgressModel>();

            request.StudentIds.ForEach(p =>
            {
                var curriculums = query.Where(x => x.CurriculumStudent.StudentId == p);
                if (curriculums != null && curriculums.Any())
                {
                    curriculums.ForEach(c =>
                    {
                        var studentCampusLearningModel = new StudentCampusLearningProgressModel()
                        {
                            CurriculumName = c.Curriculum.CurriculumName,
                            CourseLevel = c.CourseClone.CourseLevel,
                            CourseType = c.CourseClone.CourseType,
                            CourseName = c.CourseClone.Name,
                            StudentId = p,
                            CurriculumId = c.Curriculum.Id
                        };

                        var lessonResults = lessonResultEntities.Where(x => x.StudentId == p && x.CourseId == c.Curriculum.CourseCloneId).ToList();
                        var lessonResult = lessonResults.Where(x => x.Status != EnumResultStatus.Unfinished).OrderByDescending(p => p.CreatedDate).FirstOrDefault();
                        var course = courses.FirstOrDefault(x => x.Id == c.Curriculum.CourseCloneId);
                        var courseResult = courseResultEntities.FirstOrDefault(x => x.StudentId == p && x.CourseId == c.Curriculum.CourseId);
                        var totalLesson = course?.CourseUnitMockTests.Where(p => p.Unit != null).Select(p => p.Unit).Where(p => p.UnitLessons != null && p.UnitLessons.Any()).SelectMany(p => p.UnitLessons).Count();

                        if (c.Curriculum.StartDate > currentDate)
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

                        studentCampusLearningModel.TotalLessonDone = lessonResults.Where(x => x.StudentId == p && x.Status == EnumResultStatus.Done).Count();
                        studentCampusLearningModel.TotalLesson = totalLesson ?? 0;

                        studentCampusLearningModels.Add(studentCampusLearningModel);
                    });
                }
            });
            methodResult.Result = studentCampusLearningModels;
            return methodResult;
        }
    }
}
