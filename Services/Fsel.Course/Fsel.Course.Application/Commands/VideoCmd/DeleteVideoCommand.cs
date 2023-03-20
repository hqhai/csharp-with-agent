using Fsel.Common.ActionResults;
using Fsel.Common.Helpers;
using Fsel.Course.Domain.Enums.ErrorCodes;
using Fsel.Course.Domain.IRepositories;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Application.Commands.VideoCmd
{
    public class DeleteVideoCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }

        public class DeleteVideoCommandHandler : IRequestHandler<DeleteVideoCommand, MethodResult<bool>>
        {
            private readonly IVideoRepository _videoRepository;

            public DeleteVideoCommandHandler(IVideoRepository videoRepository)
            {
                _videoRepository = videoRepository;
            }

            public async Task<MethodResult<bool>> Handle(DeleteVideoCommand request, CancellationToken cancellationToken)
            {
                MethodResult<bool> methodResult = new MethodResult<bool>();

                #region Validation

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
                    methodResult.AddError(
                        nameof(EnumVideoErrorCode.VD01V),
                        new[] { MethodHelper.GenerateErrorResult(nameof(request.Id), request.Id) });
                    return methodResult;
                }

                var isVideoUsed = await _videoRepository.IsVideoUsed(request.Id);

                if (isVideoUsed)
                {
                    methodResult.StatusCode = StatusCodes.Status400BadRequest;
                    methodResult.AddError(
                        nameof(EnumVideoErrorCode.VD02V),
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
