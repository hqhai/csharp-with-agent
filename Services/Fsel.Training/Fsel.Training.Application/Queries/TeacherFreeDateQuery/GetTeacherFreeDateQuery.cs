// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.TeacherFreeDateQuery
{
    using System;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Training.Application.Services.SystemServices;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetTeacherFreeDateQuery : IRequest<MethodResult<TeacherFreeDateModel>>
    {
        public Guid TeacherFreeDateId { get; set; }
    }

    public class GetTeacherFreeDateQueryHandler : IRequestHandler<GetTeacherFreeDateQuery, MethodResult<TeacherFreeDateModel>>
    {
        private readonly ITeacherFreeDateRepository _teacherFreeDateRepository;
        private readonly IMapper _mapper;
        private readonly ISystemService _systemService;

        public GetTeacherFreeDateQueryHandler(ITeacherFreeDateRepository teacherFreeDateRepository, IMapper mapper, ISystemService systemService)
        {
            _teacherFreeDateRepository = teacherFreeDateRepository;
            _mapper = mapper;
            _systemService = systemService;
        }

        public async Task<MethodResult<TeacherFreeDateModel>> Handle(GetTeacherFreeDateQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<TeacherFreeDateModel> methodResult = new MethodResult<TeacherFreeDateModel>();

            var teacherFreeDate = await _teacherFreeDateRepository.Queryable.Include(x => x.TeacherFreeTimes).FirstOrDefaultAsync(x => x.Id == request.TeacherFreeDateId, cancellationToken);
            var teacherFreeDateModel = _mapper.Map<TeacherFreeDateModel>(teacherFreeDate);

            var timeFramesResult = await _systemService.GetLiveTimeFramesAsync();
            var timeFrames = timeFramesResult.Content?.Result;
            if (teacherFreeDateModel.TeacherFreeTimes != null)
            {
                foreach (var item in teacherFreeDateModel.TeacherFreeTimes)
                {
                    var timeFrame = timeFrames?.FirstOrDefault(x => x.Id == item.LiveTimeFrameId);
                    item.StartTime = timeFrame?.StartTime;
                    item.EndTime = timeFrame?.EndTime;
                }
            }

            methodResult.Result = teacherFreeDateModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
