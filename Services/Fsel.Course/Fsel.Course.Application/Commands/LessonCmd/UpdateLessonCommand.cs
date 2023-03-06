using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Course.Common.Models.Commands.Lesson;
using Fsel.Course.Common.Models.Entities;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums;
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
    public class UpdateLessonCommand : UpdateLessonCommandModel, IRequest<MethodResult<LessonModel>>
    {

    }
    public class UpdateLessonCommandHandler : IRequestHandler<UpdateLessonCommand, MethodResult<LessonModel>>
    {
        private readonly ILessonRepository _lessonRepository;
        private readonly IMapper _mapper;
        public UpdateLessonCommandHandler(ILessonRepository lessonRepository,
            IMapper mapper)
        {
            _lessonRepository = lessonRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<LessonModel>> Handle(UpdateLessonCommand request, CancellationToken cancellationToken)
        {
            MethodResult<LessonModel> methodResult = new MethodResult<LessonModel>();
            
            var updatelesson =await _lessonRepository.GetByIdAsync(request.Id);
            if(updatelesson == null)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddErrorMessage("Not Found Lesson");
            }
            else
            {
                _mapper.Map(request, updatelesson);
                await _lessonRepository.ExecuteTransactionAsync(async () =>
                {
                    var lesson = _lessonRepository.Update(updatelesson);
                    await _lessonRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                    methodResult.StatusCode = StatusCodes.Status201Created;
                    methodResult.Result = _mapper.Map<LessonModel>(lesson);
                    return methodResult;
                });  
            }
            return methodResult;
        }
    }
}
