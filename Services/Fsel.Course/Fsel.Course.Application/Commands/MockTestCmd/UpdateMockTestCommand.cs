// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.MockTestCmd
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.MockTests;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateMockTestCommand : UpdateMockTestCommandModel, IRequest<MethodResult<MockTestModel>>
    {
    }

    public class UpdateMockTestCommandHandler : IRequestHandler<UpdateMockTestCommand, MethodResult<MockTestModel>>
    {
        private readonly IMapper _mapper;
        private readonly IMockTestRepository _mockTestRepository;
        private readonly SectionConverter _sectionConverter;

        public UpdateMockTestCommandHandler(IMapper mapper
            , IMockTestRepository mockTestRepository
            , SectionConverter sectionConverter)

        {
            _mapper = mapper;
            _mockTestRepository = mockTestRepository;
            _sectionConverter = sectionConverter;
            _sectionConverter = sectionConverter;
        }

        public async Task<MethodResult<MockTestModel>> Handle(UpdateMockTestCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<MockTestModel> methodResult = new MethodResult<MockTestModel>();

            #region Validation

            if (request.SectionGroups == null || request.SectionGroups.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.SectionGroups));
                return methodResult;
            }
            if (await _mockTestRepository.Queryable.AnyAsync(x => x.Id != request.Id && x.Name == request.Name, cancellationToken))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(request.Name));
                return methodResult;
            }

            var mockTest = await _mockTestRepository.GetIncludeByIdAsync(request.Id);
            if (mockTest == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(mockTest));
                return methodResult;
            }

            if (mockTest.CourseUnitMockTests.Any() || mockTest.UnitSkillMockTests.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumMockTestErrorCode.MockTestInActiveState));
                return methodResult;
            }

            var sectionGroups = mockTest.MockTestSections.Select(x => x.SectionGroup ?? new SectionGroup()).ToList();
            var sectionQuestions = sectionGroups.SelectMany(x => x.Sections).SelectMany(x => x.SectionParts).SelectMany(x => x.SectionQuestions).ToList();
            var questions = sectionQuestions.Select(x => x.Question ?? new Question()).ToList();

            _mapper.Map(request, mockTest);
            mockTest.MockTestSections.Clear();

            foreach (var sectionGroup in request.SectionGroups)
            {
                if (sectionGroup == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(sectionGroup));
                    return methodResult;
                }
                var newSectionGroup = _mapper.Map<SectionGroup>(sectionGroup);
                var method = _sectionConverter.AddSessionToSessionGroup(newSectionGroup, sectionGroup.Sections, EnumCourseType.Ielts);
                if (!method.IsOK)
                {
                    methodResult.AddErrorBadRequest(method.ErrorMessages);
                }
                mockTest.MockTestSections.Add(new MockTestSection { SectionGroup = newSectionGroup });
                if (!newSectionGroup.IsValid())
                {
                    methodResult.AddErrorBadRequest(newSectionGroup.ErrorMessages);
                }
            }
            if (!mockTest.IsValid())
            {
                methodResult.AddErrorBadRequest(mockTest.ErrorMessages);
                return methodResult;
            }
            else if (!methodResult.IsOK)
            {
                return methodResult;
            }

            #endregion Validation

            await _mockTestRepository.ExecuteTransactionAsync(async () =>
            {
                await _sectionConverter.DeleteSectionGroup(sectionGroups, sectionQuestions, questions);
                mockTest = _mockTestRepository.Update(mockTest);
                await _mockTestRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<MockTestModel>(mockTest);
                return methodResult;
            });

            return methodResult;
        }
    }
}
