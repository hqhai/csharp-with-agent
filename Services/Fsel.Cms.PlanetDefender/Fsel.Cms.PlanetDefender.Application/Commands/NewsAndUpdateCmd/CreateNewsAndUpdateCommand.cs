// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Commands.NewsAndUpdateCmd
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fsel.Cms.PlanetDefender.Domain.Models.CommandModel.NewsAndUpdates;
    using Fsel.Cms.PlanetDefender.Domain.Models.EntityModels;
    using Fsel.Common.ActionResults;
    using MediatR;

    public class CreateNewsAndUpdateCommand : CreateNewsAndUpdateCommandModel, IRequest<MethodResult<NewsAndUpdateModel>>
    {
    }
}
