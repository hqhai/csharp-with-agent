// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.CategoryQuery
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.CacheServices;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetAllSubjectsQuery : IRequest<MethodResult<IList<SubjectModel>>>
    {
    }

    public class GetAllSubjectsQueryHandler : IRequestHandler<GetAllSubjectsQuery, MethodResult<IList<SubjectModel>>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ICategoryCachingService _categoryCachingService;

        public GetAllSubjectsQueryHandler(ICategoryRepository categoryRepository, ICategoryCachingService categoryCachingService)
        {
            _categoryRepository = categoryRepository;
            _categoryCachingService = categoryCachingService;
        }

        public async Task<MethodResult<IList<SubjectModel>>> Handle(GetAllSubjectsQuery request, CancellationToken cancellationToken)
        {
            var subjects = await _categoryCachingService.GetOrSetAsync("all", async (ctx, _) =>
            {
                var categories = await _categoryRepository.ReadQueryable
                    .Where(x => x.Type == EnumTypeCategory.Subject
                                && x.Status == EnumStatus.Active
                                && x.ParentId == null)
                    .ToListAsync(cancellationToken);

                return categories;
            }, token: cancellationToken);

            return new MethodResult<IList<SubjectModel>>() { Result = GetSubjectModels(subjects).ToList(), StatusCode = 200 };
        }

        private async Task LoadChildCategory(Category category)
        {
            category.Categorys = await _categoryRepository.ReadQueryable.Where(x => x.ParentId == category.Id && x.Status == EnumStatus.Active).ToListAsync();
            foreach (var child in category.Categorys)
            {
                await LoadChildCategory(child);
            }
        }

        private static IEnumerable<SubjectModel> GetSubjectModels(IEnumerable<Category> categories)
        {
            foreach (var category in categories)
            {
                var subjectModel = new SubjectModel { Id = category.Id, Name = category.Name, };

                if (category.Categorys != null && category.Categorys.Count > 0)
                {
                    subjectModel.ChildSubjects.AddRange(GetSubjectModels(category.Categorys));
                }

                yield return subjectModel;
            }
        }
    }
}
