// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.CategoryQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetCategoryByIdQuery : IRequest<MethodResult<CategoryModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, MethodResult<CategoryModel>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        private readonly ILevelRepository _levelRepository;

        public GetCategoryByIdQueryHandler(ICategoryRepository categoryRepository,
                                           IMapper mapper,
                                           ILevelRepository levelRepository)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
            _levelRepository = levelRepository;
        }

        public async Task<MethodResult<CategoryModel>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<CategoryModel> methodResult = new MethodResult<CategoryModel>();

            var category = await _categoryRepository.Queryable
                                                    .Include(x => x.SubjectConditions)
                                                    .ThenInclude(x => x.SubjectConditionRules)
                                                    .FirstOrDefaultAsync(x => x.Id == request.Id && x.Status != Shared.Enums.EnumStatus.Archive, cancellationToken);
            if (category == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var categoryResult = _mapper.Map<CategoryModel>(category);
            await SetLevelHandler(categoryResult, cancellationToken);

            methodResult.Result = categoryResult;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task SetLevelHandler(CategoryModel categoryResult, CancellationToken cancellationToken)
        {
            var levels = categoryResult.SubjectConditions?.Where(x => x.SubjectConditionRules != null).SelectMany(x => x.SubjectConditionRules!).Where(x => x.ConditionRules != null).SelectMany(x => x.ConditionRules!).Where(x => x.Levels != null).SelectMany(x => x.Levels!)
                         .Union(categoryResult.SubjectConditions.Where(x => x.SubjectConditionRules != null).SelectMany(x => x.SubjectConditionRules!).Where(x => x.ConditionValues != null).SelectMany(x => x.ConditionValues!).Where(x => x.Levels != null).SelectMany(x => x.Levels!)).ToList();

            if (levels != null && levels.Any())
            {
                var levelIds = levels.Select(x => x.LevelId).ToList();
                var levelDatas = await _levelRepository.Queryable.Include(x => x.Category).WhereBulkContains(levelIds, x => x.Id).ToListAsync(cancellationToken);

                foreach (var level in levels)
                {
                    var levelData = levelDatas.FirstOrDefault(x => x.Id == level.LevelId);
                    level.Name = $"{levelData?.Category?.Name} - {levelData?.Name}";
                }
            }
        }
    }
}
