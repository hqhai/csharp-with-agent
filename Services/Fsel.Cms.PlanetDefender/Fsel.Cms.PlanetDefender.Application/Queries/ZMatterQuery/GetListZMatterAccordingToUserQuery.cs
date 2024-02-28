// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Queries.ZMatterQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Cms.PlanetDefender.Domain.IRepositories;
    using Fsel.Cms.PlanetDefender.Domain.Models.EntityModels;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using MediatR;
    using Fsel.Cms.PlanetDefender.Application.Services.UserServices;
    using Fsel.Common.Enums.ErrorCodes;
    using Microsoft.EntityFrameworkCore;

    public class GetListZMatterAccordingToUserQuery : IRequest<MethodResult<IList<ZMatterModel>>>
    {
    }
    public class GetListZMatterAccordingToUserQueryHandler : IRequestHandler<GetListZMatterAccordingToUserQuery, MethodResult<IList<ZMatterModel>>>
    {
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IZMatterRepository _zMatterRepository;
        private readonly IMapper _mapper;
        private readonly IGameAnswerRepository _gameAnswerRepository;

        public GetListZMatterAccordingToUserQueryHandler(AuthContext authContext, IUserService userService, IZMatterRepository zMatterRepository, IMapper mapper, IGameAnswerRepository gameAnswerRepository)
        {
            _authContext = authContext;
            _userService = userService;
            _zMatterRepository = zMatterRepository;
            _mapper = mapper;
            _gameAnswerRepository = gameAnswerRepository;
        }

        public async Task<MethodResult<IList<ZMatterModel>>> Handle(GetListZMatterAccordingToUserQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<ZMatterModel>>();

            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddError(studentResult.Error);
                return methodResult;
            }
            var student = studentResult.Content?.Result;

            var gameAnswers = await _gameAnswerRepository.Queryable.Where(p => p.StudentId == student!.Id).ToListAsync(cancellationToken);

            var zMatterIds = gameAnswers.Select(p => p.ZMatterId).ToList();

            var zMatter = await _zMatterRepository.Queryable.Select(p => _mapper.Map<ZMatterModel>(p)).ToListAsync(cancellationToken);

            zMatter.ForEach(x => { x.IsOwned = zMatterIds.Any(p => p == x.Id); });

            methodResult.Result = zMatter;
            return methodResult;
        }
    }
}
