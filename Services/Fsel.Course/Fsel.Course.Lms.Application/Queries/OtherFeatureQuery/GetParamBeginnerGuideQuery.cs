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
        private readonly IVideoTimeCodeAnswerRepository _videoTimeCodeAnswerRepository;
        private readonly IVideoTimeCodeResultRepository _videoTimeCodeResultRepository;
        private readonly IPlacementTestAnswerRepository _placementTestAnswerRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly ISectionQuestionRepository _sectionQuestionRepository;

        public GetParamBeginnerGuideQueryHandler(IPlacementTestResultRepository placementTestResultRepository,
            IUserService userService,
            IVideoResultRepository videoResultRepository,
            IHomeWorkResultRepository homeWorkResultRepository,
            IClassForumResultRepository classForumResultRepository,
            IVideoTimeCodeAnswerRepository videoTimeCodeAnswerRepository,
            IPlacementTestAnswerRepository placementTestAnswerRepository,
            IQuestionRepository questionRepository,
            IVideoTimeCodeResultRepository videoTimeCodeResultRepository,
            ISectionQuestionRepository sectionQuestionRepository)
        {
            _placementTestResultRepository = placementTestResultRepository;
            _userService = userService;
            _videoResultRepository = videoResultRepository;
            _homeWorkResultRepository = homeWorkResultRepository;
            _classForumResultRepository = classForumResultRepository;
            _videoTimeCodeAnswerRepository = videoTimeCodeAnswerRepository;
            _placementTestAnswerRepository = placementTestAnswerRepository;
            _questionRepository = questionRepository;
            _videoTimeCodeResultRepository = videoTimeCodeResultRepository;
            _sectionQuestionRepository = sectionQuestionRepository;
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
            if (_videoTimeCodeAnswerRepository == null ||
                _questionRepository == null ||
                _videoTimeCodeResultRepository == null ||
                _placementTestAnswerRepository == null ||
                _placementTestResultRepository == null ||
                _sectionQuestionRepository == null)
            {
                throw new InvalidOperationException("One or more repositories are null.");
            }
            var tempQuestionTypes = (from vtca in _videoTimeCodeAnswerRepository.Queryable
                                     join q in _questionRepository.Queryable on vtca.QuestionId equals q.Id
                                     join vtcr in _videoTimeCodeResultRepository.Queryable on vtca.VideoTimeCodeResultId equals vtcr.Id
                                     select new
                                     {
                                         StudentId = vtcr.StudentId,
                                         QuestionType = q.QuestionType
                                     }
                                     ).Union(
                                         from pta in _placementTestAnswerRepository.Queryable
                                         join ptr in _placementTestResultRepository.Queryable on pta.PlacementTestResultId equals ptr.Id
                                         join sq in _sectionQuestionRepository.Queryable on pta.SectionQuestionId equals sq.Id
                                         join q in _questionRepository.Queryable on sq.QuestionId equals q.Id
                                         select new
                                         {
                                             StudentId = ptr.StudentId,
                                             QuestionType = q.QuestionType
                                         }
                                     ).Distinct();

            var studentsTemp = (from s in students
                                join p in _placementTestResultRepository.Queryable on s.Id equals p.StudentId into placementTestResults
                                join v in _videoResultRepository.Queryable on s.Id equals v.StudentId into videoResults
                                join h in _homeWorkResultRepository.Queryable on s.Id equals h.StudentId into homeWorkResults
                                join c in _classForumResultRepository.Queryable on s.Id equals c.StudentId into classForumResults
                                join tq in tempQuestionTypes on s.Id equals tq.StudentId into questionTypes
                                from tqLeft in questionTypes.DefaultIfEmpty()
                                where request.ListStudentIds.Contains(s.Id)
                                select new
                                {
                                    Student = s,
                                    PlacementTestResults = placementTestResults,
                                    VideoResults = videoResults,
                                    HomeWorkResults = homeWorkResults,
                                    ClassForumResults = classForumResults,
                                    QuestionTypes = questionTypes.Select(q => q.QuestionType).Distinct().ToList()
                                }).ToList();

            foreach (var student in studentsTemp)
            {
                var paramBeginnerGuid = new ParamBeginnerGuideModel
                {
                    StudentId = student.Student.Id,
                    IsDoneOnePT = student.PlacementTestResults?.Any(x => x.Status == EnumResultStatus.Done) ?? false,
                    IsDoneVideo = student.VideoResults?.Any(x => x.Status == EnumResultStatus.Done) ?? false,
                    IsDoneHomeWork = student.HomeWorkResults?.Any(x => x.Status == EnumResultStatus.Done) ?? false,
                    IsDoneClassForum = student.ClassForumResults?.Any(cf => cf.ClassForumDetailResults.Any(y => y.Status != EnumClassForumResultStatus.Draft)) ?? false,
                    QuestionTypes = student.QuestionTypes?.ToList()
                };

                int age = DateTimeHelper.GetYearOld(student?.Student.User?.Birthday);

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
