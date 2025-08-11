// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.HomeWorkQuery
{
    using System.Threading;
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

    public class GetHomeWorkQuery : IRequest<MethodResult<HomeWorkModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetHomeWorkQueryHandler : IRequestHandler<GetHomeWorkQuery, MethodResult<HomeWorkModel>>
    {
        private readonly IMapper _mapper;
        private readonly IHomeWorkRepository _homeWorkRepository;
        private readonly ILessonModuleRepository _lessonModuleRepository;

        public GetHomeWorkQueryHandler(IMapper mapper, IHomeWorkRepository homeWorkRepository, ILessonModuleRepository lessonModuleRepository)
        {
            _mapper = mapper;
            _homeWorkRepository = homeWorkRepository;
            _lessonModuleRepository = lessonModuleRepository;
        }

        public async Task<MethodResult<HomeWorkModel>> Handle(GetHomeWorkQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<HomeWorkModel> methodResult = new MethodResult<HomeWorkModel>();

            var homeWork = await _homeWorkRepository.GetIncludeAllAsync(request.Id);

            if (homeWork == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(homeWork));
                return methodResult;
            }

            var lessonModules = await _lessonModuleRepository.Queryable.Where(p => p.OriginalId == homeWork.OriginalId).Where(p => p.LessonConfigType == EnumLessonConfigType.HomeWork).ToListAsync(cancellationToken);

            homeWork.IsActive = lessonModules.Any();

            methodResult.Result = _mapper.Map<HomeWorkModel>(homeWork);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
