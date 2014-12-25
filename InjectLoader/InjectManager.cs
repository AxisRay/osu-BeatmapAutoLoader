using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using EasyHook;

namespace InjectLoader
{
    public delegate void InjectFailedEventHandler(string errorMsg);

    class InjectManager
    {
        public event InjectFailedEventHandler InjectFailed;
        public event EventHandler InjectSuccess;
        private readonly int _targetProcessId;
        private readonly string _channelName;
        public InjectManager(int targetProcess)
        {
            this._targetProcessId = targetProcess;
            RemoteHooking.IpcCreateServer<InjectInterface>(ref _channelName, WellKnownObjectMode.SingleCall);
        }
        public bool TryInject()
        {
            RemoteHooking.InstallSupportDriver();
            try
            {
                RemoteHooking.Inject(_targetProcessId, "Injector.dll", "Injector.dll", _channelName);
                if (InjectSuccess != null) InjectSuccess(this,EventArgs.Empty);
                return true;
            }
            catch (Exception errorMsg)
            {
                if (InjectFailed != null) InjectFailed(errorMsg.ToString());
                return false;
            }
        }
    }
}
