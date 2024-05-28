// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.FeatureAccessTimeCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Shared.Enums;
    using Fsel.System.Application.Commands.QuestBoardCmd;
    using Fsel.System.Application.Services.UserServices;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.FeatureAccessTimes;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SaveFeatureAccessTimeCommand : SaveFeatureAccessTimeCommandModel, IRequest<MethodResult<FeatureAccessTimeModel>>
    {
    }

    public class SaveFeatureAccessTimeCommandHandler : IRequestHandler<SaveFeatureAccessTimeCommand, MethodResult<FeatureAccessTimeModel>>
    {
        private readonly IMapper _mapper;
        private readonly IFeatureAccessTimeRepository _featureAccessTimeRepository;
        private readonly AuthContext _authContext;
        private readonly IMediator _mediator;
        private readonly IUserService _userService;

        public SaveFeatureAccessTimeCommandHandler(IMapper mapper, IFeatureAccessTimeRepository featureAccessTimeRepository, AuthContext authContext, IMediator mediator, IUserService userService)
        {
            _mapper = mapper;
            _featureAccessTimeRepository = featureAccessTimeRepository;
            _authContext = authContext;
            _mediator = mediator;
            _userService = userService;
        }

        public async Task<MethodResult<FeatureAccessTimeModel>> Handle(SaveFeatureAccessTimeCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<FeatureAccessTimeModel> methodResult = new MethodResult<FeatureAccessTimeModel>();

            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddError(studentResult.Error);
                return methodResult;
            }
            var student = studentResult.Content?.Result;

            await _featureAccessTimeRepository.ExecuteTransactionAsync(async () =>
            {
                var featureAccessTime = await _featureAccessTimeRepository.Queryable.OrderByDescending(x => x.LastVisited).FirstOrDefaultAsync(x => x.CreatedUserId == _authContext.CurrentUserId && (x.ObjectId == request.ObjectId || x.EnumFeature == EnumFeature.Other) && x.EnumFeature == request.EnumFeature, cancellationToken);

                if (featureAccessTime == null)
                {
                    featureAccessTime = AddNewFeatureAccessTime(request);
                }
                else if (featureAccessTime != null && !IsSameRangeHour(featureAccessTime))
                {
                    featureAccessTime = AddNewFeatureAccessTime(request);
                }
                else if (featureAccessTime != null && IsSameRangeHour(featureAccessTime))
                {
                    featureAccessTime = UpdateExistingFeatureAccessTime(featureAccessTime, request);
                }

                await _featureAccessTimeRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<FeatureAccessTimeModel>(featureAccessTime);

                if ((request.EnumFeature == EnumFeature.VideoLesson || request.EnumFeature == EnumFeature.HomeWork || request.EnumFeature == EnumFeature.MockTest || request.EnumFeature == EnumFeature.FinalTest) && request.AccessTime.HasValue)
                {
                    await DoQuestBoard(student!.Id, EnumQuestBoardCategory.ExploreTheLearningGalaxy, (int)request.AccessTime, cancellationToken);
                    await DoQuestBoard(student!.Id, EnumQuestBoardCategory.LearningSpaceship, (int)request.AccessTime, cancellationToken);
                }

                return methodResult;
            });

            return methodResult;
        }

        private async Task DoQuestBoard(Guid studentId, EnumQuestBoardCategory category, int value, CancellationToken cancellationToken)
        {
            await _mediator.Send(new DoQuestBoardCommand()
            {
                StudentID = studentId,
                Type = EnumQuestBoardType.LearningQuests,
                Category = category,
                Value = value
            }, cancellationToken);
        }

        private FeatureAccessTime AddNewFeatureAccessTime(SaveFeatureAccessTimeCommand request)
        {
            var featureAccessTime = _mapper.Map<FeatureAccessTime>(request);
            featureAccessTime.Visit = 1;
            featureAccessTime.LastVisited = DateTime.UtcNow;
            return _featureAccessTimeRepository.Add(featureAccessTime);
        }

        private FeatureAccessTime UpdateExistingFeatureAccessTime(FeatureAccessTime featureAccessTime, SaveFeatureAccessTimeCommand request)
        {
            if (request.AccessTime == null)
            {
                featureAccessTime.Visit += 1;
            }
            featureAccessTime.AccessTime += request.AccessTime ?? default;
            featureAccessTime.LastVisited = DateTime.UtcNow;

            return _featureAccessTimeRepository.Update(featureAccessTime);
        }

        private static bool IsSameRangeHour(FeatureAccessTime featureAccessTime)
        {
            bool isValid = false;
            var now = DateTime.UtcNow;
            var lastVisited = featureAccessTime.LastVisited;

            if (lastVisited!.Value.Year == now.Year
                       && lastVisited!.Value.Month == now.Month
                       && lastVisited!.Value.Day == now.Day
                       && lastVisited!.Value.Hour == now.Hour)
            {
                isValid = true;
            }
            return isValid;
        }
    }
}
