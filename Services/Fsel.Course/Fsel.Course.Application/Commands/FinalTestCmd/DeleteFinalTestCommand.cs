// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.FinalTestCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Infrastructure.Common;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class DeleteFinalTestCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class DeleteFinalTestCommandHandler : IRequestHandler<DeleteFinalTestCommand, MethodResult<bool>>
    {
        private readonly IFinalTestRepository _finalTestRepository;
        private readonly SectionConverter _sectionConverter;

        public DeleteFinalTestCommandHandler(IFinalTestRepository finalTestRepository
            , SectionConverter sectionConverter)
        {
            _finalTestRepository = finalTestRepository;
            _sectionConverter = sectionConverter;
        }

        public async Task<MethodResult<bool>> Handle(DeleteFinalTestCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();
            var finalTest = await _finalTestRepository.GetIncludeByIdAsync(request.Id);
            if (finalTest == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Id));
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
                await _sectionConverter.DeleteSectionGroup(sectionGroups, sectionQuestions, questions);

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
