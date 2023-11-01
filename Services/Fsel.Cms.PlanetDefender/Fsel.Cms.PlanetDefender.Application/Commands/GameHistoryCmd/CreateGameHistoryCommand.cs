// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Commands.GameHistoryCmd
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fsel.Cms.PlanetDefender.Domain.Models.CommandModel.GameHistorys;
    using Fsel.Cms.PlanetDefender.Domain.Models.EntityModels;
    using Fsel.Common.ActionResults;
    using MediatR;

    public class CreateGameHistoryCommand : CreateGameHistoryCommandModel, IRequest<MethodResult<GameHistoryModel>>
    {
    }
}
