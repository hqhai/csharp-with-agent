// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.ActionResults;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums.ErrorCodes;
using Fsel.Course.Domain.IRepositories;
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
        private readonly IQuestionRepository _questionRepository;
        private readonly ISectionQuestionRepository _sectionQuestionRepository;
        private readonly ISectionGroupRepository _sectionGroupRepository;

        public DeletePlacementTestCommandHandler(IPlacementTestRepository placementTestRepository
            , IQuestionRepository questionRepository
            , ISectionQuestionRepository sectionQuestionRepository
            , ISectionGroupRepository sectionGroupRepository
            )
        {
            _placementTestRepository = placementTestRepository;
            _questionRepository = questionRepository;
            _sectionQuestionRepository = sectionQuestionRepository;
            _sectionGroupRepository = sectionGroupRepository;
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
            List<Section> sections = sectionGroups.SelectMany(x => x.Sections!).ToList();
            List<SectionPart>? sectionParts = null;
            List<SectionQuestion>? sectionQuestions = null;
            List<Question>? questions = null;
            if (sections.SelectMany(x => x.SectionParts).ToList() == null || sections.SelectMany(x => x.SectionParts).ToList().Count == 0)
            {
                sectionParts = sections.SelectMany(x => x.SectionParts).ToList();
                questions = sectionParts.SelectMany(x => x.SectionQuestions).Select(x => x.Question ?? new Question()).ToList();
            }
            else
            {
                sectionQuestions = sections.SelectMany(x => x.SectionQuestions).ToList();
                questions = sectionQuestions.Select(x => x.Question ?? new Question()).ToList();
            }

            #endregion Validation

            await _placementTestRepository.ExecuteTransactionAsync(async () =>
            {
                foreach (var item in sectionGroups)
                {
                    await _sectionGroupRepository.DeleteAsync(item);
                }
                await _sectionGroupRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                if (sectionQuestions != null && sectionQuestions.Count > 0)
                {
                    foreach (var item in sectionQuestions)
                    {
                        await _sectionQuestionRepository.DeleteAsync(item);
                    }
                    await _sectionQuestionRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }

                foreach (var item in questions)
                {
                    await _questionRepository.DeleteAsync(item);
                }
                await _questionRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

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
