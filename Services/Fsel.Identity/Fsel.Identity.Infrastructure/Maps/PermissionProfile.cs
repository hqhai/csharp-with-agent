using System.Security.Claims;
using AutoMapper;
using Fsel.Core.Extensions;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.Models.CommandModels.Permissions;
using Fsel.Identity.Domain.Models.EntityModels.Permissions;

namespace Fsel.Identity.Infrastructure.Maps
{
    public class PermissionProfile : Profile
    {
        public PermissionProfile()
        {
            CreateMap<SavePermissionGroupCommandModel, PermissionGroup>().IgnoreAllNonExisting();
            CreateMap<SavePermissionCommandModel, Permission>().IgnoreAllNonExisting();
            CreateMap<PermissionGroup, PermissionGroupModel>().IgnoreAllNonExisting();
            CreateMap<Permission, PermissionModel>().IgnoreAllNonExisting();
            CreateMap<RoleClaim, Claim>().ConstructUsing(x => new Claim(x.ClaimType!, x.ClaimValue!));
        }
    }
}
