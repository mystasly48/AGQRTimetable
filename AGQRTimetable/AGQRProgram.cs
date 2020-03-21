using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace AGQRTimetable {
  public class AGQRProgram {
    [JsonProperty("start")]
    public DateTime Start;
    [JsonProperty("end")]
    public DateTime End;
    [JsonProperty("length")]
    public int Length;
    [JsonProperty("program_type")]
    public AGQRProgramType ProgramType;
    [JsonProperty("stream_type")]
    public AGQRStreamType StreamType;
    [JsonProperty("title")]
    public string Title;
    [JsonProperty("hosts")]
    public List<string> Hosts;
    [JsonProperty("urls")]
    public List<string> URLs;
    [JsonProperty("emails")]
    public List<string> Emails;
    [JsonProperty("images")]
    public List<string> Images;
  }
}
