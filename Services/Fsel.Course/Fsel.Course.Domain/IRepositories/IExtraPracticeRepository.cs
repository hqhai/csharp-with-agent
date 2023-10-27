// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base.Interfaces;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Models.EntityModels;

namespace Fsel.Course.Domain.IRepositories
{
    public interface IExtraPracticeRepository : IRepository<ExtraPractice>
    {
        Task<ExtraPracticeModel?> GetIncludeAllAsync(Guid? id);

        Task<ExtraPractice?> GetIncludeSkillReadingAsync(Guid? id);

        Task<ExtraPractice?> GetIncludeByTypeVideoEmbedAsync(Guid? id);

        Task<ExtraPractice?> GetIncludeByPlacementTestAsync(Guid? id);

        Task<ExtraPractice?> GetIncludeByTypeInteractiveVideoAsync(Guid? id);

        Task<ExtraPractice?> GetIncludeByTypeBookAsync(Guid? id);
    }
}
