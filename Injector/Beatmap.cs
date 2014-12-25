using System;
using System.IO;
using System.Linq;
using System.Net;

namespace Injector
{    
    public class DownloadEventArgs : EventArgs
        {
            public readonly string FileName;
            public readonly long Downloaded;
            public readonly long FileLength;
            public readonly int Progress;
            public DownloadEventArgs(string fileName,long downloaded,long fileLength)
            {
                this.FileName = fileName;
                this.Downloaded = downloaded;
                this.FileLength = fileLength;
                this.Progress = Convert.ToInt32((downloaded * 100L) / fileLength);
            }
            public DownloadEventArgs(string fileName, long fileLength)
            {
                this.FileName = fileName;
                this.FileLength = fileLength;
            }
        }
    class Beatmap
    {        
        public event EventHandler<DownloadEventArgs> DownloadFinished;
        public event EventHandler<DownloadEventArgs> ProgressChanged;
        public event EventHandler<DownloadEventArgs> DownloadCanceled;
        private string _setId;
        private string _name;
        private readonly string _fileName;
        private readonly long _fileLength;
        private long _downloaded;
        private bool _state;
        private readonly Stream _downloadStream;
        private readonly HttpWebResponse _response;
        public string Name
        {
            get { return _name; }
        }
        public string FileName
        {
            get { return _fileName; }
        }
        public long FileLength
        {
            get { return _fileLength; }
        }
        private Beatmap(string setId, IBeatmapMirrorSiteClient mirrorSite)
        {
            _response = HttpWebHelper.GetResponse(mirrorSite.UrlDownload + setId);
            _setId = setId;
            _fileName = mirrorSite.GetFileName(_response);
            _fileLength = _response.ContentLength;
            _name = GetBeatmapName(_fileName);
            _downloaded = 0;
            _downloadStream = _response.GetResponseStream();
        }
        public static Beatmap GetBeatmapByBeatmapId(string beatmapId,IBeatmapMirrorSiteClient mirrorSite)
        {
            
            var result = mirrorSite.SearchBeatmap(beatmapId);
            if (result == null)
            {
                return null;
            }
            var beatmap = new Beatmap(result.First().Key,mirrorSite) {_name = result.First().Value};
            return beatmap;
        }
        public bool CanDownload()
        {
            if (_response.Headers["Content-Disposition"] != null)
            {
                return true;
            }
            return false;
        }
        public void StopDownload()
        {
            _state = false;
        }
        public void StartDownload()
        {
            using (var osz = new FileStream(_fileName, FileMode.Create))
            {
                _state = true;
                var bytes = new byte[512];
                var progress = 0;
                try
                {
                    int bufferSize = _downloadStream.Read(bytes, 0, 512);
                    while (bufferSize > 0)
                    {
                        if (!_state)
                        {
                            DownloadCanceled(this,new DownloadEventArgs(_fileName,_fileLength));
                            return;
                        }
                        _downloaded += bufferSize;
                        if (progress != GetProgress())
                        {
                            progress = GetProgress();
                            if (ProgressChanged != null)
                                ProgressChanged(this, new DownloadEventArgs(_fileName, _downloaded, _fileLength));
                        }
                        osz.Write(bytes, 0, bufferSize);
                        bufferSize = _downloadStream.Read(bytes, 0, 512);
                    }
                    if (DownloadFinished != null)
                        DownloadFinished(this, new DownloadEventArgs(_fileName, _fileLength));
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    osz.Close();
                    _downloadStream.Close();
                }
            }
        }
        public int GetProgress()
        {
            return Convert.ToInt32((_downloaded * 100L) / _fileLength);
        }
        private static string GetBeatmapName(string fileName)
        {
            int end = fileName.LastIndexOf(".osz");
            int start = fileName.IndexOf(" ");
            return fileName.Substring(start, end - start);
        }
    }

}