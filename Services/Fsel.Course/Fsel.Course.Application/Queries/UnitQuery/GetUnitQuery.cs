using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Helpers;
using Fsel.Course.Domain.Enums.ErrorCodes;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.EntityModels;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Fsel.Course.Application.Queries.UnitQuery
{
    public class GetUnitQuery : IRequest<MethodResult<UnitModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetUnitQueryHandler : IRequestHandler<GetUnitQuery, MethodResult<UnitModel>>
    {
        private readonly IUnitRepository _unitRepository;
        private readonly IMapper _mapper;

        public GetUnitQueryHandler(IMapper mapper, IUnitRepository unitRepository)
        {
            _unitRepository = unitRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<UnitModel>> Handle(GetUnitQuery request, CancellationToken cancellationToken)
        {
            MethodResult<UnitModel> methodResult = new MethodResult<UnitModel>();

            var unit = await _unitRepository.GetIncludeByIdAsync(request.Id);

            if (unit == null)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddError(
                    nameof(EnumUnitErrorCode.U01V),
                    new[] { MethodHelper.GenerateErrorResult(nameof(request.Id), request.Id) });
                return methodResult;
            }

            unit.IsActive = !unit.CourseUnitMockTests.Any();

            methodResult.Result = _mapper.Map<UnitModel>(unit);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
