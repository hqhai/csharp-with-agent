// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.CourseStudentCmd
{
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.CourseStudents;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateCourseStudentCommand : CreateCourseStudentCommandModel, IRequest<MethodResult<CourseModel>>
    {
    }

    public class CreateCourseStudentCommandHandler : IRequestHandler<CreateCourseStudentCommand, MethodResult<CourseModel>>
    {
        private readonly ICourseStudentRepository _courseStudentRepository;
        private readonly IMapper _mapper;
        private readonly ICourseRepository _courseRepository;

        public CreateCourseStudentCommandHandler(ICourseStudentRepository courseStudentRepository
            , IMapper mapper
            , ICourseRepository courseRepository)
        {
            _courseStudentRepository = courseStudentRepository;
            _mapper = mapper;
            _courseRepository = courseRepository;
        }

        public async Task<MethodResult<CourseModel>> Handle(CreateCourseStudentCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<CourseModel> methodResult = new MethodResult<CourseModel>();
            var course = await _courseRepository.Queryable.Include(e => e.CourseStudents)
                                                    .FirstOrDefaultAsync(e => e.Id == request.CourseId, cancellationToken: cancellationToken);
            if (course == null)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddError(
                    nameof(EnumCourseErrorCode.CourseNotExist));
                return methodResult;
            }

            if (!course.IsValid())
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddResultFromErrorList(course.ErrorMessages);
                return methodResult;
            }

            course.CourseStudents.Add(new Domain.Entities.CourseStudent { CourseId = request.CourseId, StudentId = request.StudentId });
            await _courseRepository.ExecuteTransactionAsync(async () =>
            {
                course = _courseRepository.Update(course);

                await _courseRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<CourseModel>(course);
                return methodResult;
            });

            return methodResult;
        }
    }
}
