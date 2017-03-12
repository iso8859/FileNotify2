using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FileNotify2;

namespace fnAdmin
{
    public partial class main : Form
    {
        Settings m_settings = new Settings();
        Setting m_current;

        public main()
        {
            InitializeComponent();
        }

        protected void FillScreen(Setting setting)
        {
            if (setting != null)
            {
                txtName.Text = setting.m_name;
                txtDirectory.Text = setting.m_directory;
                txtFilter.Text = setting.m_filter;
                cronComp1.SetTriggerValue(setting.m_cron);
                btnActive.Checked = setting.m_active;
                btnPersist.Checked = setting.m_persistant;
                txtScript.Text = setting.m_script;
            }
            else
            {
                txtName.Text = "";
                txtDirectory.Text = "";
                txtFilter.Text = "";
                cronComp1.SetTriggerValue(null);
                btnActive.Checked = false;
                btnPersist.Checked = false;
                txtScript.Text = "";
            }
        }

        protected Setting GetScreen()
        {
            Setting result = new Setting();
            result.m_guid = m_current.m_guid;
            result.m_name = txtName.Text;
            result.m_directory = txtDirectory.Text;
            result.m_filter = txtFilter.Text;
            result.m_cron = cronComp1.GetTriggerValue();
            result.m_active = btnActive.Checked;
            result.m_persistant = btnPersist.Checked;
            result.m_script = txtScript.Text;
            return result;
        }

        protected Setting Find(Guid guid)
        {
            Setting result = null;
            foreach (Setting setting in m_settings.m_settings)
            {
                if (setting.m_guid == guid)
                {
                    result = setting;
                    break;
                }
            }
            return result;
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                m_settings = Settings.LoadFromFile(openFileDialog1.FileName);
                if (m_settings != null)
                {
                    lstRules.Items.Clear();
                    lstRules.Items.AddRange(m_settings.m_settings.ToArray());
                }
                else
                    MessageBox.Show("Error loading the file " + openFileDialog1.FileName);
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                m_settings.SaveToFile(saveFileDialog1.FileName);
        }

        private void newRuleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_current = new Setting();
            m_current.m_guid = Guid.NewGuid();
            m_current.m_active = true;
            FillScreen(m_current);
            lstRules.Enabled = false;
            groupBox1.Enabled = true;
            fileToolStripMenuItem.Enabled = false;
            txtName.Focus();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Setting setting = lstRules.SelectedItem as Setting;
            if (setting != null)
            {
                m_settings.m_settings.Remove(setting);
                lstRules.Items.Clear();
                lstRules.Items.AddRange(m_settings.m_settings.ToArray());
            }
        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_current = lstRules.SelectedItem as Setting;
            if (m_current != null)
            {
                FillScreen(m_current);
                lstRules.Enabled = false;
                groupBox1.Enabled = true;
                fileToolStripMenuItem.Enabled = false;
            }
        }

        private void lstRules_SelectedIndexChanged(object sender, EventArgs e)
        {
            editToolStripMenuItem_Click(sender, e);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (m_current != null)
            {
                Setting newOne = GetScreen();
                Setting oldOne = Find(newOne.m_guid);
                if (oldOne!=null)
                    m_settings.m_settings.Remove(oldOne);
                m_settings.m_settings.Add(newOne);

                lstRules.Items.Clear();
                lstRules.Items.AddRange(m_settings.m_settings.ToArray());
            }
            m_current = null;
            FillScreen(null);
            lstRules.Enabled = true;
            groupBox1.Enabled = false;
            fileToolStripMenuItem.Enabled = true;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            m_current = null;
            btnSave_Click(sender, e);
        }

        private void btnSelectDirectory_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.SelectedPath = txtDirectory.Text;
            if (fbd.ShowDialog() == DialogResult.OK)
                txtDirectory.Text = fbd.SelectedPath;
        }

        private void main_Load(object sender, EventArgs e)
        {
            try
            {
                string[] templates = System.IO.Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "\\Templates", "*.cs");
                foreach (string file in templates)
                    cboTemplates.Items.Add(System.IO.Path.GetFileNameWithoutExtension(file));
            }
            catch { }
        }

        private void btnCopyTemplate_Click(object sender, EventArgs e)
        {
            if (cboTemplates.SelectedIndex > -1)
            {
                using (System.IO.StreamReader sr = new System.IO.StreamReader(AppDomain.CurrentDomain.BaseDirectory + "\\Templates\\" + cboTemplates.SelectedItem.ToString() + ".cs"))
                {
                    txtScript.Text = sr.ReadToEnd();
                }
            }
        }
    }
}