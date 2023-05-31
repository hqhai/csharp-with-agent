// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.FinalTestCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class DeleteFinalTestCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class DeleteFinalTestCommandHandler : IRequestHandler<DeleteFinalTestCommand, MethodResult<bool>>
    {
        private readonly IFinalTestRepository _finalTestRepository;
        private readonly ISectionQuestionRepository _sectionQuestionRepository;
        private readonly ISectionGroupRepository _sectionGroupRepository;
        private readonly IQuestionRepository _questionRepository;

        public DeleteFinalTestCommandHandler(IFinalTestRepository finalTestRepository
            , ISectionQuestionRepository sectionQuestionRepository
            , ISectionGroupRepository sectionGroupRepository
            , IQuestionRepository questionRepository)
        {
            _finalTestRepository = finalTestRepository;
            _sectionQuestionRepository = sectionQuestionRepository;
            _sectionGroupRepository = sectionGroupRepository;
            _questionRepository = questionRepository;
        }

        public async Task<MethodResult<bool>> Handle(DeleteFinalTestCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();
            var finalTest = await _finalTestRepository.GetIncludeByIdAsync(request.Id);
            if (finalTest == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumFinalTestErrorCode.FinalTestsNotExist), nameof(request.Id), request?.Id);
                return methodResult;
            }
            if (finalTest.IsActive)
            {
                methodResult.AddErrorBadRequest(nameof(EnumFinalTestErrorCode.FinalTestInActiveState), nameof(finalTest.IsActive), finalTest.IsActive);
                return methodResult;
            }

            List<SectionGroup> sectionGroups = finalTest.FinalTestSections.Select(x => x.SectionGroup ?? new SectionGroup()).ToList();
            List<Section> sections = sectionGroups.SelectMany(x => x.Sections).ToList();
            List<SectionQuestion> sectionQuestions = sections.SelectMany(x => x.SectionQuestions).ToList();
            List<Question> questions = sectionQuestions.Select(x => x.Question ?? new Question()).ToList();

            await _finalTestRepository.ExecuteTransactionAsync(async () =>
            {
                foreach (var item in sectionGroups)
                {
                    await _sectionGroupRepository.DeleteAsync(item);
                }
                await _sectionGroupRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                foreach (var item in sectionQuestions)
                {
                    await _sectionQuestionRepository.DeleteAsync(item);
                }
                await _sectionQuestionRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                foreach (var item in questions)
                {
                    await _questionRepository.DeleteAsync(item);
                }
                await _questionRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                var result = await _finalTestRepository.DeleteAsync(finalTest);
                await _finalTestRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = result;
                return methodResult;
            });
            return methodResult;
        }
    }
}
