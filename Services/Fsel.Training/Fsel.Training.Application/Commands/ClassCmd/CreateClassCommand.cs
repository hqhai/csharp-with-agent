// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Commands.ClassCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Shared.Enums;
    using Fsel.Training.Application.Queries.ClassQuery;
    using Fsel.Training.Application.Services.CourseServices;
    using Fsel.Training.Domain.Entities;
    using Fsel.Training.Domain.Enums.ErrorCodes;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.CommandModels.Classes;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateClassCommand : CreateClassCommandModel, IRequest<MethodResult<ClassModel>>
    {
    }

    public class CreateClassCommandHandler : IRequestHandler<CreateClassCommand, MethodResult<ClassModel>>
    {
        private readonly IClassRepository _classRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly ICourseService _courseService;

        public CreateClassCommandHandler(IClassRepository classRepository, IMapper mapper, IMediator mediator, ICourseService courseService)
        {
            _classRepository = classRepository;
            _mapper = mapper;
            _mediator = mediator;
            _courseService = courseService;
        }

        public async Task<MethodResult<ClassModel>> Handle(CreateClassCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ClassModel> methodResult = new MethodResult<ClassModel>();

            var generateClassCode = await _mediator.Send(new GetNewClassCodeQuery { CourseLevel = request.CourseLevel, Code = request.CourseName }, cancellationToken).ConfigureAwait(false);
            var checkExistClassCode = await _classRepository.Queryable.AnyAsync(p => p.Code == generateClassCode.Result, cancellationToken);
            if (checkExistClassCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(checkExistClassCode));
                return methodResult;
            }
            var courseResult = await _courseService.GetListCourseByIds(new List<Guid> { request.CourseId });
            if (!courseResult.IsSuccessStatusCode)
            {
                methodResult.AddError(courseResult.Error);
                return methodResult;
            }
            var course = courseResult.Content?.Result?.FirstOrDefault();
            if (course == null || course.Status != EnumCourseStatus.Active)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassErrorCode.CourseNotExistOrNotActive));
                return methodResult;
            }
            await _classRepository.ExecuteTransactionAsync(async () =>
            {
                var newClass = _mapper.Map<Class>(request);
                newClass.Code = generateClassCode.Result;
                newClass.Name = generateClassCode.Result;
                newClass.Status = EnumClassStatus.New;
                if (!newClass.IsValid())
                {
                    methodResult.AddErrorBadRequest(newClass.ErrorMessages);
                    return methodResult;
                }
                newClass = _classRepository.Add(newClass);
                await _classRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<ClassModel>(newClass);
                return methodResult;
            });
            return methodResult;
        }
    }
}
