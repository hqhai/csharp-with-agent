// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Queries.StudentGameInfoQuery
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Cms.PlanetDefender.Application.Services.UserServices;
    using Fsel.Cms.PlanetDefender.Domain.IRepositories;
    using Fsel.Cms.PlanetDefender.Domain.Models.EntityModels;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentGameInfoQuery : IRequest<MethodResult<StudentGameInfoModel>>
    {
    }

    public class GetStudentGameInfoQueryHandler : IRequestHandler<GetStudentGameInfoQuery, MethodResult<StudentGameInfoModel>>
    {
        private readonly IStudentGameInfoRepository _studentGameInfoRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IGameHistoryRepository _gameHistoryRepository;
        private readonly IMapper _mapper;

        public GetStudentGameInfoQueryHandler(IStudentGameInfoRepository studentGameInfoRepository, AuthContext authContext, IUserService userService, IGameHistoryRepository gameHistoryRepository, IMapper mapper)
        {
            _studentGameInfoRepository = studentGameInfoRepository;
            _authContext = authContext;
            _userService = userService;
            _gameHistoryRepository = gameHistoryRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<StudentGameInfoModel>> Handle(GetStudentGameInfoQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<StudentGameInfoModel>();
            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            var studentId = studentResult.Content?.Result?.Id;
            var gameHistory = await _gameHistoryRepository.Queryable.Where(x => x.StudentId == studentId).OrderByDescending(p => p.RoundNumber).FirstOrDefaultAsync(cancellationToken);

            var roundNumber = gameHistory?.RoundNumber;
            var score = gameHistory?.Score;

            var studentGameInfo = await _studentGameInfoRepository.Queryable
                        .Where(x => x.StudentId == studentId)
                        .Select(x => new StudentGameInfoModel
                        {
                            Id = x.Id,
                            StudentId = x.StudentId,
                            CreatedDate = x.CreatedDate,
                            Gender = x.Gender,
                            Level = x.Level,
                            TagNameId = x.TagNameId,
                            CourseLevel = x.CourseLevel,
                            NickName = x.NickName,
                            AvatarImageId = x.AvatarImageId,
                            HighestRoundNumber = roundNumber,
                            MaxScore = score
                        }).FirstOrDefaultAsync(cancellationToken);

            methodResult.Result = studentGameInfo;
            return methodResult;
        }
    }
}
