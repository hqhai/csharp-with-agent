// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.V1i1
{
    public class VideoTimeCodeContext
    {
        public VideoResult VideoResult { get; set; } = new VideoResult();
        public VideoTimeCode VideoTimeCode { get; set; } = new VideoTimeCode();
        public VideoTimeCodeResult VideoTimeCodeResult { get; set; } = new VideoTimeCodeResult();
        public LessonResult LessonResult { get; set; } = new LessonResult();
        public Course Course { get; set; } = new Course();
        public CourseResult CourseResult { get; set; } = new CourseResult();
    }
}
