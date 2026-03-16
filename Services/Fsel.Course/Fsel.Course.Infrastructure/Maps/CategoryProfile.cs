// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Models.CommandModels.Categories;
    using Fsel.Course.Domain.Models.CommandModels.Programs;
    using Fsel.Course.Domain.Models.EntityModels;

    public class CategoryProfile : Profile
    {
        public CategoryProfile()
        {
            CreateMap<Category, CategoryModel>().IgnoreAllNonExisting();
            CreateMap<CreateCategoryCommandModel, Category>().IgnoreAllNonExisting();
            CreateMap<UpdateCategoryCommandModel, Category>().IgnoreAllNonExisting();
            CreateMap<Category, CategoryTreeModel>()
                     .ForMember(x => x.Code, x => x.MapFrom(c => c.Code))
                     .ForMember(x => x.Label, x => x.MapFrom(c => c.Name))
                     .ForMember(x => x.ExpandedIcon, x => x.MapFrom(c => c.Type))
                     .ForMember(x => x.Data, x => x.MapFrom(c => c.Id))
                     .ForMember(x => x.Children, x => x.MapFrom(c => c.Categorys));

            CreateMap<Category, ProgramModel>().IgnoreAllNonExisting();
            CreateMap<CreateProgramCommandModel, Category>().ForMember(x => x.Levels, x => x.Ignore()).IgnoreAllNonExisting();
            CreateMap<UpdateProgramCommandModel, Category>().ForMember(x => x.Levels, x => x.Ignore()).IgnoreAllNonExisting();
            CreateMap<Level, LevelModel>()
                .ForMember(x => x.ProgramLevelName, x => x.MapFrom(c => c.Category != null ? $"{c.Category.Name} - {c.Name}" : c.Name))
                .ForMember(x => x.Skils, x => x.MapFrom(c => c.SkillLevels.OrderBy(x => x.CreatedDate).Select(x => x.Skill)));
            CreateMap<Level, SelectionLevelModel>().ForMember(x => x.ProgramLevelName, x => x.MapFrom(c => c.Category != null ? $"{c.Category.Name} - {c.Name}" : c.Name));
            CreateMap<UpdateLevelCommandModel, Level>().IgnoreAllNonExisting();
        }
    }
}
