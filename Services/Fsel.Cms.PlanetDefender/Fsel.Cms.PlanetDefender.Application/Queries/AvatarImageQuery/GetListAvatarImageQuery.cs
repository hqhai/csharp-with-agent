// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Queries.AvatarImageQuery
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Cms.PlanetDefender.Domain.IRepositories;
    using Fsel.Cms.PlanetDefender.Domain.Models.EntityModels;
    using Fsel.Common.ActionResults;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetListAvatarImageQuery : IRequest<MethodResult<IList<AvatarImageModel>>>
    {
    }

    public class GetListAvatarImageQueryHandler : IRequestHandler<GetListAvatarImageQuery, MethodResult<IList<AvatarImageModel>>>
    {
        private readonly IAvatarImageRepository _avatarImageRepository;

        public GetListAvatarImageQueryHandler(IAvatarImageRepository avatarImageRepository)
        {
            _avatarImageRepository = avatarImageRepository;
        }

        public async Task<MethodResult<IList<AvatarImageModel>>> Handle(GetListAvatarImageQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<AvatarImageModel>>();

            var avatarImage = await _avatarImageRepository.Queryable.Select(x => new AvatarImageModel
            {
                Id = x.Id,
                CreatedDate = x.CreatedDate,
                CreatedFullName = x.CreatedFullName,
                FilePath = x.FilePath,
            }).ToListAsync(cancellationToken);

            methodResult.Result = avatarImage;
            return methodResult;
        }
    }
}
