using AutoMapper;
using Fsel.Common.ActionResults;

using Fsel.Course.Common.Models.Commands.Course;
using Fsel.Course.Common.Models.Entities;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.IRepositories;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntityCourse = Fsel.Course.Domain.Entities.Course;

namespace Fsel.Course.Application.Commands.Course
{
    public class CreateCourseCommand : CreateCourseCommandModel, IRequest<MethodResult<CourseModel>>
    {
    }

    public class CreateCourseCommandHandler : IRequestHandler<CreateCourseCommand, MethodResult<CourseModel>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;

        public CreateCourseCommandHandler(ICourseRepository courseRepository,
            IMapper mapper)
        {
            _courseRepository = courseRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<CourseModel>> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
        {
            MethodResult<CourseModel> methodResult = new MethodResult<CourseModel>();

            #region Validation

            EntityCourse course = _mapper.Map<EntityCourse>(request);

            if (!course.IsValid())
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddResultFromErrorList(course.ErrorMessages);
                return methodResult;
            }

            #endregion Validation

            await _courseRepository.ExecuteTransactionAsync(async () =>
            {
                course = _courseRepository.Add(course);
                await _courseRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<CourseModel>(course);
                return methodResult;
            });

            return methodResult;
        }
    }
}