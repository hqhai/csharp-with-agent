using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Helpers;
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

namespace Fsel.Course.Application.Commands.VideoCmd
{
    public class DeleteVideoCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }

        public class DeleteVideoCommandHandler : IRequestHandler<DeleteVideoCommand, MethodResult<bool>>
        {
            private readonly IVideoRepository _videoRepository;
            private readonly IMapper _mapper;

            public DeleteVideoCommandHandler(IVideoRepository videoRepository,
            IMapper mapper)
            {
                _videoRepository = videoRepository;
                _mapper = mapper;
            }

            public async Task<MethodResult<bool>> Handle(DeleteVideoCommand request, CancellationToken cancellationToken)
            {
                MethodResult<bool> methodResult = new MethodResult<bool>();

                #region Validation

                var IsLessonVideo = await _videoRepository.IsVideoLesson(request.Id);

                if (IsLessonVideo)
                {
                    methodResult.StatusCode = StatusCodes.Status400BadRequest;
                    methodResult.AddErrorMessage(
                        nameof(EnumVideoErrorCode.VD02V),
                        new[] { MethodHelper.GenerateErrorResult(nameof(request.Id), request.Id) });
                    return methodResult;
                }

                var video = await _videoRepository.Queryable
                                               .Include(i => i.VideoTimeCodes.Where(x => !x.IsDeleted))
                                               .ThenInclude(x => x.TimeCodeExcercises.Where(x => !x.IsDeleted && x.Excercise != null))
                                               .ThenInclude(x => x.Excercise)
                                               .ThenInclude(x => x.ExcerciseQuestions.Where(x => !x.IsDeleted))
                                               .ThenInclude(x => x.Question)
                                               .FirstOrDefaultAsync(x => x.Id == request.Id);
                if (video == null)
                {
                    methodResult.StatusCode = StatusCodes.Status400BadRequest;
                    methodResult.AddErrorMessage(
                        nameof(EnumVideoErrorCode.VD01V),
                        new[] { MethodHelper.GenerateErrorResult(nameof(request.Id), request.Id) });
                    return methodResult;
                }

                #endregion Validation

                await _videoRepository.ExecuteTransactionAsync(async () =>
                {
                    var result = await _videoRepository.DeleteAsync(video);
                    await _videoRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                    methodResult.StatusCode = StatusCodes.Status200OK;
                    methodResult.Result = result;
                    return methodResult;
                });

                return methodResult;
            }
        }
    }
}