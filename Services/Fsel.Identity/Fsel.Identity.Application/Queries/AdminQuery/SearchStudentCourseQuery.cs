// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.AdminQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Base.Managers;
    using Fsel.Core.Extensions;
    using Fsel.Identity.Application.Services.TrainingService;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchStudentCourseQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<StudentCourseModel>>>
    {
        public Guid StudentId { get; set; }
    }

    public class SearchStudentCourseQueryHandler : IRequestHandler<SearchStudentCourseQuery, MethodResult<PagingItemsModel<StudentCourseModel>>>
    {
        private readonly UserManager<User> _userManager;
        private readonly ITrainingService _trainingService;
        private readonly IUserSchoolRepository _userSchoolRepository;

        public SearchStudentCourseQueryHandler(UserManager<User> userManager, ITrainingService trainingService, IUserSchoolRepository userSchoolRepository)
        {
            _userManager = userManager;
            _trainingService = trainingService;
            _userSchoolRepository = userSchoolRepository;
        }

        public async Task<MethodResult<PagingItemsModel<StudentCourseModel>>> Handle(SearchStudentCourseQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PagingItemsModel<StudentCourseModel>> methodResult = new MethodResult<PagingItemsModel<StudentCourseModel>>();

            var user = await _userManager.Users.Include(x => x!.Student)
                                        .FirstOrDefaultAsync(x => x.Student != null && x.Student.Id == request.StudentId, cancellationToken);

            var studentCourseResults = await _trainingService.GetClassCourseStudentAsync(request.StudentId);
            var studentCourses = studentCourseResults.Content?.Result;
            var query = studentCourses?.AsEnumerable();
            int totalItem = query?.Count() ?? default;
            var lists = query?.ApplySortAndPaging(request).ToList();

            methodResult.Result = new PagingItemsModel<StudentCourseModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
