using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using EasyHook;
using InjectLoader;

namespace Injector
{
    class InjectManager
    {
        private readonly InjectInterface _interface;
        private readonly Timer _heartBeats=new Timer(500);
        public InjectManager(string inChannelName)
        {
            _interface = RemoteHooking.IpcConnectClient<InjectInterface>(inChannelName);
            _heartBeats.Elapsed += (sender, args) => _interface.Ping();
            Installed();
        }
        private void Installed()
        {
            _interface.IsInstalled(RemoteHooking.GetCurrentProcessId());
            _heartBeats.Start();
            RemoteHooking.WakeUpProcess();
        }
        public void ReportErrorMsg(Exception info)
        {
            _interface.ReportException(info);
        }

        public void OnBeatmapDownload(string beatmap)
        {
            _interface.OnDownloadBeatmap(beatmap);
        }
    }
}
