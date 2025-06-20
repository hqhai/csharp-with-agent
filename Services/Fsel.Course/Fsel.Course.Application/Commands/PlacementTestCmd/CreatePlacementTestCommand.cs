// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.CommandModels.PlacementTests;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Course.Infrastructure.Common;
using Fsel.Shared.Enums;
using Fsel.Shared.Helpers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Application.Commands.PlacementTestCmd
{
    public class CreatePlacementTestCommand : CreatePlacementTestCommandModel, IRequest<MethodResult<PlacementTestModel>>
    {
    }

    public class CreatePlacementTestCommandHandler : IRequestHandler<CreatePlacementTestCommand, MethodResult<PlacementTestModel>>
    {
        private readonly IPlacementTestRepository _placementTestRepository;
        private readonly IMapper _mapper;
        private readonly SectionGroupManagerConverter _sectionGroupManagerConverter;

        public CreatePlacementTestCommandHandler(IPlacementTestRepository placementTestRepository,
            IMapper mapper,
            SectionGroupManagerConverter sectionGroupManagerConverter)
        {
            _placementTestRepository = placementTestRepository;
            _mapper = mapper;
            _sectionGroupManagerConverter = sectionGroupManagerConverter;
        }

        public async Task<MethodResult<PlacementTestModel>> Handle(CreatePlacementTestCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PlacementTestModel> methodResult = new MethodResult<PlacementTestModel>();

            #region Validation

            if (request.SectionGroups == null || request.SectionGroups.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.SectionGroups));
                return methodResult;
            }
            if (await _placementTestRepository.Queryable.AnyAsync(x => x.Name == request.Name, cancellationToken))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Name));
                return methodResult;
            }
            PlacementTest placementTest = _mapper.Map<PlacementTest>(request);
            if (!placementTest.IsValid())
            {
                methodResult.AddErrorBadRequest(placementTest.ErrorMessages);
                return methodResult;
            }

            foreach (var sectionGroup in request.SectionGroups)
            {
                if (sectionGroup == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(sectionGroup));
                    return methodResult;
                }

                sectionGroup.Sections = sectionGroup.Sections.OrderBy(x => x.DisplayOrder).Select((x, index) => { x.DisplayOrder = index; return x; }).ToList();
                SectionGroup newSectionGroup = _mapper.Map<SectionGroup>(sectionGroup);
                if (!newSectionGroup.IsValid())
                {
                    methodResult.AddErrorBadRequest(newSectionGroup.ErrorMessages);
                    return methodResult;
                }
                if (sectionGroup.Sections == null || sectionGroup.Sections.Count == 0)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(sectionGroup.Sections));
                    return methodResult;
                }
                var method = _sectionGroupManagerConverter.AddSessionToSessionGroup(newSectionGroup, sectionGroup.Sections, request.Level == EnumPlacementTestLevel.IELTS ? EnumCourseType.Ielts : EnumCourseType.Academic);
                if (!method.IsOK)
                {
                    methodResult.AddErrorBadRequest(method.ErrorMessages);
                    return methodResult;
                }
                placementTest.PlacementTestSections.Add(new PlacementTestSection
                {
                    SectionGroup = newSectionGroup
                });
            }

            #endregion Validation

            await _placementTestRepository.ExecuteTransactionAsync(async () =>
            {
                if (placementTest.PlacementTestLevel == EnumPlacementTestLevel.IELTS)
                {
                    placementTest.ExtraPractice = new ExtraPractice
                    {
                        Code = placementTest.Name,
                        Name = placementTest.Name,
                        IsActive = placementTest.IsActive,
                        InstructionContent = placementTest.InstructionContent,
                        Type = EnumExtraPracticeType.MockTest,
                        CourseLevel = placementTest.PlacementTestLevel.GetCourseLevelByPlacementTestLevel()
                    };
                }

                placementTest = _placementTestRepository.Add(placementTest);
                await _placementTestRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<PlacementTestModel>(placementTest);
                return methodResult;
            });

            return methodResult;
        }
    }
}
