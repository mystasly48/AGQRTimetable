using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace AGQRTimetable {
  public class DailyPrograms {
    [JsonProperty("date")]
    public DateTime Date;
    [JsonProperty("programs")]
    public List<AGQRProgram> Programs;
  }
}
