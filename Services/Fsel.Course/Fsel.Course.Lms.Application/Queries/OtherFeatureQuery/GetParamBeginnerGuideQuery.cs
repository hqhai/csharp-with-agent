// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.OtherFeatureQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetParamBeginnerGuideQuery : IRequest<MethodResult<ParamBeginnerGuideModel>>
    {
        public Guid StudentId { get; set; }
    }

    public class GetParamBeginnerGuideQueryHandler : IRequestHandler<GetParamBeginnerGuideQuery, MethodResult<ParamBeginnerGuideModel>>
    {
        private readonly IPlacementTestResultRepository _placementTestResultRepository;
        private readonly IUserService _userService;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IHomeWorkResultRepository _homeWorkResultRepository;
        private readonly IClassForumResultRepository _classForumResultRepository;

        public GetParamBeginnerGuideQueryHandler(IPlacementTestResultRepository placementTestResultRepository,
            IUserService userService,
            IVideoResultRepository videoResultRepository,
            IHomeWorkResultRepository homeWorkResultRepository,
            IClassForumResultRepository classForumResultRepository)
        {
            _placementTestResultRepository = placementTestResultRepository;
            _userService = userService;
            _videoResultRepository = videoResultRepository;
            _homeWorkResultRepository = homeWorkResultRepository;
            _classForumResultRepository = classForumResultRepository;
        }

        public async Task<MethodResult<ParamBeginnerGuideModel>> Handle(GetParamBeginnerGuideQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ParamBeginnerGuideModel> methodResult = new MethodResult<ParamBeginnerGuideModel>();

            var paramBeginnerGuid = new ParamBeginnerGuideModel();
            var studentResult = await _userService.GetUserByStudentId(request.StudentId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult?.Content?.Result;
            var studentId = student?.Id;
            paramBeginnerGuid.IsDoneOnePT = await _placementTestResultRepository.Queryable.AnyAsync(x => x.Status == EnumResultStatus.Done && x.StudentId == request.StudentId, cancellationToken);
            int age = DateTimeHelper.GetYearOld(student?.Human?.Birthday);
            var placementTestResultDone = await _placementTestResultRepository.Queryable.Where(x => x.Status == EnumResultStatus.Done && x.StudentId == studentId)
                                                                          .OrderByDescending(x => x.CreatedDate)
                                                                          .FirstOrDefaultAsync(cancellationToken);
            var placementTestResultInitial = await _placementTestResultRepository.Queryable.Where(x => x.StudentId == studentId)
                                                                        .OrderBy(x => x.CreatedDate)
                                                                        .FirstOrDefaultAsync(cancellationToken);
            if (placementTestResultDone != null)
            {
                var (levelNext, isLock) = placementTestResultDone.Level.GetLevelInScore(placementTestResultDone.Percent, IeltsScoreHelper.GetInitialAge(placementTestResultInitial?.Level, age));
                paramBeginnerGuid.IsDonePT = isLock;
            }
            paramBeginnerGuid.IsDoneVideo = await _videoResultRepository.Queryable.AnyAsync(x => x.Status == EnumResultStatus.Done && x.StudentId == studentId, cancellationToken);
            paramBeginnerGuid.IsDoneClassForum = await _classForumResultRepository.Queryable.Include(x => x.ClassForumDetailResults).Where(x => x.StudentId == studentId)
                                                 .AnyAsync(x => x.ClassForumDetailResults.Any(y => y.Status != EnumClassForumResultStatus.Draft), cancellationToken);
            paramBeginnerGuid.IsDoneHomeWork = await _homeWorkResultRepository.Queryable.AnyAsync(x => x.Status == EnumResultStatus.Done && x.StudentId == studentId, cancellationToken);
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = paramBeginnerGuid;
            return methodResult;
        }
    }
}
