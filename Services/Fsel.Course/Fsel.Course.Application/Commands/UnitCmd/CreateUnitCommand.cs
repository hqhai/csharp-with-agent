using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Helpers;
using Fsel.Course.Common.Models.Commands.Unit;
using Fsel.Course.Common.Models.Entities;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums.ErrorCodes;
using Fsel.Course.Domain.IRepositories;
using MediatR;
using Microsoft.AspNetCore.Http;

using Unit = Fsel.Course.Domain.Entities.Unit;

namespace Fsel.Course.Application.Commands.UnitCmd
{
    public class CreateUnitCommand : CreateUnitCommandModel, IRequest<MethodResult<UnitModel>>
    {
    }

    public class CreateUnitCommandHandler : IRequestHandler<CreateUnitCommand, MethodResult<UnitModel>>
    {
        private readonly IUnitRepository _unitRepository;
        private readonly IMapper _mapper;
        private readonly ILessonRepository _lessonRepository;
        private readonly IMockTestRepository _mockTestRepository;

        public CreateUnitCommandHandler(IUnitRepository unitRepository, ILessonRepository lessonRepository, IMockTestRepository mockTestRepository,
            IMapper mapper)
        {
            _lessonRepository = lessonRepository;
            _unitRepository = unitRepository;
            _mockTestRepository = mockTestRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<UnitModel>> Handle(CreateUnitCommand request, CancellationToken cancellationToken)
        {
            MethodResult<UnitModel> methodResult = new MethodResult<UnitModel>();

            #region Validation

            Unit unit = _mapper.Map<Unit>(request);

            if (!unit.IsValid())
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddResultFromErrorList(unit.ErrorMessages);
                return methodResult;
            }

            if (request.LessonIds == null)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddErrorMessage(
                    nameof(EnumUnitErrorCode.U03V),
                    new[] { MethodHelper.GenerateErrorResult(nameof(request.LessonIds), request.LessonIds) });
                return methodResult;
            }

            if (_lessonRepository.IsIdsInValid(request.LessonIds))
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddErrorMessage(
                    nameof(EnumLessonErrorCode.L03V));

                return methodResult;
            }

            #endregion Validation

            await _unitRepository.ExecuteTransactionAsync(async () =>
            {
                unit.UnitLessons = request.LessonIds.Select(x => new UnitLesson
                {
                    LessonId = x
                }).ToList();

                unit.UnitSkillMockTests = new List<UnitSkillMockTest>
                {
                    new UnitSkillMockTest
                    {
                        MockTestId = request.MockTestId,
                        UnitId= request.MockTestId,
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