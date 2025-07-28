// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Infrastructure.Repositories
{
    using Fsel.Core.Base;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Ordering.Domain.IRepositories;

    public class OrderRepository : BaseRepository<Order>, IOrderRepository
    {
        public OrderRepository(OrderingDbContext dbContext, OrderingReadDbContext readDbContext, AuthContext authContext, AutoMapper.IMapper mapper) : base(dbContext, readDbContext, authContext, mapper)
        {
        }
    }
}
