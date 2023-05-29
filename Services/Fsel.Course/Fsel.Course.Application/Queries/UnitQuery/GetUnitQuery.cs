// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.ActionResults;
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
            ArgumentNullException.ThrowIfNull(request);

            MethodResult<UnitModel> methodResult = new MethodResult<UnitModel>();

            var unit = await _unitRepository.GetIncludeByIdAsync(request.Id);

            if (unit == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumUnitErrorCode.UnitNotExist), nameof(request.Id), request.Id);
                return methodResult;
            }

            var unitModel = _mapper.Map<UnitModel>(unit);
            unitModel.IsActive = unit.CourseUnitMockTests.Any();
            unitModel.Lessons = unit.UnitLessons.Select(x =>
                                {
                                    var model = _mapper.Map<LessonModel>(x.Lesson);
                                    model.DisplayOrder = x.DisplayOrder;
                                    return model;
                                }).OrderBy(x => x.DisplayOrder).ToList();
            unitModel.SkillMockTests = unit.UnitSkillMockTests.Select(x =>
            {
                var model = _mapper.Map<MockTestModel>(x.MockTest);
                return model;
            }).FirstOrDefault();

            methodResult.Result = unitModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
