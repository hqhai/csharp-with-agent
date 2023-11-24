// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.VideoTimeCodeResultQuery
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.VideoTimeCodeResults;
    using Fsel.Course.Lms.Application.Services.TrainingServices;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetVideoTimeCodeResultQuery : IRequest<MethodResult<VideoTimeCodeResultByStudentModel>>
    {
        public Guid VideoTimeCodeResultId { get; set; }

        public Guid VideoTimeCodeId { get; set; }
    }

    public class GetVideoTimeCodeResultQueryHandler : IRequestHandler<GetVideoTimeCodeResultQuery, MethodResult<VideoTimeCodeResultByStudentModel>>
    {
        private readonly IVideoTimeCodeResultRepository _videoTimeCodeResultRepository;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly ITrainingService _trainingService;
        private readonly AuthContext _authContext;

        public GetVideoTimeCodeResultQueryHandler(IVideoTimeCodeResultRepository videoTimeCodeResultRepository, IMapper mapper, IUserService userService, ITrainingService trainingService, AuthContext authContext)
        {
            _videoTimeCodeResultRepository = videoTimeCodeResultRepository;
            _mapper = mapper;
            _userService = userService;
            _trainingService = trainingService;
            _authContext = authContext;
        }

        public async Task<MethodResult<VideoTimeCodeResultByStudentModel>> Handle(GetVideoTimeCodeResultQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<VideoTimeCodeResultByStudentModel> methodResult = new MethodResult<VideoTimeCodeResultByStudentModel>();

            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            var student = studentResult?.Content?.Result;

            IList<Guid>? classStudentIds = new List<Guid>();

            var currentClass = await _trainingService.GetClassByStudentId(student.Id);
            classStudentIds = currentClass.Content?.Result?.ClassStudents?.Select(x => x.StudentId).ToList();

            var query = await _videoTimeCodeResultRepository.Queryable
                .Where(x => classStudentIds!.Contains(x.StudentId))
                .OrderBy(x => x.CreatedDate)
                .ToListAsync(cancellationToken);

            var vdeoTimeCodeResults = _mapper.Map<IList<VideoTimeCodeResultModel>>(query);

            var videoTimeCodeResult = await _videoTimeCodeResultRepository.Queryable.FirstOrDefaultAsync(x => x.Id == request.VideoTimeCodeResultId, cancellationToken);

            var videoTimeCodeResultByStudent = _mapper.Map<VideoTimeCodeResultByStudentModel>(videoTimeCodeResult);

            var currentVideoTimeCodeResult = vdeoTimeCodeResults.FirstOrDefault(x => x.VideoTimeCodeId == request.VideoTimeCodeId && x.StudentId == student.Id);
            videoTimeCodeResultByStudent.VideoTimeCodeResultCurrentStudent = currentVideoTimeCodeResult;

            var videoTimeCodeResultAllStudents = vdeoTimeCodeResults.Where(x => x.VideoTimeCodeId == request.VideoTimeCodeId).ToList();

            videoTimeCodeResultByStudent.VideoTimeCodeResultAllStudents = videoTimeCodeResultAllStudents;

            methodResult.Result = videoTimeCodeResultByStudent;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
