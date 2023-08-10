// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Queries.StudentReviewQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
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
        private readonly IMapper _mapper;

        public GetListStudentReviewQueryHandler(IStudentReviewRepository studentReviewRepository
            , IMapper mapper)
        {
            _studentReviewRepository = studentReviewRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<StudentReviewModel>>> Handle(GetListStudentReviewQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<StudentReviewModel>>();
            var studentReviews = await _studentReviewRepository.Queryable.Include(x => x.StudentReviewDetails).Where(x => x.ReviewType == request.ReviewType).Select(x => new StudentReviewModel
            {
                Id = x.Id,
                CreatedDate = x.CreatedDate,
                CreatedFullName = x.CreatedFullName,
                CreatedUserId = x.CreatedUserId,
                UpdatedDate = x.UpdatedDate,
                UpdatedUserId = x.UpdatedUserId,
                UpdatedFullName = x.UpdatedFullName,
                ReviewType = x.ReviewType,
                CourseId = x.CourseId ?? null,
                StudentId = x.StudentId,
                StudentReviewDetails = _mapper.Map<IList<StudentReviewDetailModel>>(x.StudentReviewDetails)
            }).OrderBy(x => x.CreatedDate).ToListAsync(cancellationToken: cancellationToken);

            methodResult.Result = studentReviews;
            methodResult.StatusCode = StatusCodes.Status200OK;

            return methodResult;
        }
    }
}
