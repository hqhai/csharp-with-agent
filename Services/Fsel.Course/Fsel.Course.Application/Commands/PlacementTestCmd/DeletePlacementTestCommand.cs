// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.ActionResults;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums.ErrorCodes;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Infrastructure.Common;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Fsel.Course.Application.Commands.PlacementTestCmd
{
    public class DeletePlacementTestCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class DeletePlacementTestCommandHandler : IRequestHandler<DeletePlacementTestCommand, MethodResult<bool>>
    {
        private readonly IPlacementTestRepository _placementTestRepository;
        private readonly SectionConverter _sectionConverter;

        public DeletePlacementTestCommandHandler(IPlacementTestRepository placementTestRepository
            , SectionConverter sectionConverter
            )
        {
            _placementTestRepository = placementTestRepository;
            _sectionConverter = sectionConverter;
        }

        public async Task<MethodResult<bool>> Handle(DeletePlacementTestCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            #region Validation

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
            List<SectionGroup> sectionGroups = placementTest.PlacementTestSections.Select(x => x.SectionGroup!).ToList();
            List<Section> sections = sectionGroups.SelectMany(x => x.Sections).ToList();
            List<SectionQuestion> sectionQuestions;
            List<Question> questions;
            if (sections.SelectMany(x => x.SectionParts).ToList() == null || sections.SelectMany(x => x.SectionParts).ToList().Count == 0)
            {
                sectionQuestions = sections.SelectMany(x => x.SectionParts).SelectMany(x => x.SectionQuestions).ToList();
                questions = sectionQuestions.Select(x => x.Question ?? new Question()).ToList();
            }
            else
            {
                sectionQuestions = sections.SelectMany(x => x.SectionQuestions).ToList();
                questions = sectionQuestions.Select(x => x.Question ?? new Question()).ToList();
            }

            #endregion Validation

            await _placementTestRepository.ExecuteTransactionAsync(async () =>
            {
                await _sectionConverter.DeleteSectionGroup(sectionGroups, sectionQuestions, questions);
                var result = await _placementTestRepository.DeleteAsync(placementTest);
                await _placementTestRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = result;
                return methodResult;
            });

            return methodResult;
        }
    }
}
