// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Infrastructure.Repositories
{
    public class PlacementTestRepository : BaseRepository<PlacementTest>, IPlacementTestRepository
    {
        public PlacementTestRepository(CourseDbContext dbContext, AuthContext authContext) : base(dbContext, authContext)
        {
        }

        public override async Task<PlacementTest?> GetIncludeByIdAsync(Guid id, int? siteId = null)
        {
            try
            {
                var query = await Queryable.FirstOrDefaultAsync(x => x.Id == id);

                if (query != null && query.Level == EnumPlacementTestLevel.IELTS)
                {
                    query = await Queryable.Include(x => x.PlacementTestSections.Where(n => n.SectionGroup != null))
                                .ThenInclude(x => x.SectionGroup)
                                .ThenInclude(x => x!.Sections)
                                .ThenInclude(x => x.SectionParts)
                                .ThenInclude(x => x.SectionQuestions.Where(n => n.Question != null))
                                .ThenInclude(x => x.Question)
                                .FirstOrDefaultAsync(x => x.Id == id);
                }
                else
                {
                    query = await Queryable
                                .Include(x => x.PlacementTestSections.Where(n => n.SectionGroup != null))
                                .ThenInclude(x => x.SectionGroup)
                                .ThenInclude(x => x!.Sections)
                                .ThenInclude(x => x.SectionQuestions.Where(n => n.Question != null))
                                .ThenInclude(x => x.Question)
                                .FirstOrDefaultAsync(x => x.Id == id);
                }

                return query;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<PlacementTestModel?> GetIncludePlacementTestById(Guid id)
        {
            try
            {
                var query = await Queryable.FirstOrDefaultAsync(x => x.Id == id);

                PlacementTestModel? placement = null;
                if (query != null && query.Level == EnumPlacementTestLevel.IELTS)
                {
                    placement = await Queryable.Include(x => x.PlacementTestSections.Where(y => !y.IsDeleted))
                                    .ThenInclude(x => x.SectionGroup)
                                    .ThenInclude(x => x!.Sections)
                                    .ThenInclude(x => x.SectionParts)
                                    .ThenInclude(x => x.SectionQuestions)
                                    .Where(x => x.Id == id)
                                    .Select(x => new PlacementTestModel
                                    {
                                        Id = x.Id,
                                        Name = x.Name,
                                        Level = x.Level,
                                        CreatedDate = x.CreatedDate,
                                        IsActive = x.IsActive,
                                        SectionGroups = x.PlacementTestSections.Select(x => x.SectionGroup).Select(x => new SectionGroupModel
                                        {
                                            Id = x!.Id,
                                            ExecutionTime = x!.ExecutionTime,
                                            CourseSkill = x.CourseSkill,
                                            Sections = x.Sections.Select(x => new SectionModel
                                            {
                                                Id = x.Id,
                                                Name = x.Name,
                                                MediaPost = x.MediaPost,
                                                TargetWord = x.TargetWord,
                                                CreatedDate = x.CreatedDate,
                                                CreatedUserId = x.CreatedUserId,
                                                SectionParts = x.SectionParts.Select(x => new SectionPartModel
                                                {
                                                    Id = x.Id,
                                                    CreatedDate = x.CreatedDate,
                                                    PartName = x.PartName,
                                                    SectionId = x.SectionId,
                                                    CreatedFullName = x.CreatedFullName,
                                                    Question = x.SectionQuestions.Select(x => x.Question).Select(x => new QuestionModel
                                                    {
                                                        Id = x!.Id,
                                                        QuestionType = x.QuestionType,
                                                        Explanation = x.Explanation,
                                                        Ungraded = x.Ungraded,
                                                        CorrectTotal = x.CorrectTotal,
                                                        Config = x.Config
                                                    }).ToList()
                                                }).ToList(),
                                            }).ToList(),
                                        }).ToList(),
                                    }).FirstOrDefaultAsync();
                }
                else
                {
                    placement = await Queryable.Include(x => x.PlacementTestSections.Where(y => !y.IsDeleted))
                                   .ThenInclude(x => x.SectionGroup)
                                   .ThenInclude(x => x!.Sections)
                                   .ThenInclude(x => x.SectionParts)
                                   .ThenInclude(x => x.SectionQuestions)
                                   .Where(x => x.Id == id)
                                   .Select(x => new PlacementTestModel
                                   {
                                       Id = x.Id,
                                       Name = x.Name,
                                       Level = x.Level,
                                       CreatedDate = x.CreatedDate,
                                       IsActive = x.IsActive,
                                       SectionGroups = x.PlacementTestSections.Select(x => x.SectionGroup).Select(x => new SectionGroupModel
                                       {
                                           Id = x!.Id,
                                           ExecutionTime = x!.ExecutionTime,
                                           CourseSkill = x.CourseSkill,
                                           Sections = x.Sections.Select(x => new SectionModel
                                           {
                                               Id = x.Id,
                                               Name = x.Name,
                                               MediaPost = x.MediaPost,
                                               TargetWord = x.TargetWord,
                                               CreatedDate = x.CreatedDate,
                                               CreatedUserId = x.CreatedUserId,
                                               Questions = x.SectionQuestions.Select(x => x.Question).Select(x => new QuestionModel
                                               {
                                                   Id = x!.Id,
                                                   QuestionType = x.QuestionType,
                                                   Explanation = x.Explanation,
                                                   Ungraded = x.Ungraded,
                                                   CorrectTotal = x.CorrectTotal,
                                                   Config = x.Config
                                               }).ToList()
                                           }).ToList(),
                                       }).ToList(),
                                   }).FirstOrDefaultAsync();
                }

                return placement;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
