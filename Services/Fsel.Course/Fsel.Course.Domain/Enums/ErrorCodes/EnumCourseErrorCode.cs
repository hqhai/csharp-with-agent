// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Enums.ErrorCodes
{
    public enum EnumCourseErrorCode
    {
        /// <summary>
        /// Course is not in a new state
        /// </summary>
        CourseNotInNewState,

        /// <summary>
        /// Course Not Type IElts
        /// </summary>
        CourseNotTypeIElts,

        /// <summary>
        /// Course Not Type Academic
        /// </summary>
        CourseNotTypeAcademic,

        /// <summary>
        /// Student not in class
        /// </summary>
        StudentNotInClass,

        /// <summary>
        /// MocktestId and UnitId cannot have values at the same time.
        /// </summary>
        MocktestIdAndUnitIdAreMutuallyExclusive,

        /// <summary>
        /// Course must be in an Active state
        /// </summary>
        CourseMustActiveState,

        /// <summary>
        /// Course is in Active state
        /// </summary>
        CourseIsActiveState,

        /// <summary>
        /// Course is in New state, can't start Lesson
        /// </summary>
        CourseIsNewStateCantStartLesson,

        /// <summary>
        /// another level unit exists
        /// </summary>
        AnotherLevelUnitExists,

        /// <summary>
        /// Duplicate Unit Id
        /// </summary>
        DuplicateUnitId,

        /// <summary>
        /// MockTest is up to two
        /// </summary>
        MockTestIsUpToTwo,

        /// <summary>
        /// FinalTest is up to One
        /// </summary>
        FinalTestIsUpToOne,

        /// <summary>
        /// UnitTest is up to Eight
        /// </summary>
        UnitTestIsUpToEight,

        /// <summary>
        /// FinalTestId must be at the end
        /// </summary>
        FinalTestIdMustBeAtTheEnd,

        /// <summary>
        /// requires 13 courseUnitMockTests
        /// </summary>
        Requires13CourseUnitMockTests
    }
}
