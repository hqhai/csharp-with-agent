// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.FinalTestCmd
{
    using System;
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

    public class CreateFinalTestCommand : CreateFinalTestCommandModel, IRequest<MethodResult<FinalTestModel>>
    {
    }

    public class CreateFinalTestCommandHandler : IRequestHandler<CreateFinalTestCommand, MethodResult<FinalTestModel>>
    {
        private readonly IMapper _mapper;
        private readonly IFinalTestRepository _finalTestRepository;
        private readonly SectionConverter _sectionConverter;

        public CreateFinalTestCommandHandler(IMapper mapper, IFinalTestRepository finalTestRepository, SectionConverter sectionConverter)
        {
            _mapper = mapper;
            _finalTestRepository = finalTestRepository;
            _sectionConverter = sectionConverter;
        }

        public async Task<MethodResult<FinalTestModel>> Handle(CreateFinalTestCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<FinalTestModel> methodResult = new MethodResult<FinalTestModel>();
            if (request.SectionGroups == null || request.SectionGroups.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSectionGroupErrorCode.SectionGroupsNull), nameof(request.SectionGroups));
                return methodResult;
            }
            FinalTest finalTest = _mapper.Map<FinalTest>(request);

            foreach (var sectionGroup in request.SectionGroups)
            {
                if (sectionGroup == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSectionGroupErrorCode.SectionGroupNull), nameof(sectionGroup));
                    return methodResult;
                }
                else
                {
                    if (sectionGroup.Sections == null || sectionGroup.Sections.Count == 0)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSectionErrorCode.SectionsNull), nameof(sectionGroup.Sections));
                        return methodResult;
                    }

                    SectionGroup newSectionGroup = _mapper.Map<SectionGroup>(sectionGroup);
                    foreach (var section in sectionGroup.Sections)
                    {
                        Section newSection = newSectionGroup.Sections.ElementAt(sectionGroup.Sections.IndexOf(section));
                        var method = _sectionConverter.AddQuestionToSession(newSection, section.Questions);
                        if (!method.IsOK)
                        {
                            methodResult.AddError(method.ErrorMessages);
                        }

                        if (!newSection.IsValid())
                        {
                            methodResult.AddErrorBadRequest(newSection.ErrorMessages);
                            return methodResult;
                        }
                    }

                    finalTest.FinalTestSections.Add(new FinalTestSection
                    {
                        SectionGroup = newSectionGroup
                    });
                    if (!newSectionGroup.IsValid())
                    {
                        methodResult.AddErrorBadRequest(newSectionGroup.ErrorMessages);
                        return methodResult;
                    }
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
            await _finalTestRepository.ExecuteTransactionAsync(async () =>
            {
                finalTest = _finalTestRepository.Add(finalTest);

                await _finalTestRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<FinalTestModel>(finalTest);
                return methodResult;
            });

            return methodResult;
        }
    }
}
