using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading;
using InjectLoader;
using Timer=System.Timers.Timer;

namespace Injector
{
    public class Main
    {
        private readonly InjectManager _injectManager; 
        private readonly SpeechSynthesizer _synth;
        private readonly IBeatmapMirrorSiteClient _mirrorSite;
        private readonly HookManager _hookManager;
        private readonly Timer _progressTimer = new Timer(4000);
        private ProgressForm _progressForm;
        private Beatmap _beatmap;
        private static string _downloading=string.Empty;
        private static Thread _downloadThread;
        private bool _canSpeak = true;
        public Main(EntryPoint entryPoint)
        {
            this._injectManager=new InjectManager(entryPoint.ChannelName);
            this._hookManager=new HookManager(entryPoint);
            this._mirrorSite=new BloodCatClient();
            this._progressTimer.Elapsed += (sender, args) =>
            {
                _progressTimer.Stop();
                _canSpeak = true;
            };
            _synth=new SpeechSynthesizer(){Volume = 100};
            _synth.SelectVoiceByHints(VoiceGender.Male, VoiceAge.Adult, 0, new CultureInfo("en-us"));
        }

        public void Run()
        {
            try
            {
                HookManager.GetBeatmapId += HookManager_GetBeatmapId;
                _hookManager.StartHook();
            }
            catch (Exception info)
            {
                _injectManager.ReportErrorMsg(info);
                return;
            }
        }

        void HookManager_GetBeatmapId(string beatmapId)
        {
                if (_downloading == beatmapId)
                {
                    CancelDownload();
                    return;
                }
                else
                {
                    if (_downloading != string.Empty && _downloading != beatmapId)
                    {
                        CancelDownload();
                    }
                    _downloading = beatmapId;
                    _downloadThread = new Thread(o => Download(beatmapId));
                    _downloadThread.Start();
                }
        }

        private void Download(string beatmapId)
        {
            try
            {
                _canSpeak = true;
                _beatmap = Beatmap.GetBeatmapByBeatmapId(beatmapId, _mirrorSite);
                _beatmap.DownloadFinished += beatmap_DownloadFinished;
                _beatmap.ProgressChanged += beatmap_ProgressChanged;
                _injectManager.OnBeatmapDownload(_beatmap.FileName);
                _synth.SpeakAsyncCancelAll();
                _synth.Speak("New Download Task Start! " + _beatmap.Name);
                _progressForm = new ProgressForm(_beatmap.FileName);
                _progressForm.Show();
                _beatmap.StartDownload();
            }
            catch (Exception info)
            {
                _synth.SpeakAsyncCancelAll();
                _synth.SpeakAsync("Download Failed!");
                _downloading = string.Empty;
                _injectManager.ReportErrorMsg(info);
                return;
            }
        }

        void CancelDownload()
        {
            _progressForm.Close();
            _beatmap.StopDownload();
            _beatmap.DownloadCanceled += (sender, args) =>
            {
                _downloading = string.Empty;
                _synth.Speak("Download Task Canceled!");
            };
        }

        void beatmap_ProgressChanged(object sender, DownloadEventArgs e)
        {
            _progressForm.SetProgress(e.Progress);
            if (_canSpeak)
            {
                _synth.SpeakAsyncCancelAll();
                _synth.SpeakAsync(e.Progress + "% Downloaded!");
                _canSpeak = false;
                _progressTimer.Start();
            }
        }

        void beatmap_DownloadFinished(object sender, DownloadEventArgs e)
        {
            _progressForm.Close();
            Process.Start(e.FileName);
            _synth.SpeakAsyncCancelAll();
            _synth.SpeakAsync("Congratulation! Beatmap Downloaded Successfully!");
            _progressTimer.Stop();
            _canSpeak = true;
            _downloading=String.Empty;
        }
    }
}
