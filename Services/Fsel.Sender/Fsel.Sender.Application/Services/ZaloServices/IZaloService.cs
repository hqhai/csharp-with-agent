namespace Fsel.Sender.Application.Services.ZaloServices
{
    using Fsel.Sender.Application.Services.ZaloServices.Models;

    public interface IZaloService
    {
        Task<SendSMSByZaloResponseModel?> SendSMSAsync(SendSMSByZaloRequestModel request, string token);
    }
}
