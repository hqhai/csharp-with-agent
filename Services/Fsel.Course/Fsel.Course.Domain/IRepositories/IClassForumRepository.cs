// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base.Interfaces;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Entities.V1i1;

namespace Fsel.Course.Domain.IRepositories
{
    public interface IClassForumRepository : IRepository<ClassForum>
    {
        Task<IDictionary<Guid, ClassForum>> GetClassForumDicAsync(IList<Guid>? originalIds);

        Task<(IDictionary<Guid, (ClassForum, ClassForumResult)>, IDictionary<Guid, ClassForum>)> BuildClassForumLookupsAsync(LessonResult lessonResult, IList<LessonModule> lessonModules);
    }
}
