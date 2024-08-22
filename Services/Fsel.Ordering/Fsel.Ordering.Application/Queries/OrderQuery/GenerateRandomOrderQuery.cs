// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.OrderQuery
{
    using System;
    using System.Text;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Ordering.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GenerateRandomOrderQuery : IRequest<MethodResult<string>>
    {
        public string? StudentCode { get; set; }
    }

    public class GenerateRandomOrderQueryHandler : IRequestHandler<GenerateRandomOrderQuery, MethodResult<string>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly AuthContext _authContext;
        private readonly IMediator _mediator;

        public GenerateRandomOrderQueryHandler(IOrderRepository orderRepository, AuthContext authContext, IMediator mediator)
        {
            _orderRepository = orderRepository;
            _authContext = authContext;
            _mediator = mediator;
        }

        public async Task<MethodResult<string>> Handle(GenerateRandomOrderQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<string>();
            if (string.IsNullOrEmpty(request.StudentCode))
            {
                methodResult.Result = string.Empty;
                return methodResult;
            }

            var randomCode = GenerateRandomString();
            var code = randomCode + "_" + request.StudentCode;
            if (await _orderRepository.Queryable.AnyAsync(p => p.UserId == _authContext.CurrentUserId && p.Code == code, cancellationToken))
            {
                var codeResult = await _mediator.Send(new GenerateRandomOrderQuery() { StudentCode = request.StudentCode }, cancellationToken);
                code = codeResult.Result;
            }

            methodResult.Result = code;
            methodResult.StatusCode = StatusCodes.Status200OK;
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
