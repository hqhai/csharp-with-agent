// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.ClassQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.EntityModels;
    using Fsel.Training.Domain.Models.QueryModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetListClassBySpecificStudentIdsQuery : GetListClassBySpecificStudentIdsQueryModel, IRequest<MethodResult<IList<CompetitionClassStudentModel>>>
    {
    }

    public class GetListClassBySpecificStudentIdsQueryHandler : IRequestHandler<GetListClassBySpecificStudentIdsQuery, MethodResult<IList<CompetitionClassStudentModel>>>
    {
        private readonly IClassStudentRepository _classStudentRepository;
        private readonly IMapper _mapper;

        public GetListClassBySpecificStudentIdsQueryHandler(IClassStudentRepository classStudentRepository, IMapper mapper)
        {
            _classStudentRepository = classStudentRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<CompetitionClassStudentModel>>> Handle(GetListClassBySpecificStudentIdsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<CompetitionClassStudentModel>> methodResult = new MethodResult<IList<CompetitionClassStudentModel>>();
            var classStudents = await _classStudentRepository.Queryable.Include(x => x.Class).Where(x => request.StudentIds!.Contains(x.StudentId)).ToListAsync(cancellationToken);
            var result = classStudents.Select(x => new CompetitionClassStudentModel
            {
                StudentId = x.StudentId,
                CourseId = x.Class!.CourseId
            }).ToList();

            methodResult.Result = result;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
