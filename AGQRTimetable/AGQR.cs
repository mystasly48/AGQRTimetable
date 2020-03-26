using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace AGQRTimetable {
  public class AGQR {
    public static readonly string TimetableUrl = "http://www.agqr.jp/timetable/streaming.html";
    private static readonly Uri TimetableUri = new Uri(TimetableUrl);

    public DateTime ExpiryDateTime { get; private set; }
    public DateTime UpdatedDateTime { get; private set; }
    public List<DailyPrograms> All { get; private set; }

    public bool IsExpired {
      get {
        // ExpiryTime is 05:00, and if it is, the timetable is alredy expired;
        return (this.ExpiryDateTime <= DateTime.Now);
      }
    }

    public AGQRProgram Now {
      get {
        if (this.UpdatedDateTime.Hour == 5 && DateTime.Now.Hour == 5) {
          return GetPauseProgram();
        } else {
          return this.Today.Programs.Where(x => x.Start <= DateTime.Now && DateTime.Now <= x.End).FirstOrDefault();
        }
      }
    }

    public DailyPrograms Today {
      get {
        if (this.UpdatedDateTime.Hour == 5 && DateTime.Now.Hour == 5) {
          return new DailyPrograms() {
            Date = GetSpecializedDate(DateTime.Now).Date,
            Programs = new List<AGQRProgram> { GetPauseProgram() }
          };
        } else {
          return this.All.Where(x => x.Date.Date == GetSpecializedDate(DateTime.Now).Date).FirstOrDefault();
        }
      }
    }

    public string JsonSimple {
      get {
        return JsonConvert.SerializeObject(this.All);
      }
    }

    public string JsonFormatted {
      get {
        return JsonConvert.SerializeObject(this.All, Formatting.Indented);
      }
    }

    public AGQR() {
      this.All = Scraping();
    }

    public void Refresh() {
      this.All = Scraping();
    }

    private List<DailyPrograms> Scraping() {
      List<DailyPrograms> res = new List<DailyPrograms>();

      HtmlDocument doc = new HtmlDocument();
      string html = GetHtml(TimetableUrl);
      doc.LoadHtml(html);

      this.UpdatedDateTime = DateTime.Now;

      // according to today.js, the timetable will be changed at 5 AM.
      DateTime today = DateTime.Now;
      if (today.Hour < 5) {
        today = today.AddDays(-1);
      }
      today = new DateTime(today.Year, today.Month, today.Day, 6, 0, 0);

      this.ExpiryDateTime = today.AddDays(7).AddHours(-1);

      List<DateTime> times = Enumerable.Repeat(new DateTime(), 7).ToList();
      for (int i = 0; i < 7; i++) {
        int p = (i + (int)today.DayOfWeek - 1) % 7;
        if (p < 0) {
          p += 7;
        }
        times[p] = today + new TimeSpan(i, 0, 0, 0);
      }

      for (int i = 0; i < 7; i++) {
        DailyPrograms dp = new DailyPrograms();
        dp.Date = times[i].Date;
        dp.Programs = new List<AGQRProgram>();
        res.Add(dp);
      }

      var tbody = doc.DocumentNode.SelectNodes("//table/tbody/tr").Where(x => x.InnerText != "\n");
      foreach (var tr in tbody) {
        foreach (var td in tr.SelectNodes("./td") ?? Enumerable.Empty<HtmlNode>()) {
          AGQRProgram program = new AGQRProgram();

          // ProgramType
          program.ProgramType = GetProgramType(td.GetAttributeValue("class", "").Trim());

          // Title
          program.Title = td.SelectSingleNode("./div[@class='title-p']").InnerText.Trim();

          // Length
          program.Length = int.Parse(td.GetAttributeValue("rowspan", "").Trim());
          if (program.Title == "ラジオアニメージュ" && program.Length == 29) {
            program.Length = 30;
          }
          
          // URLs
          program.URLs = new List<string>();
          var titlepa = td.SelectSingleNode("./div[@class='title-p']/a");
          if (titlepa != null) {
            program.URLs.Add(titlepa.GetAttributeValue("href", "").Trim());
          }
          foreach (var a in td.SelectNodes("./div[@class='bnr']/a") ?? Enumerable.Empty<HtmlNode>()) {
            program.URLs.Add(a.GetAttributeValue("href", "").Trim());
          }
          program.URLs = program.URLs.Distinct().ToList();

          // Images
          program.Images = new List<string>();
          foreach (var img in td.SelectNodes("./div[@class='bnr']//img") ?? Enumerable.Empty<HtmlNode>()) {
            program.Images.Add(new Uri(TimetableUri, img.GetAttributeValue("src", "").Trim()).AbsoluteUri);
          }
          program.Images = program.Images.Distinct().ToList();

          // Hosts
          program.Hosts = td.SelectSingleNode("./div[@class='rp']").InnerText.Trim().Split('、').Distinct().ToList();

          // Emails
          program.Emails = new List<string>();
          foreach (var s in new string[] { "./div[@class='rp']/a", "./a" }) {
            foreach (var a in td.SelectNodes(s) ?? Enumerable.Empty<HtmlNode>()) {
              if (a.SelectNodes("./img") != null) {
                program.Emails.Add(a.GetAttributeValue("href", "").Trim().Replace("mailto:", ""));
              }
            }
          }
          program.Emails = program.Emails.Distinct().ToList();

          // StreamType
          program.StreamType = AGQRStreamType.Audio;
          if (td.SelectSingleNode("./div[@class='time']/span/img") != null) {
            program.StreamType = AGQRStreamType.Movie;
          }

          // Start, End
          var min = getMinTimeSpan(times);
          for (int i = 0; i < times.Count(); i++) {
            if (min == times[i].TimeOfDay) {
              program.Start = times[i];
              times[i] = times[i].AddMinutes(program.Length);
              program.End = times[i];
              if (times[i].Hour == 6) {
                times.RemoveAt(i);
              }
              break;
            }
          }

          var spDate = GetSpecializedDate(program.Start);
          for (int i = 0; i < 7; i++) {
            if (res[i].Date == spDate.Date) {
              res[i].Programs.Add(program);
              break;
            }
          }
        }
      }

      return res;
    }

    // for debug
    private void PrintProgramInfo(AGQRProgram program) {
      Console.WriteLine("Title: {0}", program.Title);
      Console.WriteLine("Hosts: {0}", string.Join(", ", program.Hosts));
      Console.WriteLine("Program type: {0}", program.ProgramType);
      Console.WriteLine("Stream type: {0}", program.StreamType);
      Console.WriteLine("Starts at: {0}", program.Start);
      Console.WriteLine("Ends at: {0}", program.End);
      Console.WriteLine("Length: {0}", program.Length);
      Console.WriteLine("Emails: {0}", string.Join(",\n", program.Emails));
      Console.WriteLine("URLs: {0}", string.Join(",\n", program.URLs));
      Console.WriteLine("Images: {0}", string.Join(",\n", program.Images));
      Console.WriteLine();
    }

    /// <summary>
    /// 0時から5時の場合、前日の6時を返す。
    /// それ以外の場合、その日の6時を返す。
    /// </summary>
    public static DateTime GetSpecializedDate(DateTime dt) {
      if (dt.Hour < 6) {
        return dt.AddDays(-1).Date.AddHours(6);
      } else {
        return dt.Date.AddHours(6);
      }
    }

    private AGQRProgram GetPauseProgram() {
      AGQRProgram p = new AGQRProgram();
      p.Start = DateTime.Now;
      p.End = new DateTime(p.Start.Year, p.Start.Month, p.Start.Day, 6, 0, 0);
      p.Length = (int)Math.Floor((p.End - p.Start).TotalMinutes);
      p.ProgramType = AGQRProgramType.Pause;
      p.StreamType = AGQRStreamType.Movie;
      p.Title = "放送休止";
      p.Emails = new List<string>();
      p.Hosts = new List<string>();
      p.Images = new List<string>();
      p.URLs = new List<string>();
      return p;
    }

    private TimeSpan getMinTimeSpan(List<DateTime> times) {
      var minTS = times[0].TimeOfDay;
      var minMS = minTS.TotalMinutes;
      minMS += (minTS.Hours < 6) ? 24 * 60 : 0;
      for (int i = 1; i < times.Count(); i++) {
        var ts = times[i].TimeOfDay;
        var ms = ts.TotalMinutes;
        ms += (ts.Hours < 6) ? 24 * 60 : 0;
        if (ms < minMS) {
          minMS = ms;
          minTS = ts;
        }
      }
      return minTS;
    }

    private AGQRProgramType GetProgramType(string type) {
      switch (type) {
        case "bg-f":
          return AGQRProgramType.Normal;
        case "bg-l":
          return AGQRProgramType.Live;
        case "bg-repeat":
          return AGQRProgramType.Repeat;
        case "bg-etc":
          return AGQRProgramType.Pause;
        default:
          return AGQRProgramType.Pause;
      }
    }

    private string GetHtml(string url) {
      using (HttpClient client = new HttpClient()) {
        return client.GetStringAsync(url).Result;
      }
    }
  }
}
