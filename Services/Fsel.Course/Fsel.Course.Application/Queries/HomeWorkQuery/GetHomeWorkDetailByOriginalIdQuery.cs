// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.HomeWorkQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetHomeWorkDetailByOriginalIdQuery : IRequest<MethodResult<HomeWorkModel>>
    {
        public Guid OriginalId { get; set; }
    }

    public class GetHomeWorkDetailByOriginalIdQueryHandler : IRequestHandler<GetHomeWorkDetailByOriginalIdQuery, MethodResult<HomeWorkModel>>
    {
        private readonly IHomeWorkRepository _homeWorkRepository;
        private readonly IMapper _mapper;
        private readonly ILessonModuleRepository _lessonModuleRepository;

        public GetHomeWorkDetailByOriginalIdQueryHandler(IHomeWorkRepository homeWorkRepository,
                                                IMapper mapper,
                                                ILessonModuleRepository lessonModuleRepository)
        {
            _homeWorkRepository = homeWorkRepository;
            _mapper = mapper;
            _lessonModuleRepository = lessonModuleRepository;
        }

        public async Task<MethodResult<HomeWorkModel>> Handle(GetHomeWorkDetailByOriginalIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<HomeWorkModel> methodResult = new MethodResult<HomeWorkModel>();

            var homeWork = await _homeWorkRepository.Queryable
                                                    .Where(x => x.OriginalId == request.OriginalId && x.VersionStatus == Common.Enums.EnumVersionStatus.LastVersion)
                                                    .AsNoTracking()
                                                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);
            if (homeWork == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(homeWork));
                return methodResult;
            }

            var homeWorkModel = await _homeWorkRepository.GetIncludeAllAsync(homeWork.Id);
            if (homeWorkModel == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(homeWork));
                return methodResult;
            }

            var isActive = await _lessonModuleRepository.Queryable.Where(p => p.OriginalId == homeWork.OriginalId).Where(p => p.LessonConfigType == EnumLessonConfigType.HomeWork)
                                                                       .AnyAsync(cancellationToken);
            homeWorkModel.IsActive = isActive;
            methodResult.Result = homeWorkModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
