// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.CurriculumQuery
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Shared.Models.ShareModels.CampusModel;
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

            if (request.StudentIds == null || !request.StudentIds.Any())
            {
                return methodResult;
            }

            var query = await (from baseQuery in _curriculumStudentRepository.Queryable.WhereBulkContains(request.StudentIds, p => p.StudentId)
                               join cu in _curriculumRepository.Queryable on baseQuery.CurriculumId equals cu.Id
                               join c in _courseRepository.Queryable on cu.CourseId equals c.Id
                               join cc in _courseRepository.Queryable on cu.CourseCloneId equals cc.Id
                               select new
                               {
                                   CurriculumStudent = baseQuery,
                                   Curriculum = cu,
                                   Course = c,
                                   CourseClone = cc
                               }).ToListAsync(cancellationToken);

            var courseIds = query.Select(p => p.Curriculum.CourseCloneId).ToList();

            var lessonResultEntities = await _lessonResultRepository.Queryable.WhereBulkContains(request.StudentIds, p => p.StudentId).ToListAsync(cancellationToken);

            var courses = await _courseRepository.Queryable.Include(p => p.CourseUnitMockTests).ThenInclude(p => p.Unit).ThenInclude(p => p.UnitLessons).ThenInclude(p => p.Lesson).ToListAsync(cancellationToken);

            var courseResultEntities = await _courseResultRepository.Queryable.WhereBulkContains(request.StudentIds, p => p.StudentId).ToListAsync(cancellationToken);

            var currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);

            var studentCampusLearningModelsBag = new ConcurrentBag<StudentCampusLearningProgressModel>();

            Parallel.ForEach(request.StudentIds, p =>
            {
                var curriculums = query.Where(x => x.CurriculumStudent.StudentId == p).ToList();
                if (curriculums.Any())
                {
                    foreach (var c in curriculums)
                    {
                        var studentCampusLearningModel = new StudentCampusLearningProgressModel()
                        {
                            CurriculumName = c.Curriculum.CurriculumName,
                            CourseLevel = c.CourseClone.CourseLevel,
                            CourseType = c.CourseClone.CourseType,
                            CourseName = c.CourseClone.Name,
                            StudentId = p,
                            CurriculumId = c.Curriculum.Id,
                            StartDate = c.Curriculum.StartDate,
                            EndDate = c.Curriculum.EndDate
                        };

                        var lessonResults = lessonResultEntities
                            .Where(x => x.StudentId == p && x.CourseId == c.Curriculum.CourseCloneId)
                            .ToList();

                        var lessonResult = lessonResults
                            .Where(x => x.Status != EnumResultStatus.Unfinished)
                            .OrderByDescending(r => r.CreatedDate)
                            .FirstOrDefault();

                        var course = courses.FirstOrDefault(x => x.Id == c.Curriculum.CourseCloneId);

                        var courseResult = courseResultEntities
                            .FirstOrDefault(x => x.StudentId == p && x.CourseId == c.Curriculum.CourseId);

                        var totalLesson = course?.CourseUnitMockTests
                            .Where(u => u.Unit != null)
                            .Select(u => u.Unit)
                            .Where(u => u.UnitLessons != null && u.UnitLessons.Any())
                            .SelectMany(u => u.UnitLessons)
                            .Count();

                        if (c.Curriculum.StartDate > currentDate)
                        {
                            studentCampusLearningModel.Status = EnumStudentCampusLearningStatus.NotStarted;
                        }
                        else
                        {
                            if (courseResult == null || lessonResult == null)
                            {
                                studentCampusLearningModel.Status = EnumStudentCampusLearningStatus.InProgress;
                                studentCampusLearningModel.LessonName = course?.CourseUnitMockTests
                                    .Where(u => u.Unit != null)
                                    .OrderBy(u => u.Number)
                                    .Select(u => u.Unit)
                                    .FirstOrDefault()?
                                    .UnitLessons.OrderBy(l => l.DisplayOrder)
                                    .FirstOrDefault()?.Lesson?.Name;
                            }
                            else if (courseResult.Status == EnumResultStatus.Done)
                            {
                                studentCampusLearningModel.Status = EnumStudentCampusLearningStatus.Completed;
                            }
                            else
                            {
                                var lesson = course?.CourseUnitMockTests
                                    .Where(u => u.Unit != null)
                                    .Select(u => u.Unit)
                                    .Where(u => u.UnitLessons.Any())
                                    .SelectMany(u => u.UnitLessons)
                                    .FirstOrDefault(x => x.LessonId == lessonResult.LessonId)?.Lesson;

                                studentCampusLearningModel.LessonName = lesson?.Name;
                                studentCampusLearningModel.Status = EnumStudentCampusLearningStatus.InProgress;
                            }
                        }

                        studentCampusLearningModel.TotalLessonDone = lessonResults
                            .Count(x => x.StudentId == p && x.Status == EnumResultStatus.Done);

                        studentCampusLearningModel.TotalLesson = totalLesson ?? 0;

                        studentCampusLearningModelsBag.Add(studentCampusLearningModel);
                    }
                }
            });

            var studentCampusLearningModels = studentCampusLearningModelsBag.ToList();

            methodResult.Result = studentCampusLearningModels;
            return methodResult;
        }
    }
}
