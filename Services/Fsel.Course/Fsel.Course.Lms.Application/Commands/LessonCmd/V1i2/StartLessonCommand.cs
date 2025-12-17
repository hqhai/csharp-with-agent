// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.LessonCmd.V1i2
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.V1i1;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.CacheServices;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.LearningServices.LessonItemServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class StartLessonCommand : IRequest<MethodResult<LessonResultModel>>
    {
        public Guid LessonResultId { get; set; }
    }

    public class StartLessonCommandHandler : IRequestHandler<StartLessonCommand, MethodResult<LessonResultModel>>
    {
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly ILessonModuleCachingService _lessonModuleCachingService;
        private readonly ILessonModuleRepository _lessonModuleRepository;
        private readonly ILessonItemInitializerFactory _lessonItemInitializerFactory;
        private readonly IMapper _mapper;

        public StartLessonCommandHandler(ILessonResultRepository lessonResultRepository,
            ILessonModuleCachingService lessonModuleCachingService,
            ILessonModuleRepository lessonModuleRepository,
            ILessonItemInitializerFactory lessonItemInitializerFactory,
            IMapper mapper)
        {
            _lessonResultRepository = lessonResultRepository;
            _lessonModuleCachingService = lessonModuleCachingService;
            _lessonModuleRepository = lessonModuleRepository;
            _lessonItemInitializerFactory = lessonItemInitializerFactory;
            _mapper = mapper;
        }

        public async Task<MethodResult<LessonResultModel>> Handle(StartLessonCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<LessonResultModel>();

            var lessonResult = await _lessonResultRepository.Queryable.FirstOrDefaultAsync(x => x.Id == request.LessonResultId, cancellationToken);
            if (lessonResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(LessonResult), request.LessonResultId);
                return methodResult;
            }
            if (lessonResult.Status != Domain.Enums.EnumResultStatus.New)
            {
                return methodResult;
            }
            var lessonModules = await GetLessonModulesAsync(lessonResult.LessonId);
            var currentModule = lessonModules.OrderBy(x => x.OpenOrder).FirstOrDefault();
            if (currentModule == null)
            {
                return methodResult;
            }

            await UpdateNewResultLessonModule(currentModule, lessonResult, cancellationToken);

            lessonResult.Status = EnumResultStatus.Process;
            await _lessonResultRepository.BulkUpdateList(new List<LessonResult> { lessonResult }, bulk =>
            {
                bulk.ColumnInputExpression = entity => new { entity.Status };
            });

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = _mapper.Map<LessonResultModel>(lessonResult);
            return methodResult;
        }

        private async Task UpdateNewResultLessonModule(LessonModule nextModule, LessonResult lessonResult, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(nextModule);
            var initializer = _lessonItemInitializerFactory.Get(nextModule.LessonConfigType);
            if (initializer != null)
            {
                await initializer.InitializeAsync(nextModule, lessonResult, cancellationToken);
            }
            return;
        }

        private async Task<IList<LessonModule>> GetLessonModulesAsync(Guid id)
        {
            return await _lessonModuleCachingService.GetOrSetAsync(id.ToString(), async (ctx, _) =>
            {
                var lessonModules = await _lessonModuleRepository.ReadQueryable
                                                .Where(x => x.LessonId == id)
                                                .ToListAsync(_);

                return lessonModules.OrderBy(x => x.DisplayOrder).ToList();
            });
        }
    }
}
