// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ProgressQuery.V1i2
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Models.EntityModels.CachingModels;

    public static class BaseLinqProgress
    {
        public static SkillScores AggregateResult(IEnumerable<SkillScores> rows)
        {
            var list = rows.ToList();
            if (list.Count == 0)
            {
                return new SkillScores();
            }
            return new SkillScores
            {
                SkillFilePath = list.First().SkillFilePath,
                SkillId = list.First().SkillId,
                SkillName = list.First().SkillName,
                CorrectCount = list.Sum(x => x.CorrectCount),
                CorrectQuestion = list.Sum(x => x.CorrectQuestion ?? 0),
                CountQuestion = list.Sum(x => x.CountQuestion),
                Percent = list.Average(x => x.Percent),
                TokenReceived = list.Sum(x => x.TokenReceived),
            };
        }

        public static List<Guid> ExtractIdsPreferResult(CourseBuildModel courseBuild, IList<VideoResult> videoResults)
        {
            if (courseBuild?.CourseModules == null || courseBuild.CourseModules.Count == 0)
            {
                return new List<Guid>();
            }

            var buildModules = courseBuild.CourseModules
                .Where(x => x.ConfigType == EnumCourseConfigType.Unit)
                .SelectMany(x => x.UnitModuleBuilds)
                .Where(x => x.ConfigType == EnumUnitConfigType.Lesson)
                .SelectMany(x => x.LessonModuleBuilds)
                .Where(x => x.ConfigType == EnumLessonConfigType.Video
                            && x.VideoId.HasValue)
                .Select(x => new
                {
                    LessonModuleId = x.Id,
                    OriginalId = x.OriginalId,
                    CurrentId = x.VideoId!.Value
                })
                .ToList();

            if (buildModules.Count == 0)
            {
                return new List<Guid>();
            }

            var resultIdByLessonModuleId = (videoResults ?? new List<VideoResult>())
                            .Where(r => r.LessonModuleId.HasValue && r.LessonModuleId != Guid.Empty)
                            .GroupBy(r => r.LessonModuleId ?? Guid.Empty)
                            .ToDictionary(g => g.Key, g => g.First().VideoId);

            var result = buildModules
                .GroupBy(x => x.OriginalId)
                .Select(g =>
                {
                    var idFromResult = g
                        .Select(m => resultIdByLessonModuleId.TryGetValue(m.LessonModuleId, out var vid) ? (Guid?)vid : null)
                        .FirstOrDefault(v => v.HasValue);

                    return idFromResult ?? g.First().CurrentId;
                })
                .Distinct()
                .ToList();

            return result;
        }

        public static List<Guid> ExtractIdsPreferResult(CourseBuildModel courseBuild, IList<HomeWorkResult> homeWorkResults)
        {
            if (courseBuild?.CourseModules == null || courseBuild.CourseModules.Count == 0)
            {
                return new List<Guid>();
            }

            var buildModules = courseBuild.CourseModules
                .Where(x => x.ConfigType == EnumCourseConfigType.Unit)
                .SelectMany(x => x.UnitModuleBuilds)
                .Where(x => x.ConfigType == EnumUnitConfigType.Lesson)
                .SelectMany(x => x.LessonModuleBuilds)
                .Where(x => x.ConfigType == EnumLessonConfigType.HomeWork
                            && x.HomeWorkId.HasValue)
                .Select(x => new
                {
                    LessonModuleId = x.Id,
                    OriginalId = x.OriginalId,
                    CurrentId = x.HomeWorkId!.Value
                })
                .ToList();

            if (buildModules.Count == 0)
            {
                return new List<Guid>();
            }

            var resultIdByLessonModuleId = (homeWorkResults ?? new List<HomeWorkResult>())
                            .Where(r => r.LessonModuleId.HasValue && r.LessonModuleId != Guid.Empty)
                            .GroupBy(r => r.LessonModuleId ?? Guid.Empty)
                            .ToDictionary(g => g.Key, g => g.First().HomeWorkId);

            var result = buildModules
                .GroupBy(x => x.OriginalId)
                .Select(g =>
                {
                    var idFromResult = g
                        .Select(m => resultIdByLessonModuleId.TryGetValue(m.LessonModuleId, out var vid) ? (Guid?)vid : null)
                        .FirstOrDefault(v => v.HasValue);

                    return idFromResult ?? g.First().CurrentId;
                })
                .Distinct()
                .ToList();

            return result;
        }

        public static List<Guid> ExtractIdsPreferResult(CourseBuildModel courseBuild, IList<ClassForumResult> classForumResults)
        {
            if (courseBuild?.CourseModules == null || courseBuild.CourseModules.Count == 0)
            {
                return new List<Guid>();
            }

            var buildModules = courseBuild.CourseModules
                .Where(x => x.ConfigType == EnumCourseConfigType.Unit)
                .SelectMany(x => x.UnitModuleBuilds)
                .Where(x => x.ConfigType == EnumUnitConfigType.Lesson)
                .SelectMany(x => x.LessonModuleBuilds)
                .Where(x => x.ConfigType == EnumLessonConfigType.ClassForum && x.ClassForumId.HasValue)
                .Select(x => new
                {
                    LessonModuleId = x.Id,
                    OriginalId = x.OriginalId,
                    CurrentId = x.ClassForumId!.Value
                })
                .ToList();

            if (buildModules.Count == 0)
            {
                return new List<Guid>();
            }

            var resultIdByLessonModuleId = (classForumResults ?? new List<ClassForumResult>())
                            .Where(r => r.LessonModuleId.HasValue && r.LessonModuleId != Guid.Empty)
                            .GroupBy(r => r.LessonModuleId ?? Guid.Empty)
                            .ToDictionary(g => g.Key, g => g.First().ClassForumId);

            var result = buildModules
                .GroupBy(x => x.OriginalId)
                .Select(g =>
                {
                    var idFromResult = g
                        .Select(m => resultIdByLessonModuleId.TryGetValue(m.LessonModuleId, out var vid) ? (Guid?)vid : null)
                        .FirstOrDefault(v => v.HasValue);

                    return idFromResult ?? g.First().CurrentId;
                })
                .Distinct()
                .ToList();

            return result;
        }

        public static IList<SkillScores> MergeSkillScores(
        IList<SkillScores> skillScores,
        IList<SkillScores> skillScoreResults)
        {
            skillScores ??= new List<SkillScores>();
            skillScoreResults ??= new List<SkillScores>();

            var resultBySkillId = skillScoreResults
                .Where(x => x.SkillId.HasValue && x.SkillId != Guid.Empty)
                .GroupBy(x => x.SkillId!.Value)
                .ToDictionary(g => g.Key, g => AggregateResult(g));

            var resultBySkillKey = skillScoreResults
                .Where(x => !x.SkillId.HasValue || x.SkillId == Guid.Empty)
                .GroupBy(x => new { x.Skill, x.SkillName })
                .ToDictionary(g => g.Key, g => AggregateResult(g));

            foreach (var master in skillScores)
            {
                SkillScores? r = null;

                if (master.SkillId.HasValue && master.SkillId != Guid.Empty
                    && resultBySkillId.TryGetValue(master.SkillId.Value, out var byId))
                {
                    r = byId;
                }
                else
                {
                    var key = new { master.Skill, master.SkillName };
                    if (resultBySkillKey.TryGetValue(key, out var byKey))
                    {
                        r = byKey;
                    }
                }

                if (r == null)
                {
                    // Không có result => set 0 (giữ Total)
                    master.CorrectCount = 0;
                    master.CorrectQuestion = 0;
                    master.CountQuestion = 0;
                    master.Percent = 0;
                    master.TokenReceived = 0;

                    continue;
                }

                // ✅ Map các field result vào master
                master.CorrectCount = r.CorrectCount;
                master.CorrectQuestion = r.CorrectQuestion;
                master.CountQuestion = r.CountQuestion;
                master.Percent = r.Percent;
                master.TokenReceived = r.TokenReceived;
            }

            return skillScores;
        }
    }
}
