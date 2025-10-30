// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.ActionResults;
using Fsel.Common.Helpers;
using Fsel.Common.Models;
using Fsel.Course.Domain.Enums;
using Fsel.Shared.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Fsel.Course.Application.Queries.CategoryQuery.V1i1
{
    public class GetEnumQuery : IRequest<MethodResult<IList<EnumModel>>>
    {
        public EnumCourseSourceData? EnumCourseSourceData { get; set; }
    }

    public class GetEnumHandler : IRequestHandler<GetEnumQuery, MethodResult<IList<EnumModel>>>
    {
        public GetEnumHandler()
        {
        }

        public async Task<MethodResult<IList<EnumModel>>> Handle(GetEnumQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<EnumModel>> methodResult = new MethodResult<IList<EnumModel>>();

            switch (request.EnumCourseSourceData)
            {
                case EnumCourseSourceData.CourseType:
                    methodResult.Result = ConvertHelper.EnumToListModel<EnumCourseType>();
                    break;

                case EnumCourseSourceData.CourseLevel:
                    methodResult.Result = ConvertHelper.EnumToListModel<EnumCourseLevel>();
                    break;

                case EnumCourseSourceData.CourseSkill:
                    methodResult.Result = ConvertHelper.EnumToListModel<EnumCourseSkill>();
                    break;

                case EnumCourseSourceData.CourseStatus:
                    methodResult.Result = ConvertHelper.EnumToListModel<EnumCourseStatus>();
                    break;

                case EnumCourseSourceData.ExtraPracticeType:
                    methodResult.Result = ConvertHelper.EnumToListModel<EnumExtraPracticeType>();
                    break;

                case EnumCourseSourceData.QuestionType:
                    var data = ConvertHelper.EnumToList<EnumQuestionType>().Where(x => (int)x >= (int)EnumQuestionType.MultichoiceV1).ToList();
                    methodResult.Result = (from x in data
                                           select new EnumModel
                                           {
                                               Name = x.GetDescription(),
                                               Value = x.ToString()
                                           }).ToList();
                    break;

                case EnumCourseSourceData.TimeCodeType:
                    methodResult.Result = ConvertHelper.EnumToListModel<EnumTimeCodeType>();
                    break;

                case EnumCourseSourceData.PlacementTestLevel:
                    methodResult.Result = ConvertHelper.EnumToListModel<EnumPlacementTestLevel>();
                    break;

                case EnumCourseSourceData.FinalTestLevel:
                    methodResult.Result = ConvertHelper.EnumToListModel<EnumFinalTestLevel>();
                    break;

                case EnumCourseSourceData.CurrentStatus:
                    methodResult.Result = ConvertHelper.EnumToListModel<EnumAnswerStatus>();
                    break;

                case EnumCourseSourceData.WorkFlowType:
                    methodResult.Result = ConvertHelper.EnumToListModel<EnumWorkFlowType>();
                    break;

                case EnumCourseSourceData.ExtraPracticeProgress:
                    methodResult.Result = ConvertHelper.EnumToListModel<EnumExtraPracticeProgress>();
                    break;

                case EnumCourseSourceData.SortFilter:
                    methodResult.Result = ConvertHelper.EnumToListModel<EnumSortFilter>();
                    break;

                case EnumCourseSourceData.ReviewType:
                    methodResult.Result = ConvertHelper.EnumToListModel<EnumReviewType>();
                    break;

                case EnumCourseSourceData.TeacherRole:
                    methodResult.Result = ConvertHelper.EnumToListModel<EnumRoleTeacher>();
                    break;

                case EnumCourseSourceData.QuestBoardType:
                    methodResult.Result = ConvertHelper.EnumToListModel<EnumQuestBoardType>();
                    break;

                case EnumCourseSourceData.RepeatType:
                    methodResult.Result = ConvertHelper.EnumToListModel<EnumRepeatType>();
                    break;

                case EnumCourseSourceData.EnumFeature:
                    methodResult.Result = ConvertHelper.EnumToListModel<EnumFeature>();
                    break;

                case EnumCourseSourceData.MockTestScoreCriteria:
                    methodResult.Result = ConvertHelper.EnumToListModel<EnumMockTestScoreCriteria>();
                    break;

                case EnumCourseSourceData.SubtitleLanguage:
                    methodResult.Result = ConvertHelper.EnumToListModel<EnumSubtitleLanguage>();
                    break;
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
