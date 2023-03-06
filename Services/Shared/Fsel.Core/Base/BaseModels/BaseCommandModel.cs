using System.Text.Json.Serialization;

namespace Fsel.Core.Base.BaseModels
{
    public class BaseCommandModel
    {
        [JsonIgnore]
        public Guid? Id { get; set; }
    }
}