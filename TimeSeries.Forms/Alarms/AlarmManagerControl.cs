﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Reclamation.Core;
using Reclamation.TimeSeries.Alarms;

namespace Reclamation.TimeSeries.Forms.Alarms
{
    public partial class AlarmManagerControl : UserControl
    {
        BasicDBServer m_svr;

        public AlarmManagerControl()
        {
            InitializeComponent();
        }
        public AlarmManagerControl(TimeSeriesDatabase db)
        {
            InitializeComponent();
            m_svr = db.Server;

            SoundFiles s = new SoundFiles(m_svr);
            this.tabPageSounds.Controls.Add(s);
            s.Dock = DockStyle.Fill;

            AlarmDataSet ds = AlarmDataSet.CreateInstance(db);
            AlarmSetup s1 = new AlarmSetup(ds);
            this.tabPageSetup.Controls.Add(s1);
            s1.Dock = DockStyle.Fill;


            AlarmQueue q = new AlarmQueue(ds);
            this.tabPageAlarms.Controls.Add(q);
            q.Dock = DockStyle.Fill;

            AlarmDefinitionUI def = new AlarmDefinitionUI(ds);
            this.tabPageAlarmDef.Controls.Add(def);
            def.Dock = DockStyle.Fill;

            AlarmLog log = new AlarmLog(ds);
            this.tabPageLog.Controls.Add(log);
            log.Dock = DockStyle.Fill;

        }
    }
}
