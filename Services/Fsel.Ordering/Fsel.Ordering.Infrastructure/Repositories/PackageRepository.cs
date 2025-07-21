// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Infrastructure.Repositories
{
    using Fsel.Core.Base;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Ordering.Domain.IRepositories;

    public class PackageRepository : BaseRepository<Package>, IPackageRepository
    {
        private readonly IEventRepository _eventRepository;

        public PackageRepository(OrderingDbContext dbContext, OrderingReadDbContext orderingReadDb, AuthContext authContext, AutoMapper.IMapper mapper, IEventRepository eventRepository)
            : base(dbContext, orderingReadDb, authContext, mapper)
        {
            _eventRepository = eventRepository;
        }

        //public override async Task<Package?> GetByIdAsync(Guid id)
        //{
        //    var package = await Queryable.FirstOrDefaultAsync(x => x.Id == id);
        //    if (package == null)
        //    {
        //        return null;
        //    }
        //    var @event = await _eventRepository.Queryable.Include(p => p.PackageEvents).FirstOrDefaultAsync(p => p.Status == EnumEventPackageStatus.Active);
        //    if (@event != null)
        //    {
        //        var packageEvent = @event.PackageEvents.FirstOrDefault(p => p.PackageId == package.Id);
        //        if (packageEvent != null)
        //        {
        //            package.Price = packageEvent.Price;
        //        }
        //    }
        //    return package;
        //}
    }
}
