// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.LiveTimeFrameCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.LiveTimeFrames;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SaveListLiveTimeFrameCommand : SaveListLiveTimeFrameCommandModel, IRequest<MethodResult<IList<LiveTimeFrameModel>>>
    {
    }

    public class SaveListLiveTimeFrameCommandHandler : IRequestHandler<SaveListLiveTimeFrameCommand, MethodResult<IList<LiveTimeFrameModel>>>
    {
        private readonly IMapper _mapper;
        private readonly ILiveTimeFrameRepository _liveTimeFrameRepository;

        public SaveListLiveTimeFrameCommandHandler(IMapper mapper, ILiveTimeFrameRepository liveTimeFrameRepository)
        {
            _mapper = mapper;
            _liveTimeFrameRepository = liveTimeFrameRepository;
        }

        public async Task<MethodResult<IList<LiveTimeFrameModel>>> Handle(SaveListLiveTimeFrameCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<LiveTimeFrameModel>>();

            if (request.LiveTimeFrames == null || request.LiveTimeFrames.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumLiveTimeFrameErrorCode.LiveTimeFramesNotEmpty), nameof(request.LiveTimeFrames), request.LiveTimeFrames);
                return methodResult;
            }

            var createLiveTimeFrameResults = new List<LiveTimeFrame>();
            var updateLiveTimeFrameResults = new List<LiveTimeFrame>();
            var liveTimeFrameIds = request.LiveTimeFrames.Select(x => x.Id).ToArray();
            var deleteLiveTimeFrame = await _liveTimeFrameRepository.Queryable.Where(x => !liveTimeFrameIds.Contains(x.Id)).ToListAsync(cancellationToken);

            foreach (var item in request.LiveTimeFrames)
            {
                LiveTimeFrame? liveTimeFrameNew;
                if (item.Id.HasValue)
                {
                    liveTimeFrameNew = await _liveTimeFrameRepository.GetByIdAsync(item.Id.Value);
                    if (liveTimeFrameNew == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumLiveTimeFrameErrorCode.LiveTimeFramesNotEmpty));
                        return methodResult;
                    }
                    liveTimeFrameNew = _mapper.Map(item, liveTimeFrameNew);
                    updateLiveTimeFrameResults.Add(liveTimeFrameNew);
                }
                else
                {
                    var lessonResultNew = _mapper.Map<LiveTimeFrame>(item);
                    lessonResultNew = _liveTimeFrameRepository.Add(lessonResultNew);
                    if (!lessonResultNew.IsValid())
                    {
                        methodResult.AddErrorBadRequest(lessonResultNew.ErrorMessages);
                        return methodResult;
                    }
                    createLiveTimeFrameResults.Add(lessonResultNew);
                }
            }

            foreach (var item in deleteLiveTimeFrame)
            {
                await _liveTimeFrameRepository.DeleteAsync(item);
            }

            await _liveTimeFrameRepository.ExecuteTransactionAsync(async () =>
            {
                await _liveTimeFrameRepository.AddList(createLiveTimeFrameResults);
                _liveTimeFrameRepository.UpdateList(updateLiveTimeFrameResults);

                await _liveTimeFrameRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                return methodResult;
            });
            createLiveTimeFrameResults.AddRange(updateLiveTimeFrameResults);
            methodResult.StatusCode = StatusCodes.Status201Created;
            methodResult.Result = _mapper.Map<IList<LiveTimeFrameModel>>(createLiveTimeFrameResults);

            return methodResult;
        }
    }
}
