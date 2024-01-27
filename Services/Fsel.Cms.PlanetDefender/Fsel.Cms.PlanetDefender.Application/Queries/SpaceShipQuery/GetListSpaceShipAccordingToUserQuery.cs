// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Queries.SpaceShipQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Cms.PlanetDefender.Application.Services.UserServices;
    using Fsel.Cms.PlanetDefender.Domain.IRepositories;
    using Fsel.Cms.PlanetDefender.Domain.Models.EntityModels;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetListSpaceShipAccordingToUserQuery : IRequest<MethodResult<IList<SpaceShipModel>>>
    {
    }
    public class GetListSpaceShipAccordingToUserQueryHandler : IRequestHandler<GetListSpaceShipAccordingToUserQuery, MethodResult<IList<SpaceShipModel>>>
    {
        private readonly AuthContext _authContext;
        private readonly ISpaceShipRepository _spaceShipRepository;
        private readonly IUserService _userService;
        private readonly IStudentGameInfoRepository _studentGameInfoRepository;
        private readonly IMapper _mapper;
        public GetListSpaceShipAccordingToUserQueryHandler(AuthContext authContext, ISpaceShipRepository spaceShipRepository, IUserService userService, IStudentGameInfoRepository studentGameInfoRepository, IMapper mapper)
        {
            _authContext = authContext;
            _spaceShipRepository = spaceShipRepository;
            _userService = userService;
            _studentGameInfoRepository = studentGameInfoRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<SpaceShipModel>>> Handle(GetListSpaceShipAccordingToUserQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<SpaceShipModel>>();

            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddError(studentResult.Error);
                return methodResult;
            }
            var student = studentResult.Content?.Result;

            var studentGameInfo = await _studentGameInfoRepository.Queryable.Include(x => x.StudentSpaceShips).FirstOrDefaultAsync(p => p.StudentId == student!.Id, cancellationToken);

            if (studentGameInfo == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var spaceShips = await _spaceShipRepository.Queryable.Select(p => _mapper.Map<SpaceShipModel>(p)).ToListAsync(cancellationToken);

            spaceShips.ForEach(x => { x.IsOwned = studentGameInfo.StudentSpaceShips.Any(p => p.SpaceShipId == x.Id); });

            methodResult.Result = spaceShips;
            return methodResult;
        }
    }
}
