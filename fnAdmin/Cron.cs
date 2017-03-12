using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using FileNotify2;

namespace fnAdmin
{
    public partial class CronComp : UserControl
    {
        public CronComp()
        {
            InitializeComponent();
        }

        public Trigger GetTriggerValue()
        {
            Trigger result = new Trigger();
            result.m_year = txtYear.Text;
            result.m_month = txtMonth.Text;
            result.m_day = txtDay.Text;
            result.m_dow = txtDOW.Text;
            result.m_hour = txtHour.Text;
            result.m_min = txtMin.Text;
            result.m_sec = txtSec.Text;
            result.m_mode = Trigger.Mode.Schedule;
            return result;
        }

        public void SetTriggerValue(Trigger value)
        {
            if (value == null)
                value = new Trigger();

            txtYear.Text = value.m_year;
            txtMonth.Text = value.m_month;
            txtDay.Text = value.m_day;
            txtDOW.Text = value.m_dow;
            txtHour.Text = value.m_hour;
            txtMin.Text = value.m_min;
            txtSec.Text = value.m_sec;
        }
    }
}
