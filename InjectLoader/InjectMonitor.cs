using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace InjectLoader
{
    public delegate void ProcessDetectedEventHandler(int pid);
    public partial class InjectMonitor : Form
    {
        public event ProcessDetectedEventHandler osuDetected;
        public InjectMonitor()
        {
            InitializeComponent();
            EventReg();
        }
        
        private void InjectMonitor_Load(object sender, EventArgs e)
        {
            InjectorState("Waiting");
            if (!Process.GetProcessesByName("osu!").Any())
            {
                InjectorState("osu! is not running...");
                if (Program.OsuPatch != null)
                {
                    Process.Start(Program.OsuPatch);
                }
                else
                {
                    AddLog("Warning", "Cannot find osu! in default directory,please run osu! manually!");
                }
            }
        }

        private void EventReg()
        {
            osuDetected += pid =>
            {
                timer_osuDetector.Stop();
                var inject = new InjectManager(pid);
                inject.InjectFailed += inject_InjectFailed;
                inject.InjectSuccess += inject_InjectSuccess;
                inject.TryInject();

            };
            InjectInterface.InjectInterfaceInitialized+= (sender, args) =>
            {
                var _interface = (InjectInterface) sender;
                _interface.ErrorReport += (o, eventArgs) => AddLog("Error", o.ToString());
                _interface.InjectorOnline += pid =>
                {
                    InjectorState( "Injector Online");
                    AddLog("info", "Injector Online At " + pid.ToString());
                };
                _interface.InjectorOffline += errorinfo =>
                {
                    AddLog("Warning","Injector Offline!");
                    if (Process.GetProcessesByName("osu!").Any())
                    {
                        inject_InjectFailed("Unknow Error");
                    }
                    else
                    {
                        AddLog("Info","Application Exit");
                        Thread.Sleep(2000);
                        Application.Exit();
                    }
                };
            };

        }

        private void inject_InjectSuccess(object sender, EventArgs e)
        {
            AddLog("Msg", "Inject Success");
            InjectorState("Inject Success");
            FormVisible = false;
        }

        private void inject_InjectFailed(string errorMsg)
        {           
            AddLog("Error",errorMsg);
            InjectorState( "Inject Failed!");
            FormVisible = true;
            DialogResult dr=MessageBox.Show("Inject Failed! Try again?","Error",MessageBoxButtons.RetryCancel,MessageBoxIcon.Error);
            if (dr == DialogResult.Cancel)
            {
                //Application.Exit();
            }
            else
            {
                timer_osuDetector.Start();
            }
        }

        private void timer_osuDetector_Tick(object sender, EventArgs e)
        {
            var process = Process.GetProcessesByName("osu!");
            if (process.Any())
            {
                timer_osuDetector.Stop();
                if (osuDetected != null) osuDetected(process[0].Id);
            }
        }

        private bool FormVisible
        {
            set { BeginInvoke(new EventHandler((sender, args) => this.Visible = value)); }
            get { return Visible; }
        }
        private void InjectorState(string state)
        {
            BeginInvoke(new EventHandler((sender, args) => lbl_state.Text = state));
        }
        private void AddLog(string type, string text)
        {
            var time = DateTime.Now.ToShortTimeString();
            var message = string.Format("{0} {1}:{2}\n", time, type, text);
            BeginInvoke(new EventHandler((sender, args) =>
            {
                rtb_log.AppendText(message);
                rtb_log.ScrollToCaret();
            }));
            if(type!="Msg")
                notifyIcon.ShowBalloonTip(2000,type,text,type=="info"?ToolTipIcon.Info : ToolTipIcon.None);
        }

        private void notifyIcon_Click(object sender, EventArgs e)
        {
            FormVisible = !FormVisible;
        }

    }
}
