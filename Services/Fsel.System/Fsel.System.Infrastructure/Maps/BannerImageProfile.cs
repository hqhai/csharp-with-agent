namespace Fsel.System.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.Models.CommandModels.BannerImages;
    using Fsel.System.Domain.Models.EntityModels;

    public class BannerImageProfile : Profile
    {
        public BannerImageProfile()
        {
            CreateMap<BannerImage, BannerImageModel>().IgnoreAllNonExisting();
            CreateMap<CreateBannerImageCommandModel, BannerImage>().IgnoreAllNonExisting();
        }
    }
}
