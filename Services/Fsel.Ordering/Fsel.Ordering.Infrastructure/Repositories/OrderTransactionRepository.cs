// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Infrastructure.Repositories
{
    using AutoMapper;
    using Fsel.Core.Base;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Ordering.Domain.IRepositories;

    public class OrderTransactionRepository : BaseRepository<OrderTransaction>, IOrderTransactionRepository
    {
        public OrderTransactionRepository(OrderingDbContext dbContext, OrderingReadDbContext readDbContext, AuthContext authContext, IMapper mapper) : base(dbContext, readDbContext, authContext, mapper)
        {
        }
    }
}
