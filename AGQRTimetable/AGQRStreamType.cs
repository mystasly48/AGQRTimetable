using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AGQRTimetable {
  [JsonConverter(typeof(StringEnumConverter))]
  public enum AGQRStreamType {
    Audio,
    Movie
  }
}
