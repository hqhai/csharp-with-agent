// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.OtherFeatureQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetParamBeginnerGuideQuery : IRequest<MethodResult<IList<ParamBeginnerGuideModel>>>
    {
        public IList<Guid>? ListStudentIds { get; set; }
    }

    public class GetParamBeginnerGuideQueryHandler : IRequestHandler<GetParamBeginnerGuideQuery, MethodResult<IList<ParamBeginnerGuideModel>>>
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

        public async Task<MethodResult<IList<ParamBeginnerGuideModel>>> Handle(GetParamBeginnerGuideQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<ParamBeginnerGuideModel>> methodResult = new MethodResult<IList<ParamBeginnerGuideModel>>();

            var paramBeginnerGuids = new List<ParamBeginnerGuideModel>();
            var studentResult = await _userService.GetStudentsByStudentIdsAsync(request.ListStudentIds);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }
            var students = studentResult?.Content?.Result;

            if (students == null)
            {
                methodResult.Result = paramBeginnerGuids;
                return methodResult;
            }

            var studentsTemp = (from s in students
                                join p in _placementTestResultRepository.Queryable on s.Id equals p.StudentId into placementTestResults
                                join v in _videoResultRepository.Queryable on s.Id equals v.StudentId into videoResults
                                join h in _homeWorkResultRepository.Queryable on s.Id equals h.StudentId into homeWorkResults
                                join c in _classForumResultRepository.Queryable on s.Id equals c.StudentId into classForumResults
                                where request.ListStudentIds.Contains(s.Id)
                                select new
                                {
                                    Student = s,
                                    PlacementTestResults = placementTestResults,
                                    VideoResults = videoResults,
                                    HomeWorkResults = homeWorkResults,
                                    ClassForumResults = classForumResults,
                                }).ToList();

            foreach (var student in studentsTemp)
            {
                var paramBeginnerGuid = new ParamBeginnerGuideModel
                {
                    StudentId = student.Student.Id,
                    IsDoneOnePT = student.PlacementTestResults?.Any(x => x.Status == EnumResultStatus.Done) ?? false,
                    IsDoneVideo = student.VideoResults?.Any(x => x.Status == EnumResultStatus.Done) ?? false,
                    IsDoneHomeWork = student.HomeWorkResults?.Any(x => x.Status == EnumResultStatus.Done) ?? false,
                    IsDoneClassForum = student.ClassForumResults?.Any(cf => cf.ClassForumDetailResults.Any(y => y.Status != EnumClassForumResultStatus.Draft)) ?? false
                };

                int age = DateTimeHelper.GetYearOld(student?.Student.Human?.Birthday);

                var placementTestResultDone = student.PlacementTestResults?.Where(x => x.Status == EnumResultStatus.Done)
                                                                           .OrderByDescending(x => x.CreatedDate)
                                                                           .FirstOrDefault();

                var placementTestResultInitial = student.PlacementTestResults?.OrderBy(x => x.CreatedDate).FirstOrDefault();
                if (placementTestResultDone != null)
                {
                    var (levelNext, isLock) = placementTestResultDone.Level.GetLevelInScore(placementTestResultDone.Percent, IeltsScoreHelper.GetInitialAge(placementTestResultInitial?.Level, age));
                    paramBeginnerGuid.IsDonePT = isLock;
                }

                paramBeginnerGuids.Add(paramBeginnerGuid);
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = paramBeginnerGuids;
            return methodResult;
        }
    }
}
