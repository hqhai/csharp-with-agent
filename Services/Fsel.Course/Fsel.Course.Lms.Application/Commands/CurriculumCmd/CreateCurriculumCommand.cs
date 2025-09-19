// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.CurriculumCmd
{
    using System.Security.Claims;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.Curriculums;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Commands.CourseCmd;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateCurriculumCommand : CreateCurriculumCommandModel, IRequest<MethodResult<CurriculumModel>>
    {
    }

    public class CreateCurriculumCommandHandler : IRequestHandler<CreateCurriculumCommand, MethodResult<CurriculumModel>>
    {
        private readonly ICurriculumRepository _curriculumRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly AuthContext _authContext;

        public CreateCurriculumCommandHandler(ICurriculumRepository curriculumRepository, IMapper mapper, IMediator mediator, AuthContext authContext)
        {
            _curriculumRepository = curriculumRepository;
            _mapper = mapper;
            _mediator = mediator;
            _authContext = authContext;
        }

        public async Task<MethodResult<CurriculumModel>> Handle(CreateCurriculumCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<CurriculumModel>();

            #region validation

            var schoolIdStr = _authContext.ClaimsPrincipal?.FindFirstValue("SchoolId");

            if (string.IsNullOrEmpty(schoolIdStr) || !Guid.TryParse(schoolIdStr, out Guid schoolId))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(schoolId), _authContext.CurrentUserId);
                return methodResult;
            }

            if (string.IsNullOrWhiteSpace(request.CurriculumName))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required), nameof(request.CurriculumName), request.CurriculumName);
                return methodResult;
            }

            var currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);

            if (currentDate < request.StartDate)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCurriculumErrorCode.StartDateCannotBeInThePast), nameof(request.StartDate), request.StartDate);
                return methodResult;
            }

            if (request.EndDate < request.StartDate)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCurriculumErrorCode.InvalidDateRange), nameof(request.StartDate), request.StartDate);
                return methodResult;
            }

            var existingCurriculum = await _curriculumRepository.Queryable.FirstOrDefaultAsync(c => c.CurriculumName == request.CurriculumName, cancellationToken);
            if (existingCurriculum != null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCurriculumErrorCode.AlreadyExistCurriculumName), nameof(request.CurriculumName), request.CurriculumName);
                return methodResult;
            }

            #endregion validation

            await _curriculumRepository.ExecuteTransactionAsync(async () =>
            {
                var cloneCourseResult = await _mediator.Send(new CloneCourseCommand() { CourseId = request.CourseId });
                if (!cloneCourseResult.IsOK)
                {
                    methodResult.AddError(cloneCourseResult.ErrorMessages);
                    return methodResult;
                }

                var cloneCourse = cloneCourseResult.Result;

                if (cloneCourse == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(cloneCourse));
                    return methodResult;
                }

                var curriculum = _mapper.Map<CurriculumConfig>(request);
                curriculum.CourseId = request.CourseId;
                curriculum.CourseCloneId = cloneCourse.Id;
                curriculum.SchoolId = schoolId;

                if (!curriculum.IsValid())
                {
                    methodResult.AddError(curriculum.ErrorMessages);
                    return methodResult;
                }

                _curriculumRepository.Add(curriculum);
                await _curriculumRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<CurriculumModel>(curriculum);
                return methodResult;
            });

            return methodResult;
        }
    }
}
