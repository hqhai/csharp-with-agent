// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.AdminQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Application.Services.LmsCourseService;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchStudentLessonCommentQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<StudentLessonCommentModel>>>
    {
        public Guid StudentId { get; set; }
    }

    public class SearchStudentLessonCommentQueryHandler : IRequestHandler<SearchStudentLessonCommentQuery, MethodResult<PagingItemsModel<StudentLessonCommentModel>>>
    {
        private readonly UserManager<User> _userManager;
        private readonly ILmsCourseService _courseService;

        public SearchStudentLessonCommentQueryHandler(UserManager<User> userManager, ILmsCourseService courseService)
        {
            _userManager = userManager;
            _courseService = courseService;
        }

        public async Task<MethodResult<PagingItemsModel<StudentLessonCommentModel>>> Handle(SearchStudentLessonCommentQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PagingItemsModel<StudentLessonCommentModel>> methodResult = new MethodResult<PagingItemsModel<StudentLessonCommentModel>>();

            var user = await _userManager.Users.Include(x => x.Human)
                                        .ThenInclude(x => x!.Student)
                                        .FirstOrDefaultAsync(x => x.Human != null && x.Human.Student != null && x.Human.Student.Id == request.StudentId, cancellationToken);
            if (user == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(user));
                return methodResult;
            }
            var lessonCommentResult = await _courseService.GetLessonCommentByStudent(request.StudentId);
            if (!lessonCommentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallCourseServiceError));
                return methodResult;
            }
            var query = lessonCommentResult.Content?.Result?.AsEnumerable();
            int totalItem = query?.Count() ?? 0;
            var lists = query?.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToList() ?? null;
            methodResult.Result = new PagingItemsModel<StudentLessonCommentModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
