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
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.FinalTests;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateFinalTestCommand : UpdateFinalTestCommandModel, IRequest<MethodResult<FinalTestModel>>
    {
    }

    public class UpdateFinalTestCommandHandler : IRequestHandler<UpdateFinalTestCommand, MethodResult<FinalTestModel>>
    {
        private readonly IMapper _mapper;
        private readonly IFinalTestRepository _finalTestRepository;
        private readonly SectionConverter _sectionConverter;
        private readonly ICourseUnitMockTestRepository _courseUnitMockTestRepository;

        public UpdateFinalTestCommandHandler(IMapper mapper
            , IFinalTestRepository finalTestRepository
            , SectionConverter sectionConverter,
ICourseUnitMockTestRepository courseUnitMockTestRepository)
        {
            _mapper = mapper;
            _finalTestRepository = finalTestRepository;
            _sectionConverter = sectionConverter;
            _courseUnitMockTestRepository = courseUnitMockTestRepository;
        }

        public async Task<MethodResult<FinalTestModel>> Handle(UpdateFinalTestCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<FinalTestModel> methodResult = new MethodResult<FinalTestModel>();

            #region Validation

            if (request.SectionGroups == null || !request.SectionGroups.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.SectionGroups));
                return methodResult;
            }
            if (await _finalTestRepository.Queryable.AnyAsync(x => x.Id != request.Id && x.Name == request.Name, cancellationToken))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(request.Name));
                return methodResult;
            }
            var finalTest = await _finalTestRepository.GetIncludeByIdAsync(request.Id);
            if (finalTest == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(finalTest));
                return methodResult;
            }
            if (await _courseUnitMockTestRepository.Queryable.AnyAsync(p => p.FinalTestId == finalTest.Id, cancellationToken))
            {
                methodResult.AddErrorBadRequest(nameof(EnumFinalTestErrorCode.FinalTestInActiveState));
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
                //if (sectionGroup == null)
                //{
                //    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(sectionGroup));
                //    return methodResult;
                //}
                if (sectionGroup != null)
                {
                    var newSectionGroup = _mapper.Map<SectionGroup>(sectionGroup);
                    //if (sectionGroup.Sections == null || sectionGroup.Sections.Count == 0)
                    //{
                    //    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(sectionGroup.Sections));
                    //    return methodResult;
                    //}
                    var method = _sectionConverter.AddSessionToSessionGroup(newSectionGroup, sectionGroup.Sections, EnumCourseType.Academic);
                    if (!method.IsOK)
                    {
                        methodResult.AddErrorBadRequest(method.ErrorMessages);
                    }
                    finalTest.FinalTestSections.Add(new FinalTestSection { SectionGroup = newSectionGroup });
                    if (!newSectionGroup.IsValid())
                    {
                        methodResult.AddErrorBadRequest(newSectionGroup.ErrorMessages);
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
