// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.ModuleModels
{
    using System.Collections.ObjectModel;
    using Fsel.Course.Domain.Enums;

    public class ModuleLessonModel : BaseModuleModel
    {
        public EnumLessonConfigType LessonConfigType { get; set; }
        public Guid LessonId { get; set; }
        public List<ModuleLessonModel>? SubModules { get; set; }
    }
}
