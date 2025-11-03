namespace Fsel.Ordering.Application.Commands.OrderCmds.V1i2
{
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Ordering.Application.Services.UserService;
    using Fsel.Ordering.Application.Services.UserService.Models;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Ordering.Domain.Enums;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.CommandModels.Orders.V1i2;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateOrderForStudentsEventCommand : CreateOrderForStudentsEventCommandModels, IRequest<MethodResult<bool>>
    {
    }

    public class CreateOrderForStudentsEventCommandHandler : IRequestHandler<CreateOrderForStudentsEventCommand, MethodResult<bool>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IPackageRepository _packageRepository;
        private readonly IUserService _userService;
        private readonly IEventRepository _eventRepository;
        private readonly IPackageEventRepository _packageEventRepository;

        private const string Letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        private const string Digits = "0123456789";

        public CreateOrderForStudentsEventCommandHandler(IOrderRepository orderRepository, IPackageRepository packageRepository, IUserService userService, IEventRepository eventRepository, IPackageEventRepository packageEventRepository)
        {
            _orderRepository = orderRepository;
            _packageRepository = packageRepository;
            _userService = userService;
            _eventRepository = eventRepository;
            _packageEventRepository = packageEventRepository;
        }

        public async Task<MethodResult<bool>> Handle(CreateOrderForStudentsEventCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var query = await (from e in _eventRepository.Queryable
                               join pe in _packageEventRepository.Queryable on e.Id equals pe.EventId
                               join p in _packageRepository.Queryable on pe.PackageId equals p.Id
                               select new
                               {
                                   Event = e,
                                   PackageEvent = pe,
                                   Package = p,
                               }).ToListAsync(cancellationToken);

            if (query == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumEventErrorCode.EventNotExist), nameof(query));
                return methodResult;
            }

            var packageEvent = query.OrderBy(p => p.Package.MonthNumber).ThenBy(p => p.PackageEvent.CreatedDate).FirstOrDefault()?.PackageEvent;

            if (packageEvent == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumEventErrorCode.MissingVersionOfPackage), nameof(packageEvent));
                return methodResult;
            }

            var orders = new List<Order>();

            request.Students.ForEach(p =>
            {
                var randomCode = GenerateRandomString();
                var code = randomCode + "_" + p.StudentCode;
                var newOrder = CreateOrder(p, packageEvent, request.ExpiredDate, code);
                orders.Add(newOrder);
            });

            var studentIds = request.Students.Select(p => p.StudentId).Distinct().ToList();

            await _orderRepository.ExecuteTransactionAsync(async () =>
            {
                await _orderRepository.BulkMergeAsync(orders);

                var result = await _userService.UpdateExpiredDateForStudentsEvent(new UpdateExpiredDateForStudentsEventCommandModel()
                {
                    StudentIds = studentIds,
                    ExpiredDate = request.ExpiredDate
                });
                if (!result.IsSuccessStatusCode)
                {
                    methodResult.AddError(result.Error);
                    return methodResult;
                }

                methodResult.StatusCode = StatusCodes.Status201Created;
                return methodResult;
            });

            return methodResult;
        }

        private static Order CreateOrder(CreateOrderForStudentsEventCommandModel student, PackageEvent packageEvent, DateTime expiredDate, string code)
        {
            return new Order()
            {
                UserId = student.UserId,
                FullName = student.FullName,
                PhoneNumber = student.PhoneNumber,
                PackageId = packageEvent.PackageId,
                EventId = packageEvent.EventId,
                Price = packageEvent.Price,
                Code = code,
                Email = string.IsNullOrEmpty(student.Email) ? student.PhoneNumber : student.Email,
                DiscountPercent = 100,
                DiscountPrice = packageEvent.Price,
                TotalPrice = 0,
                Status = EnumOrderStatus.Payment,
                ExpireDate = expiredDate,
                RevenueType = null,
            };
        }

        private static string GenerateRandomString()
        {
            Random random = new Random();
            StringBuilder result = new StringBuilder();

            for (int i = 0; i < 2; i++)
            {
                int index = random.Next(Letters.Length);
                result.Append(Letters[index]);
            }

            for (int i = 0; i < 1; i++)
            {
                int index = random.Next(Digits.Length);
                result.Append(Digits[index]);
            }

            char[] array = result.ToString().ToCharArray();
            Array.Sort(array, (x, y) => random.Next(-1, 2));

            return new string(array);
        }
    }
}
