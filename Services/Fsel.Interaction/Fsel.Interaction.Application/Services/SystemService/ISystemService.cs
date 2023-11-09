// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Services.SystemService
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Interaction.Application.Services.SystemService.Models;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    public interface ISystemService
    {
        [Get("/forbidden-word/get-list-forbidden-word")]
        Task<IApiResponse<MethodResult<IList<String>>>> CheckContainForbiddenWord([FromQuery] String Word);

        [Get("/student/quest-board/get-list-by-category/{packageId}/{categoryBoards}")]
        Task<IApiResponse<MethodResult<IList<QuestBoardModel>>>> GetListQuestBoardQuery([FromRoute] Guid packageId, string categoryBoards);

        [Get("/student/quest-board/get-list-quest-board-student")]
        Task<IApiResponse<MethodResult<IList<QuestBoardStudentModel>>>> GetListQuestBoardStudent([FromRoute] GetListQuestBoardStudentModel query);

        [Post("/student/quest-board/taking-mission/{questBoardId}")]
        Task<IApiResponse<MethodResult<bool>>> CreateQuestBoardStudent([FromRoute] Guid questBoardId);

        [Put("/student/quest-board/update-achieved-points/{questBoardStudent}")]
        Task<IApiResponse<MethodResult<bool>>> UpdateQuestBoardStudentCommand([FromRoute] Guid questBoardStudent);
    }
}
