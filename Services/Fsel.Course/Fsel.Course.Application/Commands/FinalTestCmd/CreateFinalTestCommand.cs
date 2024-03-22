// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.FinalTestCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.FinalTests;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateFinalTestCommand : CreateFinalTestCommandModel, IRequest<MethodResult<FinalTestModel>>
    {
    }

    public class CreateFinalTestCommandHandler : IRequestHandler<CreateFinalTestCommand, MethodResult<FinalTestModel>>
    {
        private readonly IMapper _mapper;
        private readonly IFinalTestRepository _finalTestRepository;
        private readonly SectionGroupManagerConverter _sectionGroupManagerConverter;

        public CreateFinalTestCommandHandler(IMapper mapper, IFinalTestRepository finalTestRepository, SectionGroupManagerConverter sectionGroupManagerConverter)
        {
            _mapper = mapper;
            _finalTestRepository = finalTestRepository;
            _sectionGroupManagerConverter = sectionGroupManagerConverter;
        }

        public async Task<MethodResult<FinalTestModel>> Handle(CreateFinalTestCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<FinalTestModel> methodResult = new MethodResult<FinalTestModel>();
            if (request.SectionGroups == null || !request.SectionGroups.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.SectionGroups));
                return methodResult;
            }
            if (await _finalTestRepository.Queryable.AnyAsync(x => x.Name == request.Name, cancellationToken))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(request.Name));
                return methodResult;
            }
            FinalTest finalTest = _mapper.Map<FinalTest>(request);
            if (!finalTest.IsValid())
            {
                methodResult.AddErrorBadRequest(finalTest.ErrorMessages);
                return methodResult;
            }
            foreach (var sectionGroup in request.SectionGroups)
            {
                if (sectionGroup == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(sectionGroup));
                    return methodResult;
                }
                if (sectionGroup.Sections == null || sectionGroup.Sections.Count == 0)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(sectionGroup.Sections));
                    return methodResult;
                }
                sectionGroup.Sections = sectionGroup.Sections.OrderBy(x => x.DisplayOrder).Select((x, index) => { x.DisplayOrder = index; return x; }).ToList();
                SectionGroup newSectionGroup = _mapper.Map<SectionGroup>(sectionGroup);
                if (!newSectionGroup.IsValid())
                {
                    methodResult.AddErrorBadRequest(newSectionGroup.ErrorMessages);
                    return methodResult;
                }
                var method = _sectionGroupManagerConverter.AddSessionToSessionGroup(newSectionGroup, sectionGroup.Sections);
                if (!method.IsOK)
                {
                    methodResult.AddErrorBadRequest(method.ErrorMessages);
                    return methodResult;
                }
                finalTest.FinalTestSections.Add(new FinalTestSection
                {
                    SectionGroup = newSectionGroup
                });
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
