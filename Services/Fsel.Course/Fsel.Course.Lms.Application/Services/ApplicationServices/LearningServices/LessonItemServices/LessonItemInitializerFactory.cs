// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.LearningServices.LessonItemServices
{
    using Fsel.Course.Domain.Enums;

    public interface ILessonItemInitializerFactory
    {
        ILessonItemInitializer? Get(EnumLessonConfigType type);
    }

    public class LessonItemInitializerFactory : ILessonItemInitializerFactory
    {
        private readonly IDictionary<EnumLessonConfigType, ILessonItemInitializer> _initializers;

        public LessonItemInitializerFactory(
            VideoLessonItemInitializer videoInitializer,
            ClassForumLessonItemInitializer forumInitializer,
            HomeWorkLessonItemInitializer homeworkInitializer,
            DocumentLessonItemInitializer documentInitializer)
        {
            _initializers = new Dictionary<EnumLessonConfigType, ILessonItemInitializer>
        {
            { EnumLessonConfigType.Video, videoInitializer },
            { EnumLessonConfigType.ClassForum, forumInitializer },
            { EnumLessonConfigType.HomeWork, homeworkInitializer },
            { EnumLessonConfigType.Document, documentInitializer }
        };
        }

        public ILessonItemInitializer? Get(EnumLessonConfigType type)
        {
            return _initializers.TryGetValue(type, out var initializer)
                ? initializer
                : null;
        }
    }
}
