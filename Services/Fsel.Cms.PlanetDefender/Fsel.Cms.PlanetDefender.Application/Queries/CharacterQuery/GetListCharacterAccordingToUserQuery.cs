// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Queries.CharacterQuery
{
    using System.Collections.Generic;
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

    public class GetListCharacterAccordingToUserQuery : IRequest<MethodResult<IList<SpaceShipModel>>>
    {
    }
    public class GetListCharacterAccordingToUserQueryHandler : IRequestHandler<GetListCharacterAccordingToUserQuery, MethodResult<IList<SpaceShipModel>>>
    {
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IStudentGameInfoRepository _studentGameInfoRepository;
        private readonly IMapper _mapper;
        private readonly ICharacterRepository _characterRepository;
        public GetListCharacterAccordingToUserQueryHandler(AuthContext authContext, IUserService userService, IStudentGameInfoRepository studentGameInfoRepository, IMapper mapper, ICharacterRepository characterRepository)
        {
            _authContext = authContext;
            _userService = userService;
            _studentGameInfoRepository = studentGameInfoRepository;
            _mapper = mapper;
            _characterRepository = characterRepository;
        }

        public async Task<MethodResult<IList<SpaceShipModel>>> Handle(GetListCharacterAccordingToUserQuery request, CancellationToken cancellationToken)
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

            var studentGameInfo = await _studentGameInfoRepository.Queryable.Include(x => x.StudentCharacters).FirstOrDefaultAsync(p => p.StudentId == student!.Id, cancellationToken);

            if (studentGameInfo == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var characters = await _characterRepository.Queryable.Select(p => _mapper.Map<SpaceShipModel>(p)).ToListAsync(cancellationToken);

            characters.ForEach(x => { x.IsOwned = studentGameInfo.StudentCharacters.Any(p => p.CharacterId == x.Id); });

            methodResult.Result = characters;
            return methodResult;
        }
    }
}
