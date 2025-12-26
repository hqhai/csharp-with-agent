// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.LearningServices.CourseItemServices
{
    using Fsel.Course.Domain.Enums;

    public interface ICourseItemInitializerFactory
    {
        ICourseItemInitializer? Get(EnumCourseConfigType type);
    }

    public class CourseItemInitializerFactory : ICourseItemInitializerFactory
    {
        private readonly IDictionary<EnumCourseConfigType, ICourseItemInitializer> _initializers;

        public CourseItemInitializerFactory(TestCourseItemInitializer testCourseItemInitializer,
            UnitCourseItemInitializer unitCourseItemInitializer)
        {
            _initializers = new Dictionary<EnumCourseConfigType, ICourseItemInitializer>
            {
                { EnumCourseConfigType.Test, testCourseItemInitializer },
                { EnumCourseConfigType.Unit, unitCourseItemInitializer }
            };
        }

        public ICourseItemInitializer? Get(EnumCourseConfigType type)
        {
            return _initializers.TryGetValue(type, out var initializer)
              ? initializer
              : null;
        }
    }
}
