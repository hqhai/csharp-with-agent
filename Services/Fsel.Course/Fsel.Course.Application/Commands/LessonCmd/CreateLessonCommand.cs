using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Course.Common.Models.Commands.Lesson;
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

namespace Fsel.Course.Application.Commands.LessonCmd
{
    public class CreateLessonCommand : CreateLessonCommandModel, IRequest<MethodResult<LessonModel>>
    {
    }
    public class CreateLessonCommandHandler : IRequestHandler<CreateLessonCommand, MethodResult<LessonModel>>
    {
        private readonly ILessonRepository _lessonRepository;
        private readonly IMapper _mapper;
        public CreateLessonCommandHandler(ILessonRepository lessonRepository,
            IMapper mapper)
        {
            _lessonRepository = lessonRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<LessonModel>> Handle(CreateLessonCommand request, CancellationToken cancellationToken)
        {
            MethodResult<LessonModel> methodResult = new MethodResult<LessonModel>();

            #region Validation
            Lesson lesson = _mapper.Map<Lesson>(request);

            if (!lesson.IsValid())
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddResultFromErrorList(lesson.ErrorMessages);
                return methodResult;
            }
            #endregion

            await _lessonRepository.ExecuteTransactionAsync(async () => {
                lesson = _lessonRepository.Add(lesson);
                await _lessonRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<LessonModel>(lesson);
                return methodResult;
            });

            return methodResult;
        }
    }
}
