// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.VideoTimeCodeResultQuery
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
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

            var videoTimeCodeResult = await _videoTimeCodeResultRepository.GetByIdAsync(request.VideoTimeCodeResultId);

            var currentClass = await _trainingService.GetClassByStudentId(videoTimeCodeResult!.StudentId);
            var classStudentIds = currentClass.Content?.Result?.ClassStudents?.Select(x => x.StudentId).ToList();

            var videoTimeCodeResults = await _videoTimeCodeResultRepository.Queryable
                            .Include(x => x.VideoTimeCode)
                            .Where(x => x.VideoTimeCodeId == videoTimeCodeResult!.VideoTimeCodeId && classStudentIds!.Contains(x.StudentId) && x.VideoTimeCode!.TimeCodeType != EnumTimeCodeType.Standalone)
                            .ToListAsync(cancellationToken);

            var videoTimeCodeResultDtos = _mapper.Map<IList<TestResultRankingModel>>(videoTimeCodeResults);

            var studentResults = await _userService.GetStudentsByStudentIdsAsync(videoTimeCodeResultDtos.Select(x => x.StudentId).ToList());
            var students = studentResults?.Content?.Result?.OrderBy(x => x.Human?.FullName);

            foreach (var item in videoTimeCodeResultDtos)
            {
                item.IsCurrentStudent = item.StudentId == videoTimeCodeResult?.Id;
                item.WorkingTime = DateTimeHelper.GetWorkingTime(item.CreatedDate, item.UpdatedDate ?? DateTime.UtcNow, videoTimeCodeResults.Where(x => x.Id == item.Id).Select(x => x.VideoTimeCode!.ExecutionTime).FirstOrDefault());
                item.FullName = students?.FirstOrDefault(x => x.Id == item.StudentId)?.Human?.FullName;
                item.AvatarPath = students?.FirstOrDefault(x => x.Id == item.StudentId)?.Human?.AvatarPath;
            }
            methodResult.Result = videoTimeCodeResultDtos;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
