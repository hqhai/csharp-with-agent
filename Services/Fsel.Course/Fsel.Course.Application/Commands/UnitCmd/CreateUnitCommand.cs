// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.CommandModels.Units;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Course.Infrastructure.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Unit = Fsel.Course.Domain.Entities.Unit;

namespace Fsel.Course.Application.Commands.UnitCmd
{
    public class CreateUnitCommand : UpdateUnitCommandModel, IRequest<MethodResult<UnitModel>>
    {
    }

    public class CreateUnitCommandHandler : IRequestHandler<CreateUnitCommand, MethodResult<UnitModel>>
    {
        private readonly IUnitRepository _unitRepository;
        private readonly IMapper _mapper;
        private readonly UnitHelper _unitHelper;

        public CreateUnitCommandHandler(IUnitRepository unitRepository
            , UnitHelper unitHelper
            , IMapper mapper)
        {
            _unitHelper = unitHelper;
            _unitRepository = unitRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<UnitModel>> Handle(CreateUnitCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<UnitModel> methodResult = new MethodResult<UnitModel>();

            Unit unit = _mapper.Map<Unit>(request);
            var method = await _unitHelper.Validate(unit, request);
            if (!method.IsOK)
            {
                methodResult.AddErrorBadRequest(method.ErrorMessages);
                return methodResult;
            }

            await _unitRepository.ExecuteTransactionAsync(async () =>
            {
                unit.UnitLessons = request.LessonIds!.Select((x, index) => new UnitLesson
                {
                    DisplayOrder = index,
                    LessonId = x
                }).ToList();

                unit.UnitSkillMockTests = new List<UnitSkillMockTest>
                {
                    new UnitSkillMockTest
                    {
                        MockTestId = request.MockTestId,
                    }
                };
                unit = _unitRepository.Add(unit);
                await _unitRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<UnitModel>(unit);
                return methodResult;
            });

            return methodResult;
        }
    }
}
