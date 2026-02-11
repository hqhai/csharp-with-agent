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
    using Fsel.Course.Lms.Application.Services.ApplicationServices;
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
        private readonly ILearningService _learningService;
        private readonly IServiceProvider _serviceProvider;

        public GetStudentsLearningProgressQueryHandler(ICurriculumStudentRepository curriculumStudentRepository, ICurriculumRepository curriculumRepository, ILessonResultRepository lessonResultRepository, ICourseResultRepository courseResultRepository, ICourseRepository courseRepository, ILearningService learningService, IServiceProvider serviceProvider)
        {
            _curriculumStudentRepository = curriculumStudentRepository;
            _curriculumRepository = curriculumRepository;
            _lessonResultRepository = lessonResultRepository;
            _courseResultRepository = courseResultRepository;
            _courseRepository = courseRepository;
            _learningService = learningService;
            _serviceProvider = serviceProvider;
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
                               join cc in _courseRepository.Queryable.Include(x => x.Program).ThenInclude(x => x.CategoryParent).Include(x => x.Level) on cu.CourseCloneId equals cc.Id
                               select new
                               {
                                   CurriculumStudent = baseQuery,
                                   Curriculum = cu,
                                   Course = c,
                                   CourseClone = cc
                               }).ToListAsync(cancellationToken);

            var courseResultEntities = await _courseResultRepository.Queryable.WhereBulkContains(request.StudentIds, p => p.StudentId).ToListAsync(cancellationToken);

            var currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);

            var studentCampusLearningModels = new List<StudentCampusLearningProgressModel>();

            var learningComponentModels = query.Select(p => new GetLearningTreeFromCourseToTestModel()
            {
                StudentId = p.CurriculumStudent.StudentId,
                CourseId = p.Curriculum.CourseCloneId
            }).ToList();

            var results = await _learningService.GetLearningTreeFromCourseToTest(learningComponentModels, cancellationToken);

            foreach (var item in request.StudentIds)
            {
                var curriculums = query.Where(x => x.CurriculumStudent.StudentId == item).ToList();
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
                            ProgramName = c.CourseClone.Program?.Name,
                            LevelName = c.CourseClone.Level?.Name,
                            SubjectName = c.CourseClone.Program?.CategoryParent?.Name,
                            StudentId = item,
                            CurriculumId = c.Curriculum.Id,
                            StartDate = c.Curriculum.StartDate,
                            EndDate = c.Curriculum.EndDate,
                            CourseCloneId = c.Curriculum.CourseCloneId
                        };

                        var courseResult = courseResultEntities
                           .FirstOrDefault(x => x.StudentId == item && x.CourseId == c.Curriculum.CourseCloneId);

                        if (c.Curriculum.StartDate > currentDate)
                        {
                            studentCampusLearningModel.Status = EnumStudentCampusLearningStatus.NotStarted;
                        }
                        else
                        {
                            if (courseResult != null && courseResult.Status == EnumResultStatus.Done)
                            {
                                studentCampusLearningModel.Status = EnumStudentCampusLearningStatus.Completed;
                            }
                            else
                            {
                                studentCampusLearningModel.Status = EnumStudentCampusLearningStatus.InProgress;
                            }
                        }

                        var result = results.FirstOrDefault(p => item == p.StudentId && c.Curriculum.CourseCloneId == p.LearningTemplateId);
                        if (result != null)
                        {
                            var lessons = result.Children.SelectMany(p => p.Children).ToList();

                            studentCampusLearningModel.TotalLesson = lessons.Count;
                            studentCampusLearningModel.TotalLessonDone = lessons.Count(n => n.Status == EnumResultStatus.Done);
                        }

                        studentCampusLearningModels.Add(studentCampusLearningModel);
                    }
                }
            }


            methodResult.Result = studentCampusLearningModels;
            return methodResult;
        }
    }
}
