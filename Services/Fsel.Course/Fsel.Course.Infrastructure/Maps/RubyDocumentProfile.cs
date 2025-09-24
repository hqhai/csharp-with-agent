// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Models.CommandModels.RubyScope;
    using Fsel.Course.Domain.Models.EntityModels;
    using EntityRubyScope = Fsel.Course.Domain.Entities.RubyScope;

    public class RubyDocumentProfile : Profile
    {
        public RubyDocumentProfile()
        {
            CreateMap<EntityRubyScope, RubyScopeModel>().IgnoreAllNonExisting();
            CreateMap<EntityRubyScope, EntityRubyScope>()
                .ForMember(m => m.Text, opt => opt.Ignore())
                .ForMember(m => m.RowVersion, opt => opt.Ignore())
                .IgnoreAllNonExisting();
            CreateMap<CreateRubyScopeCommandModel, EntityRubyScope>().IgnoreAllNonExisting();
            CreateMap<UpdateRubyScopeCommandModel, EntityRubyScope>().IgnoreAllNonExisting();
        }
    }
}
