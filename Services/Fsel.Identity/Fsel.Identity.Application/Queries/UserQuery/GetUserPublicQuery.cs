// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.UserQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Application.Services.OrderService;
    using Fsel.Identity.Application.Services.OrderService.QueryModels;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetUserPublicQuery : IRequest<MethodResult<UserPublicModel>>
    {
        public Guid? Id { get; set; }
    }

    public class GetUserPublicQueryHandler : IRequestHandler<GetUserPublicQuery, MethodResult<UserPublicModel>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IHumanRepository _humanRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IOrderService _orderService;

        public GetUserPublicQueryHandler(UserManager<User> userManager,
            IHumanRepository humanRepository,
            IStudentRepository studentRepository,
            IOrderService orderService)
        {
            _userManager = userManager;
            _humanRepository = humanRepository;
            _studentRepository = studentRepository;
            _orderService = orderService;
        }

        public async Task<MethodResult<UserPublicModel>> Handle(GetUserPublicQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<UserPublicModel> methodResult = new MethodResult<UserPublicModel>();
            var user = await (from u in _userManager.Users
                              join h in _humanRepository.Queryable on u.Id equals h.UserId
                              join s in _studentRepository.Queryable on h.Id equals s.HumanId
                              where u.Id == request.Id
                              select new
                              {
                                  User = u,
                                  Human = h,
                                  Student = s
                              }).FirstOrDefaultAsync(cancellationToken);
            if (user == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(user));
                return methodResult;
            }
            var orderResult = await _orderService.GetCurrentByUserIdAsync(new GetCurrentOrderByUserIdQueryModel
            {
                UserId = user.User.Id
            });
            var order = orderResult.Content?.Result;
            methodResult.Result = new UserPublicModel
            {
                Id = user.User.Id,
                FullName = user.User.FullName,
                PhoneNumber = user.User.PhoneNumber,
                Email = user.User.Email,
                ExpiredDate = user.Student.ExpiredDate,
                RevenueType = order?.RevenueType
            };
            return methodResult;
        }
    }
}
