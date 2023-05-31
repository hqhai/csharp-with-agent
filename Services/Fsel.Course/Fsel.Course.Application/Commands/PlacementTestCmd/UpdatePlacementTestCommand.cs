// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums.ErrorCodes;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.CommandModels.PlacementTests;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Course.Infrastructure.Common;
using Fsel.Shared.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Fsel.Course.Application.Commands.PlacementTestCmd
{
    public class UpdatePlacementTestCommand : UpdatePlacementTestCommandModel, IRequest<MethodResult<PlacementTestModel>>
    {
    }

    public class UpdatePlacementTestCommandHandler : IRequestHandler<UpdatePlacementTestCommand, MethodResult<PlacementTestModel>>
    {
        private readonly IPlacementTestRepository _placementTestRepository;
        private readonly IMapper _mapper;
        private readonly SectionConverter _sectionConverter;

        public UpdatePlacementTestCommandHandler(IPlacementTestRepository placementTestRepository,
            IMapper mapper,
            SectionConverter sectionConverter)
        {
            _placementTestRepository = placementTestRepository;
            _mapper = mapper;
            _sectionConverter = sectionConverter;
        }

        public async Task<MethodResult<PlacementTestModel>> Handle(UpdatePlacementTestCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PlacementTestModel> methodResult = new MethodResult<PlacementTestModel>();

            #region Validation

            if (request.SectionGroups == null || request.SectionGroups.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSectionGroupErrorCode.SectionGroupsNull), nameof(request.SectionGroups));
                return methodResult;
            }

            var placementTest = await _placementTestRepository.GetIncludeByIdAsync(request.Id);
            if (placementTest == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumPlacementTestErrorCode.PlacementTestNotExist), nameof(request.Id), request.Id);
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
            if (placementTest.Level == EnumPlacementTestLevel.IELTS)
            {
                sectionQuestions = sectionParts.SelectMany(x => x.SectionQuestions).ToList();
                questions = sectionQuestions.Select(x => x.Question ?? new Question()).ToList();
            }
            else
            {
                sectionQuestions = sectionGroups.SelectMany(x => x.Sections).SelectMany(x => x.SectionQuestions).ToList();
                questions = sectionGroups.SelectMany(x => x.Sections).SelectMany(x => x.SectionQuestions).Select(x => x.Question ?? new Question()).ToList();
            }

            _mapper.Map(request, placementTest);
            placementTest.PlacementTestSections.Clear();

            foreach (var sectionGroup in request.SectionGroups)
            {
                if (sectionGroup == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSectionGroupErrorCode.SectionGroupNull), nameof(sectionGroup));
                    return methodResult;
                }
                var newSectionGroup = _mapper.Map<SectionGroup>(sectionGroup);
                var method = _sectionConverter.AddSessionToSessionGroup(newSectionGroup, sectionGroup.Sections, request.Level);
                if (!method.IsOK)
                {
                    methodResult.AddError(method.ErrorMessages);
                }
                placementTest.PlacementTestSections.Add(new PlacementTestSection { SectionGroup = newSectionGroup });
                if (!newSectionGroup.IsValid())
                {
                    methodResult.AddErrorBadRequest(newSectionGroup.ErrorMessages);
                }
            }
            if (!placementTest.IsValid())
            {
                methodResult.AddErrorBadRequest(placementTest.ErrorMessages);
                return methodResult;
            }
            else if (!methodResult.IsOK)
            {
                return methodResult;
            }

            #endregion Validation

            await _placementTestRepository.ExecuteTransactionAsync(async () =>
            {
                await _sectionConverter.DeleteSectionGroup(sectionGroups, sectionQuestions, questions);
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
