using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Threading;
using EasyHook;
using InjectLoader;
using Timer = System.Timers.Timer;

namespace Injector
{
    public class EntryPoint : IEntryPoint
    {
        private readonly Main _main;
        public readonly string ChannelName;
        public EntryPoint(
            RemoteHooking.IContext inContext,
            String inChannelName)
        {
            this.ChannelName = inChannelName;
            _main=new Main(this);
        }
        public void Run(
            object inContext,
            String inChannelName)
        {
            _main.Run();
            while (true)
            {
                Thread.Sleep(5000);
            }
        }
    }
}
