// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace Fsel.Course.Infrastructure.Repositories
{
    public class PlacementTestRepository : BaseRepository<PlacementTest>, IPlacementTestRepository
    {
        public PlacementTestRepository(CourseDbContext dbContext, CourseReadDbContext readDbContext, AuthContext authContext, IMapper mapper)
            : base(dbContext, readDbContext, authContext, mapper)
        {
        }

        public override async Task<PlacementTest?> GetIncludeByIdAsync(Guid id)
        {
            try
            {
                var query = await Queryable.FirstOrDefaultAsync(x => x.Id == id);

                if (query != null && query.PlacementTestLevel == EnumPlacementTestLevel.IELTS)
                {
                    query = await Queryable.Include(x => x.ExtraPractice)
                                .Include(x => x.PlacementTestSections.Where(n => n.SectionGroup != null))
                                .ThenInclude(x => x.SectionGroup)
                                .ThenInclude(x => x!.Sections)
                                .ThenInclude(x => x.SectionParts)
                                .ThenInclude(x => x.SectionQuestions.Where(n => n.Question != null))
                                .ThenInclude(x => x.Question)
                                .Include(x => x.ExtraPractice)
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
                                .Include(x => x.ExtraPractice)
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
                if (query != null && query.PlacementTestLevel == EnumPlacementTestLevel.IELTS)
                {
                    placement = await Queryable.Include(x => x.PlacementTestSections.Where(y => !y.IsDeleted))
                                    .ThenInclude(x => x.SectionGroup)
                                    .ThenInclude(x => x!.Sections.Where(y => !y.IsDeleted))
                                    .ThenInclude(x => x.SectionParts.Where(y => !y.IsDeleted))
                                    .ThenInclude(x => x.SectionQuestions.Where(y => !y.IsDeleted))
                                    .ThenInclude(x => x.Question)
                                    .Where(x => x.Id == id)
                                    .Select(x => new PlacementTestModel
                                    {
                                        Id = x.Id,
                                        Name = x.Name,
                                        Level = x.PlacementTestLevel,
                                        CreatedDate = x.CreatedDate,
                                        IsActive = x.IsActive,
                                        SectionGroups = x.PlacementTestSections.Select(x => x.SectionGroup).OrderBy(x => x!.CreatedDate).Select(x => new SectionGroupModel
                                        {
                                            Id = x!.Id,
                                            ExecutionTime = x.ExecutionTime,
                                            CourseSkill = x.CourseSkill,
                                            SkillId = x.SkillId,
                                            SkillName = x.Skill != null ? x.Skill.Name : null,
                                            Sections = x.Sections.OrderBy(x => x.DisplayOrder).Select(x => new SectionModel
                                            {
                                                Id = x.Id,
                                                Name = x.Name,
                                                MediaPost = x.MediaPost,
                                                TargetWord = x.TargetWord,
                                                DisplayOrder = x.DisplayOrder,
                                                VideoFilePath = x.VideoFilePath,
                                                SectionParts = x.SectionParts.OrderBy(x => x!.CreatedDate).Select(x => new SectionPartModel
                                                {
                                                    Id = x.Id,
                                                    PartName = x.PartName,
                                                    SectionId = x.SectionId,
                                                    Questions = x.SectionQuestions.Select(x => x.Question).OrderBy(x => x!.CreatedDate).Select(x => new QuestionModel
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
                                   .ThenInclude(x => x!.Sections.Where(y => !y.IsDeleted))
                                   .ThenInclude(x => x.SectionParts.Where(y => !y.IsDeleted))
                                   .ThenInclude(x => x.SectionQuestions.Where(y => !y.IsDeleted))
                                   .ThenInclude(x => x.Question)
                                   .Where(x => x.Id == id)
                                   .Select(x => new PlacementTestModel
                                   {
                                       Id = x.Id,
                                       Name = x.Name,
                                       Level = x.PlacementTestLevel,
                                       CreatedDate = x.CreatedDate,
                                       IsActive = x.IsActive,
                                       SectionGroups = x.PlacementTestSections.Select(x => x.SectionGroup).OrderBy(x => x!.CreatedDate).Select(x => new SectionGroupModel
                                       {
                                           Id = x!.Id,
                                           ExecutionTime = x!.ExecutionTime,
                                           CourseSkill = x.CourseSkill,
                                           Sections = x.Sections.OrderBy(x => x!.DisplayOrder).Select(x => new SectionModel
                                           {
                                               Id = x.Id,
                                               Name = x.Name,
                                               MediaPost = x.MediaPost,
                                               VideoFilePath = x.VideoFilePath,
                                               DisplayOrder = x.DisplayOrder,
                                               TargetWord = x.TargetWord,
                                               Questions = x.SectionQuestions.Select(x => x.Question).OrderBy(x => x!.CreatedDate).Select(x => new QuestionModel
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
