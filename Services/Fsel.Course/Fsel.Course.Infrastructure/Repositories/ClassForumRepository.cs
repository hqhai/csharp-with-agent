// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.Enums;
using Fsel.Core.Base;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Entities.V1i1;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Infrastructure.Repositories
{
    public class ClassForumRepository : BaseRepository<ClassForum>, IClassForumRepository
    {
        private readonly IClassForumResultRepository _classForumResultRepository;

        public ClassForumRepository(CourseDbContext dbContext,
            CourseReadDbContext readDbContext,
            AuthContext authContext,
            AutoMapper.IMapper mapper,
            IClassForumResultRepository classForumResultRepository)
            : base(dbContext, readDbContext, authContext, mapper)
        {
            _classForumResultRepository = classForumResultRepository;
        }

        public async Task<IDictionary<Guid, ClassForum>> GetClassForumDicAsync(IList<Guid>? originalIds)
        {
            if (originalIds == null || originalIds.Count == 0)
            {
                return new Dictionary<Guid, ClassForum>();
            }

            var classForums = await ReadQueryable.WhereBulkContains(originalIds, x => x.OriginalId)
                                               .Where(x => x.VersionStatus == EnumVersionStatus.LastVersion)
                                               .ToListAsync();

            return classForums.ToDictionary(x => x.OriginalId);
        }

        public async Task<(IDictionary<Guid, (ClassForum, ClassForumResult)>, IDictionary<Guid, ClassForum>)>
        BuildClassForumLookupsAsync(LessonResult? lessonResult, IList<LessonModule> lessonModules)
        {
            var classForumOriginalIds = lessonModules
                .Where(x => x.LessonConfigType == EnumLessonConfigType.ClassForum)
                .Select(x => x.OriginalId)
                .Distinct()
                .ToList();

            if (!classForumOriginalIds.Any())
            {
                return (new Dictionary<Guid, (ClassForum, ClassForumResult)>(),
                        new Dictionary<Guid, ClassForum>());
            }

            var classForumResultsByOriginalId = new Dictionary<Guid, (ClassForum, ClassForumResult)>();
            var pendingClassForumOriginalIds = new List<Guid>();
            if (lessonResult != null)
            {
                var classForumResults = await (from baseQ in _classForumResultRepository.ReadQueryable
                                               where baseQ.LessonResultId == lessonResult.Id
                                               join classForum in ReadQueryable
                                                   on baseQ.ClassForumId equals classForum.Id
                                               select new
                                               {
                                                   ClassForum = classForum,
                                                   ClassForumResult = baseQ
                                               }).ToListAsync();

                var classForumOriginalIdsHasResult = classForumResults
                    .Where(x => x.ClassForum != null)
                    .Select(x => x.ClassForum!.OriginalId)
                    .Distinct();

                pendingClassForumOriginalIds = classForumOriginalIds
                   .Except(classForumOriginalIdsHasResult)
                   .ToList();

                classForumResultsByOriginalId = classForumResults
                       .Where(x => x.ClassForum != null)
                       .ToDictionary(
                           x => x.ClassForum!.OriginalId,
                           x => (ClassForum: x.ClassForum!, ClassForumResult: x.ClassForumResult));
            }
            else
            {
                pendingClassForumOriginalIds = classForumOriginalIds;
            }

            var classForumDics = await GetClassForumDicAsync(pendingClassForumOriginalIds);

            return (classForumResultsByOriginalId, classForumDics);
        }
    }
}
