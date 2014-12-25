using System;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms.VisualStyles;
using EasyHook;
using Timer=System.Timers.Timer;
namespace Injector
{
    public delegate void GetBeatmapIdEventHandler(string beatmapId);
    public class HookManager
    {
        public static event GetBeatmapIdEventHandler GetBeatmapId;
        private static string _lastBeatmap = string.Empty;
        private static Timer timer = new Timer(1000);
        private  LocalHook _shellExecuteExWHook;
        private  LocalHook _createProcessWHook;
        private EntryPoint _entryPoint;
        public HookManager(EntryPoint point)
        {
            this._entryPoint = point;
            timer.Elapsed += (sender, args) =>
            {
                timer.Stop();
                _lastBeatmap = string.Empty;
            };
        }
        public  bool StartHook()
        {
            try
            {
                _shellExecuteExWHook = LocalHook.Create(
                    LocalHook.GetProcAddress("shell32.dll", "ShellExecuteExW"),
                    new DShellExecuteEx(ShellExecuteEx_Hooked),
                    _entryPoint);

                _shellExecuteExWHook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });

                _createProcessWHook = LocalHook.Create(
                    LocalHook.GetProcAddress("kernel32.dll", "CreateProcessW"),
                    new DCreateProcessW(CreateProcess_Hooked),
                    _entryPoint);

                _createProcessWHook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
                return true;
            }
            catch (Exception extInfo)
            {
                throw new Exception("Hook Failed"+extInfo);
            }
        }
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        private delegate bool DShellExecuteEx(ref Native.SHELLEXECUTEINFO lpExecInfo);
        private static bool ShellExecuteEx_Hooked(ref Native.SHELLEXECUTEINFO lpExecInfo)
        {
            var This = (EntryPoint)HookRuntimeInfo.Callback;
            int opindex = lpExecInfo.lpFile.IndexOf(@"http://osu.ppy.sh/b/");
            if (opindex != -1)
            {
                string beatmapId = lpExecInfo.lpFile.Remove(0, opindex + 20);
                lock (_lastBeatmap)
                {
                    if (_lastBeatmap == beatmapId)
                    {
                    }
                    else
                    {
                        _lastBeatmap = beatmapId;
                        if (GetBeatmapId != null) GetBeatmapId(beatmapId);
                        timer.Start();
                        return true;
                    }
                }
            }
            return Native.ShellExecuteExW(ref lpExecInfo);
        }
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        private delegate bool DCreateProcessW(
            string lpApplicationName,
            string lpCommandLine,
            ref Native.SECURITY_ATTRIBUTES lpProcessAttributes,
            ref Native.SECURITY_ATTRIBUTES lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            [In] ref Native.STARTUPINFO lpStartupInfo,
            out Native.PROCESS_INFORMATION lpProcessInformation);
        private static bool CreateProcess_Hooked(
            string lpApplicationName,
            string lpCommandLine,
            ref Native.SECURITY_ATTRIBUTES lpProcessAttributes,
            ref Native.SECURITY_ATTRIBUTES lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            [In] ref Native.STARTUPINFO lpStartupInfo,
            out Native.PROCESS_INFORMATION lpProcessInformation)
        {

            EntryPoint This = (EntryPoint)HookRuntimeInfo.Callback;
            int opindex = lpCommandLine.IndexOf(@"http://osu.ppy.sh/b/");
            if (opindex != -1)
            {
                string beatmapId = lpCommandLine.Remove(0, opindex + 20);
                lock (_lastBeatmap)
                {
                    if (_lastBeatmap == beatmapId)
                    {
                    }
                    else
                    {
                        if (GetBeatmapId != null) GetBeatmapId(beatmapId);
                        _lastBeatmap = beatmapId;
                        timer.Start();
                        lpProcessInformation = new Native.PROCESS_INFORMATION();
                        return true;
                    }
                }
            }
            return Native.CreateProcessW(
                lpApplicationName,
                lpCommandLine,
                ref lpProcessAttributes,
                ref lpThreadAttributes,
                bInheritHandles,
                dwCreationFlags,
                lpEnvironment,
                lpCurrentDirectory,
                ref lpStartupInfo,
                out lpProcessInformation);
        }
    }
}