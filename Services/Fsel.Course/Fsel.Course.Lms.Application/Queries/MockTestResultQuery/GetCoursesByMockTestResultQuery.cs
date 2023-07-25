// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.MockTestResultQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using EntityCourse = Domain.Entities.Course;

    public class GetCoursesByMockTestResultQuery : IRequest<MethodResult<IList<CourseModel>>>
    {
    }

    public class GetCoursesByMockTestResultQueryHandler : IRequestHandler<GetCoursesByMockTestResultQuery, MethodResult<IList<CourseModel>>>
    {
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly IMapper _mapper;

        public GetCoursesByMockTestResultQueryHandler(IMockTestResultRepository mockTestResultRepository, IMapper mapper)
        {
            _mockTestResultRepository = mockTestResultRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<CourseModel>>> Handle(GetCoursesByMockTestResultQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<CourseModel>> methodResult = new MethodResult<IList<CourseModel>>();

            var mockTestResults = await _mockTestResultRepository.Queryable.Include(x => x.Course).ToListAsync(cancellationToken);
            List<EntityCourse> courses = mockTestResults.Select(x => x.Course ?? new EntityCourse()).ToList();
            methodResult.Result = _mapper.Map<IList<CourseModel>>(courses);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
