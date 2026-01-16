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
        MockTestCannotBeDreaterThan2,

        /// <summary>
        /// FinalTest is up to One
        /// </summary>
        FinalTestCannotBeDreaterThan1,

        /// <summary>
        /// FinalTestId must be at the end
        /// </summary>
        FinalTestIdMustBeAtTheEnd,

        /// <summary>
        /// requires 13 courseUnitMockTests
        /// </summary>
        Requires13CourseUnitMockTests,

        /// <summary>
        /// Unit Results Exist
        /// </summary>
        UnitResultsExist,

        /// <summary>
        ///CourseUnitMockTests Is Ten
        /// </summary>
        TheNumberOfItemsCannotBeDifferentFrom10,

        /// <summary>
        /// MockTes tMust Be I nPositions 5 And 10
        /// </summary>
        MockTestMustBeInPositions5And10,

        /// <summary>
        ///  MocktestResults Exist
        /// </summary>
        MockTestResultsExist,

        /// <summary>
        ///  FinalTest HasBeen Used
        /// </summary>
        FinalTestHasBeenUsed,

        /// <summary>
        ///  Unit Has Been Used
        /// </summary>
        UnitHasBeenUsed,

        /// <summary>
        /// Invalid Unit Quantity
        /// </summary>
        InvalidUnitQuantity,

        /// <summary>
        /// CourseRequest Not Active
        /// </summary>
        CourseRequestNotActive,

        /// <summary>
        /// Course Not InActive
        /// </summary>
        CourseNotInActive,

        /// <summary>
        /// Course Module Not Null
        /// </summary>
        CourseModulesNotNull,

        /// <summary>
        /// Course is used
        /// </summary>
        CourseIsUsed
    }
}
