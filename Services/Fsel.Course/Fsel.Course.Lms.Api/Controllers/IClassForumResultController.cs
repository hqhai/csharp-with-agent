// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Controllers
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Lms.Application.Commands.ClassForumCmd;
    using Fsel.Course.Lms.Application.Commands.ClassForumResultCmd;
    using Microsoft.AspNetCore.Mvc;

    public interface IClassForumResultController
    {
        Task<IActionResult> Create([FromBody] CreateClassForumResultCommand command);
        Task<IActionResult> ExecuteList([FromBody] BaseQueryModel query);
        Task<IActionResult> Get([FromRoute] Guid id);
        Task<IActionResult> Rate([FromBody] RateClassForumResultCommand command);
    }
}