using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Timer = System.Timers.Timer;

namespace InjectLoader
{
    public delegate void InjectorOnlineEventHandler(int InClientPID);
    public delegate void InjectorOfflineEventHandler(string errorinfo);
    public delegate void DownloadBeatmapEventHandler(string Beatmap);

    public class InjectInterface : MarshalByRefObject
    {
        public static event EventHandler InjectInterfaceInitialized;
        public event EventHandler ErrorReport;
        public event InjectorOnlineEventHandler InjectorOnline;
        public event InjectorOfflineEventHandler InjectorOffline;
        public event DownloadBeatmapEventHandler DownloadBeatmap;
        private readonly SpeechSynthesizer _synth;
        private readonly Timer _watchDog;
        private static bool _flag=false;

        public InjectInterface()
        {
            InjectInterfaceInitialized(this, EventArgs.Empty);
            _watchDog=new Timer(1500);
            
            _synth = new SpeechSynthesizer {Volume = 100};
            _synth.SelectVoiceByHints(VoiceGender.Male, VoiceAge.Adult, 0, new CultureInfo("en-us"));
            
            InjectorOnline+=pid => _synth.SpeakAsync("Injector Online!");
            
            _watchDog.Elapsed += (sender, args) =>
            {
                if (_flag)
                {
                    _flag = false;
                }
                else
                {
                    _watchDog.Stop();
                    if (InjectorOffline != null) InjectorOffline("Unknow Error");
                    _synth.SpeakAsync(Process.GetProcessesByName("osu!").Any() ? "Injector Offline!" : "See you!");
                }
            };
        }

        public void IsInstalled(int InClientPID)
        {
            if (InjectorOnline != null) InjectorOnline(InClientPID);
            _flag = true;
            _watchDog.Start();
        }

        public void ReportException(Exception inInfo)
        {
            if (ErrorReport != null) ErrorReport(inInfo, EventArgs.Empty);
        }

        public void OnDownloadBeatmap(string beatmap)
        {
            if (DownloadBeatmap != null) DownloadBeatmap(beatmap);
        }

        public void Ping()
        {
            _flag = true;
        }
    }
}
