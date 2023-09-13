// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.OrderQuery
{
    using System;
    using System.Globalization;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Ordering.Application.Services.UserService;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GenerateRamdomOrderQuery : IRequest<MethodResult<GenerateRamdomOrderModel>>
    {
        public Guid PackageId { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public Guid UserId { get; set; }
    }

    public class GetOrderQueryHandler : IRequestHandler<GenerateRamdomOrderQuery, MethodResult<GenerateRamdomOrderModel>>
    {
        private readonly IPackageRepository _packageRepository;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public GetOrderQueryHandler(IPackageRepository packageRepository,
            IMapper mapper,
            IUserService userService)
        {
            _packageRepository = packageRepository;
            _mapper = mapper;
            _userService = userService;
        }

        public async Task<MethodResult<GenerateRamdomOrderModel>> Handle(GenerateRamdomOrderQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<GenerateRamdomOrderModel> methodResult = new MethodResult<GenerateRamdomOrderModel>();

            GenerateRamdomOrderModel order = new GenerateRamdomOrderModel();
            var package = await _packageRepository.Queryable.FirstOrDefaultAsync(x => x.Id == request.PackageId, cancellationToken);
            if (package == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(package));
                return methodResult;
            }
            var student = await _userService.GetStudentByUserIdAsync(request.UserId);
            if (!student.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError));
                return methodResult;
            }

            CultureInfo culture = new CultureInfo("en-US");
            string formattedDate = DateTime.Now.ToString("ddMMyyyy", culture);
            var code = $"{request.CourseLevel.GetEnumCourseType()}{formattedDate}{package.Code.ToString()!.Substring(0, 1)}{student.Content?.Result?.Human?.Code}";
            order.Code = code;
            order.Package = _mapper.Map<PackageModel>(package);

            methodResult.Result = order;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
