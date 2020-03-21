using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AGQRTimetable;

namespace SampleForms {
  public partial class Form1 : Form {
    DataGridView[] views;
    Label[] labels;

    public Form1() {
      InitializeComponent();

      views = new DataGridView[] {
        monGridView, tueGridView, wedGridView, thuGridView, friGridView, satGridView, sunGridView
      };
      labels = new Label[] {
        monLabel, tueLabel, wedLabel, thuLabel, friLabel, satLabel, sunLabel
      };
    }

    private void Form1_Load(object sender, EventArgs e) {
      AGQR agqr = new AGQR();

      for (int i = 0; i < 7; i++) {
        labels[i].Text = agqr.All.Dailies[i].Date.ToString("MM/dd (ddd)", CultureInfo.CreateSpecificCulture("en-US"));
        foreach (var p in agqr.All.Dailies[i].Programs) {
          views[i].Rows.Add(p.Start.ToString("HH:mm"), p.Title);
        }
      }

      foreach (var v in views) {
        if (v.Controls.OfType<VScrollBar>().Count(x => x.Visible) > 0) {
          v.Columns[1].Width = 190;
        } else {
          v.Columns[1].Width = 205;
        }
      }

      string path = "C:/Programming/Csharp/Library/AGQRTimetable/SampleForms/bin/Debug/Timetable.json";
      using (StreamWriter sw = new StreamWriter(path, false, Encoding.UTF8)) {
        sw.WriteLine(agqr.JsonFormatted);
      }
    }

    private void Form1_Resize(object sender, EventArgs e) {
      foreach (var v in views) {
        if (v.Controls.OfType<VScrollBar>().Count(x => x.Visible) > 0) {
          v.Columns[1].Width = 190;
        } else {
          v.Columns[1].Width = 205;
        }
      }
    }
  }
}
