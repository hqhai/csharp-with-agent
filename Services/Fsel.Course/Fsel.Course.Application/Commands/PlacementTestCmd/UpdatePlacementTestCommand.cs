// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums.ErrorCodes;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.CommandModels.PlacementTests;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Course.Infrastructure.Common;
using Fsel.Shared.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Application.Commands.PlacementTestCmd
{
    public class UpdatePlacementTestCommand : UpdatePlacementTestCommandModel, IRequest<MethodResult<PlacementTestModel>>
    {
    }

    public class UpdatePlacementTestCommandHandler : IRequestHandler<UpdatePlacementTestCommand, MethodResult<PlacementTestModel>>
    {
        private readonly IPlacementTestRepository _placementTestRepository;
        private readonly IMapper _mapper;
        private readonly SectionGroupManagerConverter _sectionGroupManagerConverter;

        public UpdatePlacementTestCommandHandler(IPlacementTestRepository placementTestRepository,
            IMapper mapper,
            SectionGroupManagerConverter sectionGroupManagerConverter)
        {
            _placementTestRepository = placementTestRepository;
            _mapper = mapper;
            _sectionGroupManagerConverter = sectionGroupManagerConverter;
        }

        public async Task<MethodResult<PlacementTestModel>> Handle(UpdatePlacementTestCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PlacementTestModel> methodResult = new MethodResult<PlacementTestModel>();

            #region Validation

            if (request.SectionGroups == null || request.SectionGroups.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.SectionGroups));
                return methodResult;
            }
            if (await _placementTestRepository.Queryable.AnyAsync(x => x.Id != request.Id && x.Name == request.Name, cancellationToken))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(request.Name));
                return methodResult;
            }
            var placementTest = await _placementTestRepository.GetIncludeByIdAsync(request.Id);
            if (placementTest == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(placementTest));
                return methodResult;
            }

            if (placementTest.IsActive)
            {
                methodResult.AddErrorBadRequest(nameof(EnumPlacementTestErrorCode.PlacementTestInActiveState), nameof(placementTest.IsActive), placementTest.IsActive);
                return methodResult;
            }

            IList<SectionGroup> sectionGroups = placementTest.PlacementTestSections.Select(x => x.SectionGroup ?? new SectionGroup()).ToList();
            var sectionParts = sectionGroups.SelectMany(x => x.Sections).SelectMany(x => x.SectionParts).ToList();
            IList<Question> questions;
            IList<SectionQuestion> sectionQuestions;
            if (placementTest.PlacementTestLevel == EnumPlacementTestLevel.IELTS)
            {
                sectionQuestions = sectionParts.SelectMany(x => x.SectionQuestions).ToList();
                questions = sectionQuestions.Select(x => x.Question ?? new Question()).ToList();
            }
            else
            {
                sectionQuestions = sectionGroups.SelectMany(x => x.Sections).SelectMany(x => x.SectionQuestions).ToList();
                questions = sectionGroups.SelectMany(x => x.Sections).SelectMany(x => x.SectionQuestions).Select(x => x.Question ?? new Question()).ToList();
            }
            placementTest.PlacementTestSections.Clear();

            _mapper.Map(request, placementTest);
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
                var newSectionGroup = _mapper.Map<SectionGroup>(sectionGroup);
                if (!newSectionGroup.IsValid())
                {
                    methodResult.AddErrorBadRequest(newSectionGroup.ErrorMessages);
                    return methodResult;
                }
                var method = _sectionGroupManagerConverter.AddSessionToSessionGroup(newSectionGroup, sectionGroup.Sections, request.Level == EnumPlacementTestLevel.IELTS ? EnumCourseType.Ielts : EnumCourseType.Academic);
                if (!method.IsOK)
                {
                    methodResult.AddErrorBadRequest(method.ErrorMessages);
                    return methodResult;
                }
                placementTest.PlacementTestSections.Add(new PlacementTestSection { SectionGroup = newSectionGroup });
            }

            #endregion Validation

            await _placementTestRepository.ExecuteTransactionAsync(async () =>
            {
                await _sectionGroupManagerConverter.DeleteSectionGroup(sectionGroups, sectionQuestions, questions);
                placementTest = _placementTestRepository.Update(placementTest);
                await _placementTestRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<PlacementTestModel>(placementTest);
                return methodResult;
            });

            return methodResult;
        }
    }
}
