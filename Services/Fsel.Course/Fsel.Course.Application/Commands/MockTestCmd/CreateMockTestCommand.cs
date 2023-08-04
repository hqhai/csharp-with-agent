// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.MockTestCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.MockTests;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateMockTestCommand : CreateMockTestCommandModel, IRequest<MethodResult<MockTestModel>>
    {
    }

    public class CreateMockTestCommandHandler : IRequestHandler<CreateMockTestCommand, MethodResult<MockTestModel>>
    {
        private readonly IMapper _mapper;
        private readonly IMockTestRepository _mockTestRepository;
        private readonly SectionConverter _sectionConverter;

        public CreateMockTestCommandHandler(IMapper mapper
            , IMockTestRepository mockTestRepository
            , SectionConverter sectionConverter
            )
        {
            _mapper = mapper;
            _mockTestRepository = mockTestRepository;
            _sectionConverter = sectionConverter;
        }

        public async Task<MethodResult<MockTestModel>> Handle(CreateMockTestCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<MockTestModel> methodResult = new MethodResult<MockTestModel>();

            #region Validation

            if (request.SectionGroups == null || request.SectionGroups.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.SectionGroups));
                return methodResult;
            }
            if (await _mockTestRepository.Queryable.AnyAsync(x => x.Name == request.Name, cancellationToken))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(request.Name));
                return methodResult;
            }
            MockTest mockTest = _mapper.Map<MockTest>(request);
            if (!mockTest.IsValid())
            {
                methodResult.AddErrorBadRequest(mockTest.ErrorMessages);
                return methodResult;
            }

            foreach (var sectionGroup in request.SectionGroups)
            {
                if (sectionGroup == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(sectionGroup));
                    return methodResult;
                }
                else
                {
                    if (sectionGroup.Sections == null || sectionGroup.Sections.Count == 0)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(sectionGroup.Sections));
                        return methodResult;
                    }

                    SectionGroup newSectionGroup = _mapper.Map<SectionGroup>(sectionGroup);
                    var method = _sectionConverter.AddSessionToSessionGroup(newSectionGroup, sectionGroup.Sections, EnumCourseType.Ielts);
                    if (!method.IsOK)
                    {
                        methodResult.AddErrorBadRequest(method.ErrorMessages);
                    }

                    mockTest.MockTestSections.Add(new MockTestSection
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

            if (!methodResult.IsOK)
            {
                return methodResult;
            }

            #endregion Validation

            await _mockTestRepository.ExecuteTransactionAsync(async () =>
            {
                mockTest = _mockTestRepository.Add(mockTest);
                await _mockTestRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<MockTestModel>(mockTest);
                return methodResult;
            });

            return methodResult;
        }
    }
}
