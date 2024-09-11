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
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.MockTests;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using static Fsel.Shared.Constants.ValueSettings;

    public class CreateMockTestCommand : CreateMockTestCommandModel, IRequest<MethodResult<MockTestModel>>
    {
    }

    public class CreateMockTestCommandHandler : IRequestHandler<CreateMockTestCommand, MethodResult<MockTestModel>>
    {
        private readonly IMapper _mapper;
        private readonly IMockTestRepository _mockTestRepository;
        private readonly SectionGroupManagerConverter _sectionGroupManagerConverter;

        public CreateMockTestCommandHandler(IMapper mapper
            , IMockTestRepository mockTestRepository
            , SectionGroupManagerConverter sectionGroupManagerConverter
            )
        {
            _mapper = mapper;
            _mockTestRepository = mockTestRepository;
            _sectionGroupManagerConverter = sectionGroupManagerConverter;
        }

        public async Task<MethodResult<MockTestModel>> Handle(CreateMockTestCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<MockTestModel> methodResult = new MethodResult<MockTestModel>();

            var maxVersion = await _mockTestRepository.Queryable.MaxAsync(x => x.Version, cancellationToken);
            if (request.Version < maxVersion)
            {
                methodResult.AddErrorBadRequest(nameof(EnumMockTestErrorCode.VersionTooLow), nameof(request.Version));
                return methodResult;
            }

            #region Validation

            if (request.SectionGroups == null || (!request.SectionGroups.Any() || request.SectionGroups.Any(x => x == null)))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.SectionGroups));
                return methodResult;
            }
            if (request.MockTestType == EnumMockTestType.SkillMockTest && request.SectionGroups.Count != 1)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(request.SectionGroups));
                return methodResult;
            }
            else if (request.MockTestType == EnumMockTestType.FullMockTest && request.SectionGroups.Count != 4)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(request.SectionGroups));
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
                SectionGroup newSectionGroup = _mapper.Map<SectionGroup>(sectionGroup);
                if (sectionGroup.Sections != null && sectionGroup.Sections.Any())
                {
                    newSectionGroup.Sections = newSectionGroup.Sections.OrderBy(x => x.DisplayOrder).Select((x, index) => { x.DisplayOrder = index; return x; }).ToList();
                    if (!newSectionGroup.IsValid())
                    {
                        methodResult.AddErrorBadRequest(newSectionGroup.ErrorMessages);
                        return methodResult;
                    }
                    var method = _sectionGroupManagerConverter.AddSessionToSessionGroup(newSectionGroup, sectionGroup.Sections, EnumCourseType.Ielts);
                    if (!method.IsOK)
                    {
                        methodResult.AddErrorBadRequest(method.ErrorMessages);
                        return methodResult;
                    }
                }
                newSectionGroup.ExecutionTime = GetTimeSkill(sectionGroup.CourseSkill, sectionGroup.AudioPath);
                mockTest.MockTestSections.Add(new MockTestSection
                {
                    SectionGroup = newSectionGroup
                });
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

        public double GetTimeSkill(EnumCourseSkill skill, string? fileAudio)
        {
            if (skill == EnumCourseSkill.Reading)
            {
                return SectionGroupIELST.ExecutionTimeReading;
            }
            else if (skill == EnumCourseSkill.Listening && !string.IsNullOrEmpty(fileAudio))
            {
                return (MediaHelper.GetMediaDurationAsync(fileAudio) ?? default) + SectionGroupIELST.AdditionalTimeListening;
            }
            else if (skill == EnumCourseSkill.Speaking && !string.IsNullOrEmpty(fileAudio))
            {
                return (MediaHelper.GetMediaDurationAsync(fileAudio) ?? default); //thời gian audio
            }
            return default;
        }
    }
}
