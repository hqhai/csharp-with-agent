using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fsel.Core.Base.BaseModels;

namespace Fsel.Course.Lms.Application.Services.SystemService.Models
{
    public class ChatbotTokenConfigsModel : BaseModel
    {
        public int ReadingToken { get; set; }
        public int ListeningToken { get; set; }
        public int WritingToken { get; set; }
        public int SpeakingToken { get; set; }
        public int VocabularyToken { get; set; }
        public int GrammarToken { get; set; }
    }
}
