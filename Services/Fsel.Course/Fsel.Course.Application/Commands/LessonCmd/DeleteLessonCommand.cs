using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Core.Base.BaseModels;
using Fsel.Course.Common.Models.Commands.Lesson;
using Fsel.Course.Common.Models.Entities;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Infrastructure.Repositories;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Application.Commands.LessonCmd
{
    public class DeleteLessonCommand : IRequest<MethodResult<Guid>>
    {
        public Guid Id { get; set; }
    }
    public class DeleteLessonCommandHandler : IRequestHandler<DeleteLessonCommand, MethodResult<Guid>>
    {
        private readonly ILessonRepository _lessonRepository;
        private readonly IMapper _mapper;

        public DeleteLessonCommandHandler(ILessonRepository lessonRepository,
            IMapper mapper)
        {
            _lessonRepository = lessonRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<Guid>> Handle(DeleteLessonCommand request, CancellationToken cancellationToken)
        {
            MethodResult<Guid> methodResult = new MethodResult<Guid>();

            #region Validation
            var lesson = await _lessonRepository.GetByIdAsync(request.Id);

            if (lesson == null )
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddErrorMessage("Lesson not found");
                return methodResult;
            }
            #endregion

            await _lessonRepository.ExecuteTransactionAsync(async () => {
                var IsLesson = await _lessonRepository.DeleteAsync(lesson);
                if (IsLesson == true)
                {
                    await _lessonRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    methodResult.StatusCode = StatusCodes.Status201Created;
                    methodResult.AddErrorMessage("Delete Lesson successfull");
                    methodResult.Result = lesson.Id;
                    return methodResult;
                }
                else
                {
                    methodResult.StatusCode = StatusCodes.Status400BadRequest;
                    methodResult.AddErrorMessage("Delete Lesson fails");
                    return methodResult;
                }
                
            });

            return methodResult;
        }
    }
}