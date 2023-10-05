using AutoMapper;
using Fsel.Cms.PlanetDefender.Domain.IRepositories;
using Fsel.Cms.PlanetDefender.Domain.Models.EntityModels;
using Fsel.Cms.PlanetDefender.Domain.Models.QueryModels.StudentGameInfos;
using Fsel.Common.ActionResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Cms.PlanetDefender.Application.Queries.StudentGameInfoQuery
{
    public class GetLevelOfStudentsByStudentIdsQuery : GetLevelOfStudentsByStudentIdsQueryModel, IRequest<MethodResult<IList<StudentGameInfoModel>>>
    {
    }
    public class GetLevelOfStudentsByStudentIdsQueryHandler : IRequestHandler<GetLevelOfStudentsByStudentIdsQuery, MethodResult<IList<StudentGameInfoModel>>>
    {
        private readonly IStudentGameInfoRepository _studentGameInfoRepository;
        private readonly IMapper _mapper;
        public GetLevelOfStudentsByStudentIdsQueryHandler(IStudentGameInfoRepository studentGameInfoRepository, IMapper mapper)
        {
            _studentGameInfoRepository = studentGameInfoRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<StudentGameInfoModel>>> Handle(GetLevelOfStudentsByStudentIdsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<StudentGameInfoModel>> methodResult = new MethodResult<IList<StudentGameInfoModel>>();

            if (request.StudentIds == null || request.StudentIds.Count == 0)
            {
                return methodResult;
            }

            var studentGameInfoModel = await _studentGameInfoRepository.Queryable.Where(p => request.StudentIds.Contains(p.StudentId)).ToListAsync(cancellationToken);

            methodResult.Result = _mapper.Map<IList<StudentGameInfoModel>>(studentGameInfoModel);
            return methodResult;

        }
    }
}
