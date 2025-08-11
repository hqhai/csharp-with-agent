// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.HomeWorkQuery
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetHomeWorkByOriginalIdQuery : IRequest<MethodResult<HomeWorkModel>>
    {
        public Guid OriginalId { get; set; }
    }

    public class GetHomeWorkByOriginalIdQueryHandler : IRequestHandler<GetHomeWorkByOriginalIdQuery, MethodResult<HomeWorkModel>>
    {
        private readonly IHomeWorkRepository _homeWorkRepository;
        private readonly IMapper _mapper;
        private readonly ILessonModuleRepository _lessonModuleRepository;

        public GetHomeWorkByOriginalIdQueryHandler(IHomeWorkRepository homeWorkRepository,
                                                IMapper mapper,
                                                ILessonModuleRepository lessonModuleRepository)
        {
            _homeWorkRepository = homeWorkRepository;
            _mapper = mapper;
            _lessonModuleRepository = lessonModuleRepository;
        }

        public async Task<MethodResult<HomeWorkModel>> Handle(GetHomeWorkByOriginalIdQuery request, CancellationToken cancellationToken)
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

            var lessonModules = await _lessonModuleRepository.Queryable.Where(p => p.OriginalId == homeWork.OriginalId).Where(p => p.LessonConfigType == EnumLessonConfigType.HomeWork).ToListAsync(cancellationToken);

            var homeWorkModel = _mapper.Map<HomeWorkModel>(homeWork);

            homeWorkModel.IsActive = lessonModules.Any();

            methodResult.Result = homeWorkModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
