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
        private readonly ICategoryCachingService _categoryCachingService;

        public GetAllSubjectsQueryHandler(ICategoryRepository categoryRepository, ICategoryCachingService categoryCachingService)
        {
            _categoryCachingService = categoryCachingService;
        }

        public async Task<MethodResult<IList<SubjectModel>>> Handle(GetAllSubjectsQuery request, CancellationToken cancellationToken)
        {
            var subjects = await _categoryCachingService.GetAll(cancellationToken);
            var subjectModels = subjects.Select(GetSubjectModels).ToList();
            return new MethodResult<IList<SubjectModel>>() { Result = subjectModels, StatusCode = 200 };
        }

        private SubjectModel GetSubjectModels(Category category)
        {
            var subjectModel = new SubjectModel
            {
                Id = category.Id,
                Name = category.Name,
                Type = category.Type.ToString(),
                TestMode = category.TestMode,
                Thumbnail = category.Thumbnail,
                ChildSubjects = new List<SubjectModel>()
            };
            foreach (var child in category.Categorys)
            {
                subjectModel.ChildSubjects.Add(GetSubjectModels(child));
            }

            return subjectModel;
        }
    }
}
