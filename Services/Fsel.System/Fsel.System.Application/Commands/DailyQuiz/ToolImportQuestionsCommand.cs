namespace Fsel.System.Application.Commands.DailyQuiz
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Common.Models.Excels;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Constants;
    using Fsel.System.Domain.Models.CommandModels.DailyQuiz;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class ToolImportQuestionsCommand : BaseImportCommandModel, IRequest<MethodResult<Stream>>
    {
    }

    public class ToolImportQuestionsCommandHandler : IRequestHandler<ToolImportQuestionsCommand, MethodResult<Stream>>
    {
        private readonly IMediator _mediator;

        public ToolImportQuestionsCommandHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<MethodResult<Stream>> Handle(ToolImportQuestionsCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<Stream>();

            if (request.FormFile == null || request.FormFile.Length == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.ImportFileRequired));
                return methodResult;
            }

            var result = request.FormFile.ImportAndValidateExcel(async (ImportQuestionModel x, IList<ImportQuestionModel> models, int rowIndex, IList<ValidateExcelModel> errors) =>
            {
                if (string.IsNullOrEmpty(x.Category))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Category), Message = "Chưa nhập" });
                }
                if (string.IsNullOrEmpty(x.Subcategory))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Subcategory), Message = "Chưa nhập" });
                }
                if (string.IsNullOrEmpty(x.QuestionEnglish))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.QuestionEnglish), Message = "Chưa nhập" });
                }
                if (string.IsNullOrEmpty(x.QuestionVietnamese))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.QuestionVietnamese), Message = "Chưa nhập" });
                }
                if (string.IsNullOrEmpty(x.OptionA))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.OptionA), Message = "Chưa nhập" });
                }
                else if (x.AnswerA == null || string.IsNullOrEmpty(x.AnswerA.vi) || string.IsNullOrEmpty(x.AnswerA.en))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.OptionA), Message = "Sai định dạng" });
                }
                if (string.IsNullOrEmpty(x.OptionB))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.OptionB), Message = "Chưa nhập" });
                }
                else if (x.AnswerB == null || string.IsNullOrEmpty(x.AnswerB.vi) || string.IsNullOrEmpty(x.AnswerB.en))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.OptionB), Message = "Sai định dạng" });
                }
                if (string.IsNullOrEmpty(x.OptionC))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.OptionC), Message = "Chưa nhập" });
                }
                else if (x.AnswerC == null || string.IsNullOrEmpty(x.AnswerC.vi) || string.IsNullOrEmpty(x.AnswerC.en))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.OptionC), Message = "Sai định dạng" });
                }
                if (string.IsNullOrEmpty(x.CorrectAnswer))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.CorrectAnswer), Message = "Chưa nhập" });
                }
                else if (x.CorrectAnswer != "A" && x.CorrectAnswer != "B" && x.CorrectAnswer != "C")
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.CorrectAnswer), Message = "Sai định dạng" });
                }
                if (string.IsNullOrEmpty(x.Explanation))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Explanation), Message = "Chưa nhập" });
                }
                else if (x.ExplanationModel == null || string.IsNullOrEmpty(x.ExplanationModel.vi) || string.IsNullOrEmpty(x.ExplanationModel.en))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Explanation), Message = "Sai định dạng" });
                }

                return await Task.FromResult(errors.Count == 0);
            },
                null,
                null,
                null,
                false);

            if (result.Stream != null)
            {
                methodResult.Result = result.Stream;
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            var datas = result.Datas.ToList();
            if (datas == null || datas.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.ImportFileRequired));
                return methodResult;
            }

            var questionModels = new List<CreateDailyQuizQuestionCommandModel>();
            datas.ForEach(p =>
            {
                var questionModel = new CreateDailyQuizQuestionCommandModel()
                {
                    Category = p.Category,
                    SubCategory = p.Subcategory,
                    Content = p.QuestionVietnamese,
                    Explanation = p.ExplanationModel?.vi,
                    DailyQuizAnswers = new List<CreateDailyQuizAnswerCommandModel>()
                    {
                        new CreateDailyQuizAnswerCommandModel()
                        {
                            Content = p.AnswerA?.vi,
                            IsCorrect = p.CorrectAnswer == "A",
                            Translations = new List<CreateDailyQuizTranslationCommandModel>()
                            {
                                new CreateDailyQuizTranslationCommandModel()
                                {
                                    Content = p.AnswerA?.vi,
                                    Language = "vi-VN",
                                },
                                new CreateDailyQuizTranslationCommandModel()
                                {
                                    Content = p.AnswerA ?.en,
                                    Language = "en-US",
                                }
                            }
                        },
                        new CreateDailyQuizAnswerCommandModel()
                        {
                            Content = p.AnswerB?.vi,
                            IsCorrect = p.CorrectAnswer == "B",
                            Translations = new List<CreateDailyQuizTranslationCommandModel>()
                            {
                                new CreateDailyQuizTranslationCommandModel()
                                {
                                    Content = p.AnswerB?.vi,
                                    Language = "vi-VN",
                                },
                                new CreateDailyQuizTranslationCommandModel()
                                {
                                    Content = p.AnswerB ?.en,
                                    Language = "en-US",
                                }
                            }
                        },
                        new CreateDailyQuizAnswerCommandModel()
                        {
                            Content = p.AnswerC?.vi,
                            IsCorrect = p.CorrectAnswer == "C",
                            Translations = new List<CreateDailyQuizTranslationCommandModel>()
                            {
                                new CreateDailyQuizTranslationCommandModel()
                                {
                                    Content = p.AnswerC?.vi,
                                    Language = "vi-VN",
                                },
                                new CreateDailyQuizTranslationCommandModel()
                                {
                                    Content = p.AnswerC ?.en,
                                    Language = "en-US",
                                }
                            }
                        }
                    },
                    Translations = new List<CreateDailyQuizTranslationCommandModel>()
                    {
                        new CreateDailyQuizTranslationCommandModel()
                        {
                            Content = p.QuestionVietnamese,
                            Language = "vi-VN",
                            Explanation = p.ExplanationModel?.vi
                        },
                        new CreateDailyQuizTranslationCommandModel()
                        {
                            Content = p.QuestionEnglish,
                            Language = "en-US",
                            Explanation = p.ExplanationModel?.en
                        }
                    }
                };
                questionModels.Add(questionModel);
            });

            var createQuestionResult = await _mediator.Send(new CreateDailyQuizQuestionsCommand() { DailyQuizQuestions = questionModels }, cancellationToken);

            return methodResult;
        }
    }
}
