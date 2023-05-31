// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.FinalTestCmd
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.FinalTests;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class UpdateFinalTestCommand : UpdateFinalTestCommandModel, IRequest<MethodResult<FinalTestModel>>
    {
    }

    public class UpdateFinalTestCommandHandler : IRequestHandler<UpdateFinalTestCommand, MethodResult<FinalTestModel>>
    {
        private readonly IMapper _mapper;
        private readonly IFinalTestRepository _finalTestRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly ISectionGroupRepository _sectionGroupRepository;
        private readonly ISectionQuestionRepository _sectionQuestionRepository;
        private readonly SectionConverter _sectionConverter;

        public UpdateFinalTestCommandHandler(IMapper mapper
            , IFinalTestRepository finalTestRepository
            , IQuestionRepository questionRepository
            , ISectionGroupRepository sectionGroupRepository
            , ISectionQuestionRepository sectionQuestionRepository
            , SectionConverter sectionConverter)
        {
            _mapper = mapper;
            _finalTestRepository = finalTestRepository;
            _questionRepository = questionRepository;
            _sectionGroupRepository = sectionGroupRepository;
            _sectionQuestionRepository = sectionQuestionRepository;
            _sectionConverter = sectionConverter;
        }

        public async Task<MethodResult<FinalTestModel>> Handle(UpdateFinalTestCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<FinalTestModel> methodResult = new MethodResult<FinalTestModel>();

            #region Validation

            var finalTest = await _finalTestRepository.GetIncludeByIdAsync(request.Id);
            if (finalTest == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumFinalTestErrorCode.FinalTestsNotExist), nameof(request.Id), request.Id);
                return methodResult;
            }
            if (finalTest.IsActive)
            {
                methodResult.AddErrorBadRequest(nameof(EnumFinalTestErrorCode.FinalTestInActiveState), nameof(finalTest.IsActive), finalTest.IsActive);
                return methodResult;
            }

            if (request.SectionGroups == null || request.SectionGroups.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumExerciseErrorCode.ExercisesNull), nameof(request.SectionGroups));
                return methodResult;
            }
            List<SectionGroup> sectionGroups = finalTest.FinalTestSections.Select(x => x.SectionGroup ?? new SectionGroup()).ToList();
            List<Section> sections = sectionGroups.SelectMany(x => x.Sections).ToList();
            List<SectionQuestion> sectionQuestions = sections.SelectMany(x => x.SectionQuestions).ToList();
            List<Question> questions = sectionQuestions.Select(x => x.Question ?? new Question()).ToList();

            _mapper.Map(request, finalTest);
            finalTest.FinalTestSections = new List<FinalTestSection>();
            foreach (var sectionGroup in request.SectionGroups)
            {
                if (sectionGroup == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSectionGroupErrorCode.SectionGroupNull), nameof(sectionGroup));
                    return methodResult;
                }
                var newSectionGroup = _mapper.Map<SectionGroup>(sectionGroup);
                if (sectionGroup.Sections == null || sectionGroup.Sections.Count == 0)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSectionErrorCode.SectionsNull), nameof(sectionGroup.Sections));
                    return methodResult;
                }
                var method = _sectionConverter.AddSessionToSessionGroup(newSectionGroup, sectionGroup.Sections, null);
                if (!method.IsOK)
                {
                    methodResult.AddError(method.ErrorMessages);
                }
                finalTest.FinalTestSections.Add(new FinalTestSection { SectionGroup = newSectionGroup });
                if (!newSectionGroup.IsValid())
                {
                    methodResult.AddErrorBadRequest(newSectionGroup.ErrorMessages);
                }
            }
            if (!finalTest.IsValid())
            {
                methodResult.AddErrorBadRequest(finalTest.ErrorMessages);
                return methodResult;
            }
            else if (!methodResult.IsOK)
            {
                return methodResult;
            }

            #endregion Validation

            await _finalTestRepository.ExecuteTransactionAsync(async () =>
            {
                await _sectionConverter.DeleteSectionGroup(sectionGroups, sectionQuestions, questions);
                finalTest = _finalTestRepository.Update(finalTest);
                await _finalTestRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<FinalTestModel>(finalTest);
                return methodResult;
            });

            return methodResult;
        }
    }
}
