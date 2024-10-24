// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.TestCmd
{
    using System;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Common.Models.Excels;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.Tests;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class ImportUpdateModuleProgressCommand : BaseImportCommandModel, IRequest<MethodResult<Stream>>
    {
    }

    public class ImportUpdateModuleProgressCommandHandler : IRequestHandler<ImportUpdateModuleProgressCommand, MethodResult<Stream>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly IVideoTimeCodeRepository _videoTimeCodeRepository;
        private readonly IFinalTestRepository _finalTestRepository;
        private readonly IMockTestRepository _mockTestRepository;
        private readonly IMediator _mediator;

        public ImportUpdateModuleProgressCommandHandler(ICourseRepository courseRepository,
            IUnitRepository unitRepository,
            ILessonRepository lessonRepository,
            IVideoTimeCodeRepository videoTimeCodeRepository,
            IFinalTestRepository finalTestRepository,
            IMockTestRepository mockTestRepository,
            IMediator mediator)
        {
            _courseRepository = courseRepository;
            _unitRepository = unitRepository;
            _lessonRepository = lessonRepository;
            _videoTimeCodeRepository = videoTimeCodeRepository;
            _finalTestRepository = finalTestRepository;
            _mockTestRepository = mockTestRepository;
            _mediator = mediator;
        }

        public async Task<MethodResult<Stream>> Handle(ImportUpdateModuleProgressCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<Stream>();
            if (request.FormFile == null)
            {
                return methodResult;
            }
            var result = request.FormFile.ImportAndValidateExcel(async (UpdateStudentModuleProgressCommandModel x, IList<UpdateStudentModuleProgressCommandModel> models, int rowIndex, IList<ValidateExcelModel> errors) =>
            {
                ValidateField(nameof(x.Email), x.Email, errors, rowIndex, isValidEmail: true);
                ValidateField(nameof(x.Type), x.Type, errors, rowIndex);
                switch (x.Type)
                {
                    case nameof(Lesson):
                    case nameof(ClassForum):
                        ValidateField(nameof(x.UnitId), x.UnitId, errors, rowIndex);
                        ValidateField(nameof(x.LessonId), x.LessonId, errors, rowIndex);
                        break;

                    case nameof(VideoTimeCode):
                        ValidateField(nameof(x.UnitId), x.UnitId, errors, rowIndex);
                        ValidateField(nameof(x.LessonId), x.LessonId, errors, rowIndex);
                        ValidateField(nameof(x.ObjectId), x.ObjectId, errors, rowIndex);
                        break;

                    case nameof(FinalTest):
                    case nameof(EnumMockTestType.FullMockTest):
                        ValidateField(nameof(x.ObjectId), x.ObjectId, errors, rowIndex);
                        break;

                    case nameof(EnumMockTestType.SkillMockTest):
                        ValidateField(nameof(x.UnitId), x.UnitId, errors, rowIndex);
                        ValidateField(nameof(x.ObjectId), x.ObjectId, errors, rowIndex);
                        break;

                    default:
                        break;
                }

                ValidateField(nameof(x.CourseId), x.CourseId, errors, rowIndex, isGuid: true);
                ValidateField(nameof(x.UnitId), x.UnitId, errors, rowIndex, isGuid: true, isRequired: false);
                ValidateField(nameof(x.LessonId), x.LessonId, errors, rowIndex, isGuid: true, isRequired: false);
                ValidateField(nameof(x.ObjectId), x.ObjectId, errors, rowIndex, isGuid: true, isRequired: false);

                if (!string.IsNullOrEmpty(x.CourseId) && Guid.TryParse(x.CourseId.Trim(), out Guid courseId) && !await _courseRepository.AnyGuidAsync(courseId))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.CourseId), Message = $"{nameof(x.CourseId)} is {EnumSystemErrorCode.DataNotExist} Data" });
                }

                if (!string.IsNullOrEmpty(x.UnitId) && Guid.TryParse(x.UnitId.Trim(), out Guid unitId) && !await _unitRepository.AnyGuidAsync(unitId))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.UnitId), Message = $"{nameof(x.UnitId)} is {EnumSystemErrorCode.DataNotExist} Data" });
                }

                if (!string.IsNullOrEmpty(x.LessonId) && Guid.TryParse(x.LessonId.Trim(), out Guid lessonId) && !await _lessonRepository.AnyGuidAsync(lessonId))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.LessonId), Message = $"{nameof(x.LessonId)} is {EnumSystemErrorCode.DataNotExist} Data" });
                }

                if (!string.IsNullOrEmpty(x.ObjectId) && Guid.TryParse(x.ObjectId.Trim(), out Guid objectId))
                {
                    switch (x.Type)
                    {
                        case nameof(VideoTimeCode):
                            if (!await _videoTimeCodeRepository.AnyGuidAsync(objectId))
                            {
                                errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.ObjectId), Message = $"{nameof(x.ObjectId)} is {EnumSystemErrorCode.DataNotExist} Data" });
                            }
                            break;

                        case nameof(FinalTest):
                            if (!await _finalTestRepository.AnyGuidAsync(objectId))
                            {
                                errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.ObjectId), Message = $"{nameof(x.ObjectId)} is {EnumSystemErrorCode.DataNotExist} Data" });
                            }
                            break;

                        case nameof(EnumMockTestType.SkillMockTest):
                        case nameof(EnumMockTestType.FullMockTest):
                            if (!await _mockTestRepository.AnyGuidAsync(objectId))
                            {
                                errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.ObjectId), Message = $"{nameof(x.ObjectId)} is {EnumSystemErrorCode.DataNotExist} Data" });
                            }
                            break;

                        default:
                            break;
                    }
                }
                return await Task.FromResult(errors.Count == 0);
            });

            if (result.Stream != null)
            {
                methodResult.Result = result.Stream;
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            foreach (var item in result.Datas)
            {
                if (!Guid.TryParse(item.CourseId?.Trim(), out Guid courseId))
                {
                    continue;
                }
                var method = await _mediator.Send(new UpdateModuleProcessCommand
                {
                    Email = item.Email,
                    CourseId = courseId,
                    UnitId = TryParseGuid(item.UnitId),
                    LessonId = TryParseGuid(item.LessonId),
                    Type = item.Type,
                    ObjectId = TryParseGuid(item.ObjectId)
                }, cancellationToken);
            }
            return methodResult;
        }

        private static Guid? TryParseGuid(string? objectId)
        {
            if (!string.IsNullOrEmpty(objectId) && Guid.TryParse(objectId.Trim(), out Guid result))
            {
                return result;
            }
            return null;
        }

        private static void ValidateField(string? fieldName, string? fieldValue, IList<ValidateExcelModel> errors, int rowIndex, bool isGuid = false, bool isValidEmail = false, bool isRequired = true)
        {
            if (isRequired && string.IsNullOrEmpty(fieldValue))
            {
                errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = fieldName, Message = $"{fieldName} is {EnumSystemErrorCode.DataNotExist}" });
            }
            else if (!string.IsNullOrEmpty(fieldValue))
            {
                if (isValidEmail && !fieldValue.Trim().IsValidEmail())
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = fieldName, Message = $"{fieldName} is {EnumSystemErrorCode.InValidFormat}" });
                }
                else if (isGuid && !Guid.TryParse(fieldValue.Trim(), out _))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = fieldName, Message = $"{fieldName} is {EnumSystemErrorCode.InValidFormat}" });
                }
            }
        }
    }
}
