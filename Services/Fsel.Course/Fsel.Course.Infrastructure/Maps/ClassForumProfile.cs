using AutoMapper;
using Fsel.Core.Extensions;
using Fsel.Course.Common.Models.Commands.ClassForum;
using Fsel.Course.Common.Models.Entities;
using Fsel.Course.Domain.Entities;

namespace Fsel.Course.Infrastructure.Maps
{
    public class ClassForumProfile : Profile
    {
        public ClassForumProfile()
        {
            CreateMap<ClassForum, ClassForumModel>().IgnoreAllNonExisting();
            CreateMap<CreateClassForumCommandModel, ClassForum>().IgnoreAllNonExisting();
            CreateMap<UpdateClassForumCommandModel, ClassForum>().IgnoreAllNonExisting();
        }
    }
}