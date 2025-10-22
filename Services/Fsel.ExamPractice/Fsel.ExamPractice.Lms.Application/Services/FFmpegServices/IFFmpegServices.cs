// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Lms.Application.Services.FFmpegServices
{
    using System.Threading.Tasks;
    using Fsel.ExamPractice.Lms.Application.Services.FFmpegServices.Models;
    using Refit;

    public interface IFFmpegServices
    {
        [Multipart]
        [Post("/get-info-audio")]
        Task<IApiResponse<InfoAudioModel>> GetInfoAudio([AliasAs("file")] StreamPart file);

        [Multipart]
        [Post("/convert")]
        Task<IApiResponse<ConvertModel>> Convert([AliasAs("file")] StreamPart file);
    }
}
