// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents
{
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Microsoft.EntityFrameworkCore;

    public class BaseInternalEventHandler
    {
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly IHomeWorkResultRepository _homeWorkResultRepository;

        public BaseInternalEventHandler(IVideoResultRepository videoResultRepository,
            IClassForumResultRepository classForumResultRepository,
            IHomeWorkResultRepository homeWorkResultRepository)
        {
            _videoResultRepository = videoResultRepository;
            _classForumResultRepository = classForumResultRepository;
            _homeWorkResultRepository = homeWorkResultRepository;
        }

        #region Get Skill Scores

        public async Task<List<SkillScores>> VideoSkillScores(Guid? lessonResultid)
        {
            ArgumentNullException.ThrowIfNull(lessonResultid);
            List<SkillScores> skillScores = new List<SkillScores>();
            var videoResult = await _videoResultRepository.Queryable.FirstOrDefaultAsync(x => x.Status == EnumResultStatus.Done && x.LessonResultId == lessonResultid);
            if (videoResult != null && videoResult.VideoSkillScores != null)
            {
                skillScores = videoResult.VideoSkillScores.Where(x => x.Type == EnumTimeCodeType.Standalone).SelectMany(x => x.SkillScores!).Where(x => x.TotalCount != 0).ToList();
            }
            return skillScores;
        }

        public async Task<List<SkillScores>> UnitTestSkillScores(Guid? lessonResultid)
        {
            ArgumentNullException.ThrowIfNull(lessonResultid);
            List<SkillScores> skillScores = new List<SkillScores>();
            var videoResult = await _videoResultRepository.Queryable.FirstOrDefaultAsync(x => x.Status == EnumResultStatus.Done && x.LessonResultId == lessonResultid);
            if (videoResult != null && videoResult.VideoSkillScores != null)
            {
                skillScores = videoResult.VideoSkillScores.Where(x => x.Type == EnumTimeCodeType.UnitTest).SelectMany(x => x.SkillScores!).Where(x => x.TotalCount != 0).ToList();
            }
            return skillScores;
        }

        public async Task<List<SkillScores>> SkillTestSkillScores(Guid? lessonResultid)
        {
            ArgumentNullException.ThrowIfNull(lessonResultid);
            List<SkillScores> skillScores = new List<SkillScores>();
            var videoResult = await _videoResultRepository.Queryable.FirstOrDefaultAsync(x => x.Status == EnumResultStatus.Done && x.LessonResultId == lessonResultid);
            if (videoResult != null && videoResult.VideoSkillScores != null)
            {
                skillScores = videoResult.VideoSkillScores.Where(x => x.Type == EnumTimeCodeType.SkillTest).SelectMany(x => x.SkillScores!).Where(x => x.TotalCount != 0).ToList();
            }
            return skillScores;
        }

        public async Task<SkillScores> ClassForumSkillScores(Guid? lessonResultid)
        {
            ArgumentNullException.ThrowIfNull(lessonResultid);
            SkillScores skillScores = new SkillScores();
            var classForumResult = await _classForumResultRepository.Queryable.Include(x => x.ClassForum).Include(x => x.ClassForumScores).FirstOrDefaultAsync(x => x.Status == EnumClassForumResultStatus.Graded && x.LessonResultId == lessonResultid);
            if (classForumResult != null && classForumResult.ClassForumScores != null && classForumResult.ClassForum != null)
            {
                skillScores.Skill = classForumResult.ClassForum.CourseSkill;
                skillScores.TotalCount = 36;
                skillScores.CorrectCount = classForumResult.ClassForumScores.Sum(x => x.Score);
            }
            return skillScores;
        }

        public async Task<List<SkillScores>> HomeWordsSkillScores(Guid? lessonResultid)
        {
            ArgumentNullException.ThrowIfNull(lessonResultid);
            List<SkillScores> skillScores = new List<SkillScores>();
            var homeWorkResults = await _homeWorkResultRepository.Queryable.Where(x => x.Status == EnumResultStatus.Done && x.LessonResultId == lessonResultid).ToArrayAsync();
            if (homeWorkResults != null)
            {
                foreach (var item in homeWorkResults)
                {
                    if (item.SkillScores != null)
                    {
                        skillScores.AddRange(item.SkillScores.Where(x => x.TotalCount != 0 && x.CorrectCount != 0).ToList());
                    }
                }
            }
            return skillScores;
        }

        #endregion Get Skill Scores

        #region Tinh Diem Unit

        public async Task<double> PercentUnit(IList<SkillScores>? skillScores, int percentSkill)
        {
            ArgumentNullException.ThrowIfNull(skillScores);
            double percent = 0;
            List<SkillScores> unitkillScores = skillScores
                                     .GroupBy(x => x.Skill)
                                     .Select(group => new SkillScores
                                     {
                                         Skill = group.Key,
                                         Scores = group.Sum(x => x.Scores),
                                         TotalCount = group.Sum(x => x.TotalCount),
                                         CorrectCount = group.Sum(x => x.CorrectCount)
                                     })
                                     .ToList();
            int dem = unitkillScores.Count;
            foreach (var item in unitkillScores)
            {
                percent += item.TotalCount == 0 ? 0 : (item.CorrectCount / item.TotalCount) * (percentSkill / dem);
            }
            return percent;
        }

        #endregion Tinh Diem Unit
    }
}
