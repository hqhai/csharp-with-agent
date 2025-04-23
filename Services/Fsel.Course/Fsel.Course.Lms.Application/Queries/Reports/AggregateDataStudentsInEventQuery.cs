namespace Fsel.Course.Lms.Application.Queries.Reports
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.ManagerReports;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.SystemService.QueryModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class AggregateDataStudentsInEventQuery : AggregateDataStudentsInEventQueryModel, IRequest<MethodResult<IList<AggregateDataStudentsInEventModel>>>
    {
    }

    public class AggregateDataStudentsInEventQueryHandler : IRequestHandler<AggregateDataStudentsInEventQuery, MethodResult<IList<AggregateDataStudentsInEventModel>>>
    {
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly ISystemService _systemService;

        public AggregateDataStudentsInEventQueryHandler(ILessonResultRepository lessonResultRepository, IClassForumResultRepository classForumResultRepository, ICourseResultRepository courseResultRepository, IUnitResultRepository unitResultRepository, ISystemService systemService)
        {
            _lessonResultRepository = lessonResultRepository;
            _classForumResultRepository = classForumResultRepository;
            _courseResultRepository = courseResultRepository;
            _unitResultRepository = unitResultRepository;
            _systemService = systemService;
        }

        public async Task<MethodResult<IList<AggregateDataStudentsInEventModel>>> Handle(AggregateDataStudentsInEventQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<AggregateDataStudentsInEventModel>>();

            var studentIds = request.StudentIds;
            if (studentIds == null || !studentIds.Any())
            {
                return methodResult;
            }

            var unitResultEntities = await (from ur in _unitResultRepository.Queryable.WhereBulkContains(studentIds, p => p.StudentId)
                                            join cr in _courseResultRepository.Queryable on new { ur.StudentId, ur.CourseId } equals new { cr.StudentId, cr.CourseId }
                                            where cr.WorkingStatus == EnumWorkingStatus.Active && ur.Status == EnumResultStatus.Done && ur.CompletionDate >= request.StartDate && ur.CompletionDate <= request.EndDate
                                            select ur).ToListAsync(cancellationToken);

            var lessonResultEntities = await (from lr in _lessonResultRepository.Queryable.WhereBulkContains(studentIds, p => p.StudentId)
                                              join cr in _courseResultRepository.Queryable on new { lr.StudentId, lr.CourseId } equals new { cr.StudentId, cr.CourseId }
                                              where cr.WorkingStatus == EnumWorkingStatus.Active && lr.Status == EnumResultStatus.Done && lr.UpdatedDate >= request.StartDate && lr.UpdatedDate <= request.EndDate
                                              select lr).ToListAsync(cancellationToken);

            var classForumResultEntities = await (from lr in _lessonResultRepository.Queryable.WhereBulkContains(studentIds, p => p.StudentId)
                                                  join cfr in _classForumResultRepository.Queryable on lr.Id equals cfr.LessonResultId
                                                  join cr in _courseResultRepository.Queryable on new { lr.StudentId, lr.CourseId } equals new { cr.StudentId, cr.CourseId }
                                                  where cr.WorkingStatus == EnumWorkingStatus.Active && cfr.Status.HasValue && cfr.UpdatedDate >= request.StartDate && cfr.UpdatedDate <= request.EndDate
                                                  select cfr).ToListAsync(cancellationToken);

            var unitIds = unitResultEntities.Select(p => p.UnitId).Distinct().ToList();

            var chatbotConfigResults = await _systemService.GetUnitChatbotConfigsByUnitIds(new GetUnitChatbotConfigsQueryModel() { UnitIds = unitIds });
            var chatbotConfigs = chatbotConfigResults.Content?.Result;

            var students = new List<AggregateDataStudentsInEventModel>();

            studentIds.ForEach(student =>
            {
                var lessonResults = lessonResultEntities.Where(p => p.StudentId == student).ToList();
                var classForumResults = classForumResultEntities.Where(p => p.StudentId == student).ToList();
                var unitResultIds = unitResultEntities.Where(p => p.StudentId == student).Select(p => p.UnitId).ToList();

                var unitChatbotConfigs = chatbotConfigs?.Where(p => unitResultIds != null && unitResultIds.Contains(p.UnitId)).ToList();

                var totalReading = lessonResults.Where(p => p.SkillScores != null && p.SkillScores.Any()).SelectMany(p => p.SkillScores!).Where(p => p.Skill == EnumCourseSkill.Reading).Sum(p => p.TotalQuestion);
                var totalListening = lessonResults.Where(p => p.SkillScores != null && p.SkillScores.Any()).SelectMany(p => p.SkillScores!).Where(p => p.Skill == EnumCourseSkill.Listening).Sum(p => p.TotalQuestion);

                var totalWriting = classForumResults.Where(p => p.SkillScores != null && p.SkillScores.Any()).SelectMany(p => p.SkillScores!).Where(p => p.Skill == EnumCourseSkill.Writing).Count();
                var totalSpeaking = classForumResults.Where(p => p.SkillScores != null && p.SkillScores.Any()).SelectMany(p => p.SkillScores!).Where(p => p.Skill == EnumCourseSkill.Speaking).Count();

                var totalVocabulary = unitChatbotConfigs?.Where(p => p.ChatbotSkillConfigs != null && p.ChatbotSkillConfigs.Any()).SelectMany(p => p.ChatbotSkillConfigs!).Where(p => p.Skill == EnumCourseSkill.Vocabulary).Select(p => p.Configs).SelectMany(p => p).Count();

                var totalGrammar = unitChatbotConfigs?.Where(p => p.ChatbotSkillConfigs != null && p.ChatbotSkillConfigs.Any()).SelectMany(p => p.ChatbotSkillConfigs!).Where(p => p.Skill == EnumCourseSkill.Grammar).Select(p => p.Configs).SelectMany(p => p).Count();

                students.Add(new AggregateDataStudentsInEventModel()
                {
                    StudentId = student,
                    TotalLesson = lessonResults.Count,
                    TotalReading = (int)totalReading,
                    TotalListening = (int)totalListening,
                    TotalWriting = totalWriting,
                    TotalSpeaking = totalSpeaking,
                    TotalGrammar = totalGrammar ?? 0,
                    TotalVocabulary = totalVocabulary ?? 0,
                });
            });

            methodResult.Result = students;
            return methodResult;
        }
    }
}
