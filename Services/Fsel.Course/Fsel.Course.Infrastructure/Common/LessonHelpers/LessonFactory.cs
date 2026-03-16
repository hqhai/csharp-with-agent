// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common.LessonHelpers
{
    using Fsel.Common.Enums;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.V1i1;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Models.CommandModels.LessonInstructions;
    using Fsel.Course.Domain.Models.CommandModels.Lessons.V1i1;
    using Fsel.Shared.Helpers;

    public class LessonFactory
    {
        private readonly UpdateLessonCommandModel _createRequest;

        public LessonFactory(UpdateLessonCommandModel createRequest)
        {
            _createRequest = createRequest;
        }

        public Lesson Build(int version = 0, Guid? originalId = null)
        {
            var lesson = new Lesson
            {
                Name = _createRequest.Name,
                Code = _createRequest.Code,
                InstructionContent = _createRequest.InstructionContent,
                VideoCount = _createRequest.LessonModules?.Count(m => m.LessonConfigType == EnumLessonConfigType.Video) ?? default,
                ClassForumCount = _createRequest.LessonModules?.Count(m => m.LessonConfigType == EnumLessonConfigType.ClassForum) ?? default,
                HomeWorkCount = _createRequest.LessonModules?.Count(m => m.LessonConfigType == EnumLessonConfigType.HomeWork) ?? default,
                DocumentCount = _createRequest.LessonModules?.Count(m => m.LessonConfigType == EnumLessonConfigType.Document) ?? default,
                VersionStatus = EnumVersionStatus.LastVersion,
                Version = version,
                LevelId = _createRequest.LevelId,
                ProgramId = _createRequest.ProgramId,
                Status = _createRequest.Status
            };

            lesson.OriginalId = originalId.HasValue ? originalId.Value : lesson.Id;

            lesson.LessonModules = LessonModuleClassification(_createRequest.LessonModules).ToList();
            lesson.LessonInstructions = LessonInstructionClassification(_createRequest.LessonInstructions).ToList();

            return lesson;
        }

        private static IEnumerable<LessonModule> LessonModuleClassification(IList<UpdateLessonModuleModel>? modules)
        {
            if (modules != null)
            {
                for (var i = 0; i < modules.Count; i++)
                {
                    var module = modules[i];

                    yield return new LessonModule
                    {
                        LessonConfigType = module.LessonConfigType,
                        Percent = module.Percent,
                        OpenOrder = module.OpenOrder,
                        OriginalId = module.OriginalId ?? Guid.Empty,
                        DisplayOrder = i + 1,
                        DisplayNumber = modules.IndexOfSubSet(module, m => m.LessonConfigType == module.LessonConfigType) + 1 ?? 0,
                        Description = module.Description,
                        Name = module.Name,
                        Thumbnail = module.Thumbnail
                    };
                }
            }
        }

        private static IEnumerable<LessonInstruction> LessonInstructionClassification(IList<CreateLessonInstructionCommandModel>? lessonInstructions)
        {
            if (lessonInstructions != null)
            {
                foreach (var lessonInstruction in lessonInstructions)
                {
                    yield return new LessonInstruction
                    {
                        SkillId = lessonInstruction.SkillId,
                        Instruction = lessonInstruction.Instruction
                    };
                }
            }
        }

        public static LessonFactory Create(UpdateLessonCommandModel createRequest)
        {
            return new LessonFactory(createRequest);
        }
    }
}
