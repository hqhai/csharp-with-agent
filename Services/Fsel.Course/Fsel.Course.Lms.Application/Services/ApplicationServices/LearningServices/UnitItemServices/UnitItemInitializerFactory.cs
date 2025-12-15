// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.LearningServices.UnitItemServices
{
    using Fsel.Course.Domain.Enums;

    public interface IUnitItemInitializerFactory
    {
        IUnitItemInitializer? Get(EnumUnitConfigType type);
    }

    public class UnitItemInitializerFactory : IUnitItemInitializerFactory
    {
        private readonly IDictionary<EnumUnitConfigType, IUnitItemInitializer> _initializers;

        public UnitItemInitializerFactory(TestUnitItemInitializer testUnitItemInitializer,
            LessonUnitItemInitializer lessonUnitItemInitializer)
        {
            _initializers = new Dictionary<EnumUnitConfigType, IUnitItemInitializer>
            {
                { EnumUnitConfigType.Test, testUnitItemInitializer },
                { EnumUnitConfigType.Lesson, lessonUnitItemInitializer }
            };
        }

        public IUnitItemInitializer? Get(EnumUnitConfigType type)
        {
            return _initializers.TryGetValue(type, out var initializer)
              ? initializer
              : null;
        }
    }
}
