// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Infrastructure.Common.Processors
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.ExamPractice.Domain.Entities;
    using Fsel.ExamPractice.Domain.Enums;
    using Fsel.ExamPractice.Domain.Models.CommandModels.ExamPractices;
    using Fsel.ExamPractice.Domain.Models.EntityModels.ExamPractices;
    using Fsel.ExamPractice.Infrastructure.Common;
    using Fsel.ExamPractice.Infrastructure.Common.ExamPracticeHelpers;

    /// <summary>
    /// Base interface for type-specific processors
    /// </summary>
    public interface IUpdateExamPracticeProcessor
    {
        EnumExamPracticeType ExamPracticeType { get; }

        /// <summary>
        /// Validate requirements for clone operation
        /// </summary>
        VoidMethodResult ValidateForClone(UpdateExamPracticeCommandModel request, ExamPracticeHelper helper);

        /// <summary>
        /// Apply update logic for this type
        /// </summary>
        Task ApplyUpdateAsync(
            UpdateExamPracticeCommandModel request,
            ExamPractice oldEntity,
            ExamPractice newEntity,
            bool isUsedByClient,
            bool shouldClone,
            IMapper mapper,
            ExamPracticeHelper helper,
            MethodResult<ExamPracticeModel> result,
            CancellationToken ct);

        /// <summary>
        /// Apply publish rule (Draft -> Active)
        /// </summary>
        void ApplyPublishRule(UpdateExamPracticeCommandModel request, ExamPractice entity);
    }

    /// <summary>
    /// Base class with common processing logic
    /// </summary>
    public abstract class BaseUpdateExamPracticeProcessor : IUpdateExamPracticeProcessor
    {
        public abstract EnumExamPracticeType ExamPracticeType { get; }

        public virtual VoidMethodResult ValidateForClone(UpdateExamPracticeCommandModel request, ExamPracticeHelper helper)
        {
            var errorResult = new VoidMethodResult();
            var leafSections = helper.GetLeafSections(request.ExamPracticeSections);
            if (!helper.AllSectionsHaveAtLeastOneQuestion(leafSections))
            {
                errorResult.AddErrorBadRequest(
                    nameof(Domain.Enums.ErrorCodes.EnumExamPracticeErrorCode.MissingRequiredData),
                    nameof(ExamPractice.Status),
                    EnumExamPracticeStatus.Active);
            }
            return errorResult;
        }

        public virtual async Task ApplyUpdateAsync(
            UpdateExamPracticeCommandModel request,
            ExamPractice oldEntity,
            ExamPractice newEntity,
            bool isUsedByClient,
            bool shouldClone,
            IMapper mapper,
            ExamPracticeHelper helper,
            MethodResult<ExamPracticeModel> result,
            CancellationToken ct)
        {
            // Skip if clone mode (status already set)
            if (shouldClone)
                return;

            var targetEntity = isUsedByClient ? newEntity : oldEntity;
            mapper.Map(request, targetEntity);

            if (!targetEntity.IsValid())
            {
                result.AddErrorBadRequest(targetEntity.ErrorMessages);
                return;
            }

            // Apply publish rule
            if (!request.IsDraft)
            {
                ApplyPublishRule(request, targetEntity);
            }

            // Update sections using ExamPracticeHelper (same as Original)
            var newSections = new List<ExamPracticeSection>();

            // 1) Map sections recursively (load from DB if exists, map request)
            var mapResult = await helper.MapSectionsRecursively(targetEntity, request.ExamPracticeSections, newSections);
            if (!mapResult.IsOK)
            {
                result.AddErrorBadRequest(mapResult.ErrorMessages);
                return;
            }

            // 2) Delete old sections not in request
            await helper.DeleteExamPracticeSectionsAsync(request);

            // 3) Assign new sections to entity
            targetEntity.ExamPracticeSections = newSections;
        }

        public abstract void ApplyPublishRule(UpdateExamPracticeCommandModel request, ExamPractice entity);
    }
}