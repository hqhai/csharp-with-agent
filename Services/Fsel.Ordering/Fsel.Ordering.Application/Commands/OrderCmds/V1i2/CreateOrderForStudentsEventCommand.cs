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

        public CreateOrderForStudentsEventCommandHandler(IOrderRepository orderRepository, IPackageRepository packageRepository, IUserService userService, IEventRepository eventRepository)
        {
            _orderRepository = orderRepository;
            _packageRepository = packageRepository;
            _userService = userService;
            _eventRepository = eventRepository;
        }

        public async Task<MethodResult<bool>> Handle(CreateOrderForStudentsEventCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var package = await _packageRepository.Queryable.OrderBy(p => p.MonthNumber).FirstOrDefaultAsync(cancellationToken);

            if (package == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(package));
                return methodResult;
            }

            var currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);

            var @event = await _eventRepository.Queryable.Include(p => p.PackageEvents).FirstOrDefaultAsync(p => p.IsDefault, cancellationToken);

            if (@event == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumEventErrorCode.EventNotExist), nameof(@event));
                return methodResult;
            }

            var packageEvent = @event.PackageEvents.FirstOrDefault(p => p.PackageId == package.Id);

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
                var newOrder = new Order()
                {
                    UserId = p.UserId,
                    FullName = p.FullName,
                    PhoneNumber = p.PhoneNumber,
                    PackageId = package.Id,
                    EventId = @event.Id,
                    Price = packageEvent.Price,
                    Code = code,
                    Email = string.IsNullOrEmpty(p.Email) ? p.PhoneNumber : p.Email,
                    DiscountPercent = 0,
                    DiscountPrice = 0,
                    TotalPrice = packageEvent.Price,
                    Status = EnumOrderStatus.Payment,
                    ExpireDate = request.ExpiredDate,
                    RevenueType = null,
                };
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

        private static string GenerateRandomString()
        {
            // Các ký tự chữ cái
            const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            // Các ký tự số
            const string digits = "0123456789";

            // Sử dụng Random để tạo ngẫu nhiên
            Random random = new Random();
            StringBuilder result = new StringBuilder();

            // Tạo ngẫu nhiên các chữ cái
            for (int i = 0; i < 2; i++)
            {
                int index = random.Next(letters.Length);
                result.Append(letters[index]);
            }

            // Tạo ngẫu nhiên các chữ số
            for (int i = 0; i < 1; i++)
            {
                int index = random.Next(digits.Length);
                result.Append(digits[index]);
            }

            // Trộn các ký tự ngẫu nhiên
            char[] array = result.ToString().ToCharArray();
            Array.Sort(array, (x, y) => random.Next(-1, 2));

            return new string(array);
        }
    }
}
