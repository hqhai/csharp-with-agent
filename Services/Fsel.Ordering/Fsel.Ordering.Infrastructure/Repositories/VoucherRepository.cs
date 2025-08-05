// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Infrastructure.Repositories
{
    using Fsel.Core.Base;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Ordering.Domain.IRepositories;

    public class VoucherRepository : BaseRepository<Voucher>, IVoucherRepository
    {
        public VoucherRepository(OrderingDbContext dbContext, OrderingReadDbContext readDbContext, AuthContext authContext, AutoMapper.IMapper mapper) : base(dbContext, readDbContext, authContext, mapper)
        {
        }

        public override IQueryable<Voucher> Queryable => _dbSet.AsQueryable();
    }
}
