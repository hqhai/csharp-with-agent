// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.LevelQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetLevelInCategoryQuery : IRequest<MethodResult<IList<SubjectModel>>>
    {
    }

    public class GetLevelInCategoryQueryHandler : IRequestHandler<GetLevelInCategoryQuery, MethodResult<IList<SubjectModel>>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public GetLevelInCategoryQueryHandler(ICategoryRepository categoryRepository,
            IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<SubjectModel>>> Handle(GetLevelInCategoryQuery request, CancellationToken cancellationToken)
        {
            var categories = await _categoryRepository.ReadQueryable
                .Where(x => x.Type == EnumTypeCategory.Subject
                            && x.Status == EnumStatus.Active
                            && x.ParentId == null)
                .ToListAsync(cancellationToken);
            foreach (var category in categories)
            {
                await LoadChildCategory(category);
            }

            var subjectModels = categories.Select(x => GetSubjectModels(x)).ToList();
            return new MethodResult<IList<SubjectModel>>() { Result = subjectModels, StatusCode = 200 };
        }

        private async Task LoadChildCategory(Category category)
        {
            category.Categorys = await _categoryRepository.ReadQueryable.Include(x => x.Levels).Where(x => x.ParentId == category.Id && x.Status == EnumStatus.Active)
                .ToListAsync();
            foreach (var child in category.Categorys)
            {
                await LoadChildCategory(child);
            }
        }

        private SubjectModel GetSubjectModels(Category category)
        {
            var subjectModel = new SubjectModel
            {
                Id = category.Id,
                Name = category.Name,
                Type = category.Type.ToString(),
                TestMode = category.TestMode,
                ChildSubjects = new List<SubjectModel>()
            };

            if (category.Levels != null && category.Levels.Any())
            {
                subjectModel.Levels = _mapper.Map<List<LevelModel>>(category.Levels).OrderBy(l => l.LevelOrder).ToList();
            }

            foreach (var child in category.Categorys)
            {
                subjectModel.ChildSubjects.Add(GetSubjectModels(child));
            }

            return subjectModel;
        }
    }
}
