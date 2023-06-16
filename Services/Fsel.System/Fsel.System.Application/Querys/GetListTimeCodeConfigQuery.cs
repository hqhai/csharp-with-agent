// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Querys
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models;
    using global::System;
    using global::System.Threading.Tasks;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetListTimeCodeConfigQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<CourseTimeConfigModel>>>
    {
    }

    public class GetListTimeCodeConfigQueryHandler : IRequestHandler<GetListTimeCodeConfigQuery, MethodResult<PagingItemsModel<CourseTimeConfigModel>>>
    {
        private readonly ICourseTimeConfigRepository _courseTimeConfigRepository;
        private readonly IMapper _mapper;

        public GetListTimeCodeConfigQueryHandler(ICourseTimeConfigRepository courseTimeConfigRepository, IMapper mapper)
        {
            _courseTimeConfigRepository = courseTimeConfigRepository;
            _mapper = mapper;
        }

        public Task<MethodResult<PagingItemsModel<CourseTimeConfigModel>>> Handle(GetListTimeCodeConfigQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var methodResult = new MethodResult<PagingItemsModel<CourseTimeConfigModel>>();
            methodResult.Result = new PagingItemsModel<CourseTimeConfigModel>();

            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
        }
    }
}
