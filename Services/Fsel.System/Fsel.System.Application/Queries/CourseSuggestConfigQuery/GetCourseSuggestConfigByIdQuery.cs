// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.CourseSuggestConfigQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetCourseSuggestConfigByIdQuery : IRequest<MethodResult<CourseSuggestConfigModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetCourseSuggestConfigByIdQueryHandler : IRequestHandler<GetCourseSuggestConfigByIdQuery, MethodResult<CourseSuggestConfigModel>>
    {
        private readonly ICourseSuggestConfigRepository _courseSuggestConfigRepository;
        private readonly IMapper _mapper;

        public GetCourseSuggestConfigByIdQueryHandler(ICourseSuggestConfigRepository courseSuggestConfigRepository,
                                                      IMapper mapper)
        {
            _courseSuggestConfigRepository = courseSuggestConfigRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<CourseSuggestConfigModel>> Handle(GetCourseSuggestConfigByIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<CourseSuggestConfigModel> methodResult = new MethodResult<CourseSuggestConfigModel>();

            var courseSuggestConfig = await _courseSuggestConfigRepository.Queryable.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (courseSuggestConfig == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Id), nameof(request.Id));
                return methodResult;
            }

            methodResult.Result = _mapper.Map<CourseSuggestConfigModel>(courseSuggestConfig);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
