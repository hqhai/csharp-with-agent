using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Domain.Enums
{
    public enum EnumQuestionType
    {
        Multichoice = 1,
        Dropdown,
        Checklist,
        Listing,
        MatchingType1,
        MatchingType2,
        ShortAnswer,
        GapFill,
        GapFillWordBank,
        DragAndDropSsentenceOrder,
        DragAndDropPicture,
        MultipleOptionSentenceCompletion,
        ExercisePreparation,
    }
}