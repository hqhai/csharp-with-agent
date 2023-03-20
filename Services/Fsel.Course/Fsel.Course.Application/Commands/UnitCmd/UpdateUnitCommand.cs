using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Helpers;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.Enums.ErrorCodes;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.CommandModels.Units;
using Fsel.Course.Domain.Models.EntityModels;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Application.Commands.UnitCmd
{
    public class UpdateUnitCommand : UpdateUnitCommandModel, IRequest<MethodResult<UnitModel>>
    {
    }

    public class UpdateUnitCommandHandler : IRequestHandler<UpdateUnitCommand, MethodResult<UnitModel>>
    {
        private readonly IUnitRepository _unitRepository;
        private readonly IMapper _mapper;
        private readonly ILessonRepository _lessonRepository;
        private readonly IMockTestRepository _mockTestRepository;

        public UpdateUnitCommandHandler(IUnitRepository unitTestRepository, ILessonRepository lessonRepository, IMockTestRepository mockTestRepository,
            IMapper mapper)
        {
            _lessonRepository = lessonRepository;
            _unitRepository = unitTestRepository;
            _mapper = mapper;
            _mockTestRepository = mockTestRepository;
        }

        public async Task<MethodResult<UnitModel>> Handle(UpdateUnitCommand request, CancellationToken cancellationToken)
        {
            MethodResult<UnitModel> methodResult = new MethodResult<UnitModel>();

            #region Validation

            var unit = await _unitRepository.Queryable
                                    .Include(e => e.UnitLessons)
                                    .Include(e => e.UnitSkillMockTests)
                                    .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken: cancellationToken);
            if (unit == null)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddError(
                    nameof(EnumUnitErrorCode.U01V),
                    new[] { MethodHelper.GenerateErrorResult(nameof(request.Id), request?.Id) });
                return methodResult;
            }

            var isUnitUsed = await _unitRepository.IsUnitUsed(request.Id);
            if (isUnitUsed)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddError(
                    nameof(EnumUnitErrorCode.U02V),
                    new[] { MethodHelper.GenerateErrorResult(nameof(request.Id), request.Id) });
                return methodResult;
            }

            if (request.LessonIds == null)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddError(
                    nameof(EnumUnitErrorCode.U03V),
                    new[] { MethodHelper.GenerateErrorResult(nameof(request.LessonIds), request.LessonIds) });
                return methodResult;
            }

            if (_lessonRepository.IsIdsInValid(request.LessonIds))
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddError(
                    nameof(EnumLessonErrorCode.LS03V));

                return methodResult;
            }

            var checkMockTest = _mockTestRepository.Queryable.Any(x => x.MockTestType == EnumMockTestType.UnitMockTest && x.Id == request.MockTestId);
            if (!checkMockTest)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddError(
                nameof(EnumMockTestErrorCode.MT04V));
                return methodResult;
            }

            _mapper.Map(request, unit);

            if (!unit.IsValid())
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddResultFromErrorList(unit.ErrorMessages);
                return methodResult;
            }

            #endregion Validation

            await _unitRepository.ExecuteTransactionAsync(async () =>
            {
                unit = _unitRepository.Update(unit);
                await _unitRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<UnitModel>(unit);
                return methodResult;
            });

            return methodResult;
        }
    }
}
