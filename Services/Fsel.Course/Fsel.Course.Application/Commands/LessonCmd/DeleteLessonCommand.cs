using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Helpers;
using Fsel.Core.Base.BaseModels;
using Fsel.Course.Application.Commands.PlacementTestCmd;
using Fsel.Course.Common.Models.Commands.Lesson;
using Fsel.Course.Common.Models.Entities;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums.ErrorCodes;
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
    public class DeleteLessonCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class DeleteLessonCommandHandler : IRequestHandler<DeleteLessonCommand, MethodResult<bool>>
    {
        private readonly ILessonRepository _lessonRepository;
        private readonly IMapper _mapper;

        public DeleteLessonCommandHandler(ILessonRepository lessonRepository,
            IMapper mapper)
        {
            _lessonRepository = lessonRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<bool>> Handle(DeleteLessonCommand request, CancellationToken cancellationToken)
        {
            MethodResult<bool> methodResult = new MethodResult<bool>();

            #region Validation

            var IsUnitLesson = await _lessonRepository.IsUnitLesson(request.Id);

            if (IsUnitLesson)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddErrorMessage(
                    nameof(EnumLessonErrorCode.LS02V),
                    new[] { MethodHelper.GenerateErrorResult(nameof(request.Id), request.Id) });
                return methodResult;
            }

            var lesson = await _lessonRepository.GetByIdAsync(request.Id);
            if (lesson == null)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddErrorMessage(
                    nameof(EnumLessonErrorCode.LS01V),
                    new[] { MethodHelper.GenerateErrorResult(nameof(request.Id), request.Id) });
                return methodResult;
            }

            #endregion Validation

            await _lessonRepository.ExecuteTransactionAsync(async () =>
            {
                var result = await _lessonRepository.DeleteAsync(lesson);
                await _lessonRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = result;
                return methodResult;
            });

            return methodResult;
        }
    }
}