// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Queries.StudentReviewQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Interaction.Application.Services.CourseServices;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetListStudentReviewQuery : IRequest<MethodResult<IList<StudentReviewModel>>>
    {
        public EnumReviewType ReviewType { get; set; }
    }

    public class GetListStudentReviewQueryHandler : IRequestHandler<GetListStudentReviewQuery, MethodResult<IList<StudentReviewModel>>>
    {
        private readonly IStudentReviewRepository _studentReviewRepository;
        private readonly AuthContext _authContext;
        private readonly ICourseService _courseService;
        private readonly IMapper _mapper;

        public GetListStudentReviewQueryHandler(IStudentReviewRepository studentReviewRepository
            , AuthContext authContext
            , ICourseService courseService
            , IMapper mapper)
        {
            _studentReviewRepository = studentReviewRepository;
            _authContext = authContext;
            _courseService = courseService;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<StudentReviewModel>>> Handle(GetListStudentReviewQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<StudentReviewModel>>();
            var courseResult = await _courseService.GetCourseStudying();
            if (!courseResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallCourseServiceError));
                return methodResult;
            }
            var course = courseResult.Content?.Result;
            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }
            var query = _studentReviewRepository.Queryable.Include(x => x.StudentReviewDetails)
                        .Where(x => x.ReviewType == request.ReviewType && x.CreatedUserId == _authContext.CurrentUserId);
            if (request.ReviewType == EnumReviewType.Course)
            {
                query = query.Where(x => x.CourseId == course.Id);
            }
            var studentReviews = await query.OrderBy(x => x.CreatedDate)
            .Select(x => new StudentReviewModel
            {
                Id = x.Id,
                ReviewType = x.ReviewType,
                CourseId = x.CourseId ?? null,
                StudentId = x.StudentId,
                StudentReviewDetails = _mapper.Map<IList<StudentReviewDetailModel>>(x.StudentReviewDetails.OrderBy(x => x.CreatedDate))
            }).ToListAsync(cancellationToken: cancellationToken);
            methodResult.Result = studentReviews;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
