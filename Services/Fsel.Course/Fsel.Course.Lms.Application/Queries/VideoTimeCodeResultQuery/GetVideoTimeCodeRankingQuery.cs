// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.VideoTimeCodeResultQuery
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.TrainingServices;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetVideoTimeCodeRankingQuery : IRequest<MethodResult<IList<TestResultRankingModel>>>
    {
        public Guid VideoTimeCodeResultId { get; set; }
    }

    public class GetVideoTimeCodeRankingQueryHandler : IRequestHandler<GetVideoTimeCodeRankingQuery, MethodResult<IList<TestResultRankingModel>>>
    {
        private readonly IVideoTimeCodeResultRepository _videoTimeCodeResultRepository;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly ITrainingService _trainingService;

        public GetVideoTimeCodeRankingQueryHandler(IVideoTimeCodeResultRepository videoTimeCodeResultRepository, IMapper mapper, IUserService userService, ITrainingService trainingService)
        {
            _videoTimeCodeResultRepository = videoTimeCodeResultRepository;
            _mapper = mapper;
            _userService = userService;
            _trainingService = trainingService;
        }

        public async Task<MethodResult<IList<TestResultRankingModel>>> Handle(GetVideoTimeCodeRankingQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<TestResultRankingModel>> methodResult = new MethodResult<IList<TestResultRankingModel>>();

            List<TestResultRankingModel> testResultRankings = new List<TestResultRankingModel>();

            var videoTimeCodeResult = await _videoTimeCodeResultRepository.GetByIdAsync(request.VideoTimeCodeResultId);

            if (videoTimeCodeResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoTimeCodeResult));
                return methodResult;
            }

            var currentClass = await _trainingService.GetClassByStudentId(videoTimeCodeResult.StudentId);
            var classStudentIds = currentClass.Content?.Result?.ClassStudents?.Select(x => x.StudentId).ToList();

            var videoTimeCodeResults = await _videoTimeCodeResultRepository.Queryable
                            .Include(x => x.VideoTimeCode)
                            .Where(x => x.VideoTimeCodeId == videoTimeCodeResult!.VideoTimeCodeId && classStudentIds!.Contains(x.StudentId) && x.VideoTimeCode!.TimeCodeType != EnumTimeCodeType.Standalone)
                            .ToListAsync(cancellationToken);

            var studentResults = await _userService.GetStudentsByStudentIdsAsync(classStudentIds);
            var students = studentResults?.Content?.Result;

            if (students != null)
            {
                foreach (var item in students)
                {
                    var videoTimeCodeResultStudent = videoTimeCodeResults.FirstOrDefault(x => x.StudentId == item.Id);
                    var videoTimeCodeResultDto = _mapper.Map<TestResultRankingModel>(videoTimeCodeResultStudent);
                    if (videoTimeCodeResultDto != null && videoTimeCodeResultStudent != null)
                    {
                        videoTimeCodeResultDto.IsCurrentStudent = item.Id == videoTimeCodeResult.StudentId;
                        videoTimeCodeResultDto.WorkingTime = DateTimeHelper.GetWorkingTime(videoTimeCodeResultStudent.CreatedDate, videoTimeCodeResultStudent.UpdatedDate ?? DateTime.UtcNow, videoTimeCodeResults.Where(x => x.StudentId == item.Id).Select(x => x.VideoTimeCode!.ExecutionTime).FirstOrDefault());
                    }
                    else
                    {
                        videoTimeCodeResultDto = new TestResultRankingModel();
                    }
                    videoTimeCodeResultDto.FullName = item.Human?.FullName;
                    videoTimeCodeResultDto.AvatarPath = item.Human?.AvatarPath;
                    testResultRankings.Add(videoTimeCodeResultDto);
                }
            }

            methodResult.Result = testResultRankings.OrderByDescending(x => x.Percent).ThenBy(x => x.FullName).ToList();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
