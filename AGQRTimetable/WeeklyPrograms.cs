using System.Collections.Generic;
using Newtonsoft.Json;

namespace AGQRTimetable {
  public class WeeklyPrograms {
    [JsonProperty("weekly")]
    public List<DailyPrograms> Dailies;
  }
}
