// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.FFmpegServices
{
    using System.Threading.Tasks;
    using Fsel.Course.Lms.Application.Services.FFmpegServices.Models;
    using Refit;

    public interface IFFmpegServices
    {
        [Multipart]
        [Post("/get-info-audio")]
        Task<IApiResponse<InfoAudioModel>> GetInfoAudio([AliasAs("file")] StreamPart file);
    }
}
