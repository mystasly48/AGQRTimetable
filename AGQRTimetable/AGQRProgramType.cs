using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AGQRTimetable {
  [JsonConverter(typeof(StringEnumConverter))]
  public enum AGQRProgramType {
    [JsonProperty("normal")]
    Normal,
    [JsonProperty("live")]
    Live,
    [JsonProperty("repeat")]
    Repeat,
    [JsonProperty("pause")]
    Pause
  }
}
