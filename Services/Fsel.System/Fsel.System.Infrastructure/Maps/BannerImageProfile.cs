namespace Fsel.System.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.Models.CommandModels.BannerImages;

    public class BannerImageProfile : Profile
    {
        public BannerImageProfile()
        {
            CreateMap<BannerImage, BannerImageModel>().IgnoreAllNonExisting();
            CreateMap<CreateBannerImageCommandModel, BannerImage>().IgnoreAllNonExisting();
            CreateMap<BannerImage, CreateBannerImageCommandModel>().IgnoreAllNonExisting();
            CreateMap<BannerImage, BannerStudentQueueModel>().IgnoreAllNonExisting();
        }
    }
}
