// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Storage.Application.Services.FFmpegServices
{
    using System.Threading.Tasks;
    using Fsel.Storage.Application.Services.FFmpegServices.Models;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    public interface IFFmpegServices
    {
        [Multipart]
        [Post("/get-info-audio")]
        Task<IApiResponse<InfoAudioModel>> GetInfoAudio([AliasAs("file")] StreamPart file);

        [Multipart]
        [Post("/convert")]
        Task<IApiResponse<ConvertModel>> Convert([AliasAs("file")] StreamPart file);

        [Get("/status/{jobId}")]
        Task<IApiResponse<StatusJobModel>> Status([FromRoute] Guid jobId);
    }
}
