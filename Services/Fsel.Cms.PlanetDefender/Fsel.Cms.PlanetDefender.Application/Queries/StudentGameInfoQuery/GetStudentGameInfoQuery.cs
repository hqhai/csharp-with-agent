// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Queries.StudentGameInfoQuery
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
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

        public GetStudentGameInfoQueryHandler(IStudentGameInfoRepository studentGameInfoRepository, AuthContext authContext, IUserService userService)
        {
            _studentGameInfoRepository = studentGameInfoRepository;
            _authContext = authContext;
            _userService = userService;
        }

        public async Task<MethodResult<StudentGameInfoModel>> Handle(GetStudentGameInfoQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<StudentGameInfoModel>();
            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            var studentId = studentResult.Content?.Result?.Id;
            var studentGameInfo = await _studentGameInfoRepository.Queryable
                        .Where(x => x.StudentId == studentId)
                        .Select(x => new StudentGameInfoModel
                        {
                            Id = x.Id,
                            StudentId = x.StudentId,
                            CreatedDate = x.CreatedDate,
                            Gender = x.Gender,
                            Level = x.Level,
                            NickName = x.NickName,
                            StudentGameAvatars = x.StudentGameAvatars.Where(x => x.StudentGameInfoId == studentId).Select(x => new StudentGameAvatarModel
                            {
                                AvatarImageId = x.AvatarImageId,
                                StudentGameInfoId = x.StudentGameInfoId,
                                CreatedDate = x.CreatedDate,
                                IsActive = x.IsActive,
                                Id = x.Id,
                            }).ToList(),
                        }).FirstOrDefaultAsync(cancellationToken);

            methodResult.Result = studentGameInfo;
            return methodResult;
        }
    }
}
