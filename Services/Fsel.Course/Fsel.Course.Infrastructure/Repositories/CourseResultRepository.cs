// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Repositories
{
    using System.Collections.Generic;
    using System.Linq;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;

    public class CourseResultRepository : BaseRepository<CourseResult>, ICourseResultRepository
    {
        private readonly ICourseUnitMockTestRepository _courseUnitMockTestRepository;
        private readonly IFinalTestResultRepository _finalTestResultRepository;
        private readonly IUnitLessonRepository _unitLessonRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IUnitSkillMockTestRepository _unitSkillMockTestRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;

        public CourseResultRepository(ICourseUnitMockTestRepository courseUnitMockTestRepository,
            IFinalTestResultRepository finalTestResultRepository,
            IUnitLessonRepository unitLessonRepository,
            ILessonResultRepository lessonResultRepository,
            IUnitSkillMockTestRepository unitSkillMockTestRepository,
            IMockTestResultRepository mockTestResultRepository,
            CourseDbContext dbContext, AuthContext authContext, AutoMapper.IMapper mapper) : base(dbContext, authContext, mapper)
        {
            _courseUnitMockTestRepository = courseUnitMockTestRepository;
            _finalTestResultRepository = finalTestResultRepository;
            _unitLessonRepository = unitLessonRepository;
            _lessonResultRepository = lessonResultRepository;
            _unitSkillMockTestRepository = unitSkillMockTestRepository;
            _mockTestResultRepository = mockTestResultRepository;
        }

        //public async Task<IList<LearningProgressLearnModel>> GetStudyPositionAsync(EnumCourseType courseType)
        //{
        //    if (courseType == EnumCourseType.Academic)
        //    {
        //        var a = (from baseQ in Queryable

        //                 join courseUnitMockTest in _courseUnitMockTestRepository.Queryable on baseQ.CourseId equals courseUnitMockTest.CourseId

        //                 join ftr in _finalTestResultRepository.Queryable
        //                  on new { baseQ.StudentId, baseQ.CourseId, FinalTestId = courseUnitMockTest.FinalTestId } equals new { ftr.StudentId, ftr.CourseId, FinalTestId = (Guid?)ftr.FinalTestId } into ftrGroup
        //                 from ftr in ftrGroup.DefaultIfEmpty()

        //                 join ul in _unitLessonRepository.Queryable
        //                 on courseUnitMockTest.UnitId equals (Guid?)ul.UnitId into ulGroup
        //                 from ul in ulGroup.DefaultIfEmpty()

        //                 join lr in _lessonResultRepository.Queryable
        //                 on new { baseQ.StudentId, ul.UnitId, baseQ.CourseId, ul.LessonId } equals new { lr.StudentId, lr.UnitId, lr.CourseId, lr.LessonId } into lrGroup
        //                 from lr in lrGroup.DefaultIfEmpty()
        //                 where baseQ.WorkingStatus == EnumWorkingStatus.Active
        //                 group new { ul, courseUnitMockTest, ftr, lr } by new { DisplayOrder = (int?)ul.DisplayOrder, UnitDisplayOrder = courseUnitMockTest.DisplayOrder } into groupedData
        //                 select new LearningProgressLearnModel
        //                 {
        //                     DisplayOrder = groupedData.Key.DisplayOrder,
        //                     UnitDisplayOrder = groupedData.Key.UnitDisplayOrder,
        //                     StudentCount = groupedData.Count(g =>
        //                         (g.ftr != null && !new[] { EnumResultStatus.Done, EnumResultStatus.Unfinished }.Contains(g.ftr.Status)) ||
        //                         (g.lr != null && !new[] { EnumResultStatus.Done, EnumResultStatus.Unfinished }.Contains(g.lr.Status)))
        //                 }).OrderBy(x => x.UnitDisplayOrder).ThenBy(x => x.DisplayOrder).ToList();

        //        return a;
        //    }
        //    else if (courseType == EnumCourseType.Ielts)
        //    {
        //        var query = (from baseQ in Queryable
        //                     join cum in _courseUnitMockTestRepository.Queryable on baseQ.CourseId equals cum.CourseId

        //                     join ul in _unitLessonRepository.Queryable
        //                     on cum.UnitId equals (Guid?)ul.UnitId into ulGroup
        //                     from ul in ulGroup.DefaultIfEmpty()

        //                     join lr in _lessonResultRepository.Queryable
        //                     on new { baseQ.StudentId, ul.UnitId, baseQ.CourseId, ul.LessonId } equals new { lr.StudentId, lr.UnitId, lr.CourseId, lr.LessonId } into lrGroup
        //                     from lr in lrGroup.DefaultIfEmpty()

        //                     where
        //                         baseQ.WorkingStatus == EnumWorkingStatus.Active &&
        //                         cum.UnitId.HasValue &&
        //                         lr.Status == EnumResultStatus.Done
        //                     group lr by new { LessonId = (Guid?)ul.LessonId, DisplayOrder = (int?)cum.DisplayOrder } into g
        //                     select new LearningProgressLearnModel
        //                     {
        //                         FinalTestId = null,
        //                         FullMockTestId = null,
        //                         SkillMockTestId = null,
        //                         UnitDisplayOrder = 0,
        //                         LessonId = g.Key.LessonId,
        //                         DisplayOrder = g.Key.DisplayOrder,
        //                         StudentCount = g.Count(),
        //                     }).Concat(from baseQ in Queryable

        //                               join cum in _courseUnitMockTestRepository.Queryable on baseQ.CourseId equals cum.CourseId

        //                               join usm in _unitSkillMockTestRepository.Queryable
        //                               on new { UnitId = cum.UnitId } equals new { UnitId = (Guid?)usm.UnitId } into usmGroup
        //                               from usm in usmGroup.DefaultIfEmpty()

        //                               join mtrs in _mockTestResultRepository.Queryable
        //                               on new { baseQ.StudentId, UnitId = (Guid?)usm.UnitId, baseQ.CourseId, usm.MockTestId } equals new { mtrs.StudentId, UnitId = mtrs.UnitId, mtrs.CourseId, mtrs.MockTestId } into mtrsGroup
        //                               from mtrs in mtrsGroup.DefaultIfEmpty()

        //                               join mtr in _mockTestResultRepository.Queryable
        //                               on new { baseQ.StudentId, baseQ.CourseId, MockTestId = cum.MockTestId } equals new { mtr.StudentId, mtr.CourseId, MockTestId = (Guid?)mtr.MockTestId } into mtrGroup
        //                               from mtr in mtrGroup.DefaultIfEmpty()

        //                               where baseQ.WorkingStatus == EnumWorkingStatus.Active &&
        //                                    (mtr == null || mtr.Status == EnumResultStatus.Done) &&
        //                                    (mtrs == null || mtrs.Status == EnumResultStatus.Done)
        //                               group new { mtr, mtrs } by new { cum.MockTestId, usmMockTestId = (Guid?)usm.MockTestId, cum.DisplayOrder } into g
        //                               select new LearningProgressLearnModel
        //                               {
        //                                   LessonId = null,
        //                                   FinalTestId = null,
        //                                   UnitDisplayOrder = 0,
        //                                   FullMockTestId = g.Key.MockTestId,
        //                                   SkillMockTestId = g.Key.usmMockTestId,
        //                                   DisplayOrder = g.Key.DisplayOrder,
        //                                   StudentCount = g.Count(),
        //                               }).AsEnumerable();

        //        var combinedResults = query.OrderBy(r => r.DisplayOrder)
        //                                   .ThenByDescending(r => r.LessonId)
        //                                   .ThenBy(r => r.SkillMockTestId)
        //                                   .ThenByDescending(r => r.StudentCount)
        //                                   .ToList();
        //    }
        //    return new List<LearningProgressLearnModel>();
        //}
    }
}
