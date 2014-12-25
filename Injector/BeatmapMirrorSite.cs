using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Windows.Forms.VisualStyles;
using HtmlAgilityPack;

namespace Injector
{
    public interface IBeatmapMirrorSiteClient
    {
        string UrlDownload { get; }
        string UrlSearch { get; }
        Dictionary<string, string> SearchBeatmap(string keyWords);
        string GetFileName(HttpWebResponse response);
    }
    public class BloodCatClient : IBeatmapMirrorSiteClient
    {
        private const string UrlDownload = "http://bloodcat.com/osu/m/";
        private const string UrlSearch = "http://bloodcat.com/osu/?q=";

        string IBeatmapMirrorSiteClient.UrlDownload
        {
            get { return UrlDownload; }
        }

        string IBeatmapMirrorSiteClient.UrlSearch
        {
            get { return UrlSearch; }
        }

        public Dictionary<string, string> SearchBeatmap(string keyWords)
        {
            var searchResult=new Dictionary<string, string>();
            var responseStream = HttpWebHelper.GetResponseStream(UrlSearch + keyWords);
            var doc = new HtmlDocument();
            doc.Load(responseStream, Encoding.UTF8);
            var nodeHeaders = doc.DocumentNode.SelectNodes("/html[1]/head[1]/body[1]/div[2]/div[2]/div[1]");
            try
            {
                foreach (HtmlNode node in nodeHeaders)
                {
                    string beatmap = node.SelectSingleNode("./a[1]").InnerText;
                    string id = node.SelectSingleNode("./div[1]/a[1]").InnerText;
                    searchResult.Add(id, beatmap);
                }
            }
            catch (Exception)
            {
                return searchResult;
            }
            return searchResult;
        }

        public string GetFileName(HttpWebResponse response)
        {
            string headers = response.Headers["Content-Disposition"];
            int headst = headers.IndexOf("filename=", StringComparison.Ordinal);
            int headend = headers.IndexOf(".osz", headst, StringComparison.Ordinal);
            var fileName = headers.Substring(headst + 10, headend - headst - 6);
            return fileName;
        }
    }



    //public delegate void ProgressChangeEventHandler(int progress, string setId);

    //public delegate void DownloadFinishedEventHandler(string setId);

    //internal class BeatmapDownloader
    //{
    //    private const string DownloadUrl = "http://bloodcat.com/osu/m/";
    //    private long _downloaded;
    //    private long _fileLength;
    //    private string _fileName;
    //    private int _progress;
    //    private HttpWebResponse _response;
    //    public string setId;

    //    public BeatmapDownloader(string setId)
    //    {
    //        this.setId = setId;
    //        SetResponse(setId);
    //    }

    //    public event ProgressChangeEventHandler ProgressChanged;
    //    public event DownloadFinishedEventHandler DownloadFinished;

    //    public string GetSetId(string beatmapId)
    //    {
    //        return beatmapId;
    //    }

    //    public string GetFileName()
    //    {
    //        string headers = _response.Headers["Content-Disposition"];
    //        int headst = headers.IndexOf("filename=", StringComparison.Ordinal);
    //        int headend = headers.IndexOf(".osz", headst, StringComparison.Ordinal);
    //        _fileName = headers.Substring(headst + 10, headend - headst - 6);
    //        return _fileName;
    //    }

    //    private void SetResponse(string setId)
    //    {
    //        _response = HttpWebResponseUtility.CreateGetHttpResponse(DownloadUrl + setId, null, null, null);
    //    }

    //    public long GetFileLength()
    //    {
    //        return _response.ContentLength;
    //    }

    //    public int GetProgress()
    //    {
    //        int progress = Convert.ToInt32((_downloaded*100L)/_fileLength);
    //        return progress;
    //    }

    //    public bool CanDownload()
    //    {
    //        if (_response.Headers["Content-Disposition"] != null)
    //        {
    //            return true;
    //        }
    //        return false;
    //    }

    //    public string DownloadBeatMap()
    //    {
    //        GetFileName();
    //        Stream myStream = _response.GetResponseStream();
    //        var fsDownload = new FileStream(_fileName, FileMode.Create);
    //        _fileLength = GetFileLength();

    //        var btContent = new byte[512];
    //        _downloaded = 0;
    //        _progress = -1;

    //        if (myStream != null)
    //        {
    //            int intSize = myStream.Read(btContent, 0, 512);
    //            while (intSize > 0)
    //            {
    //                _downloaded += intSize;
    //                ;
    //                if (_progress != GetProgress() && ProgressChanged != null)
    //                {
    //                    _progress = GetProgress();
    //                    ProgressChanged(_progress, setId);
    //                }
    //                fsDownload.Write(btContent, 0, intSize);
    //                intSize = myStream.Read(btContent, 0, 512);
    //            }
    //            fsDownload.Close();
    //            myStream.Close();
    //        }
    //        if (DownloadFinished != null) DownloadFinished(setId);
    //        return _fileName;
    //    }
    //}

    //public static class SearchHelper
    //{
    //    public static Dictionary<string, string> Search(string keywords)
    //    {
    //        var resultDictionary = new Dictionary<string, string>();
    //        var request = WebRequest.Create("http://bloodcat.com/osu/?q=" + keywords) as HttpWebRequest;
    //        request.Method = "GET";
    //        request.UserAgent =
    //            "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";
    //        try
    //        {
    //            var response = request.GetResponse() as HttpWebResponse;
    //            var doc = new HtmlDocument();
    //            doc.Load(response.GetResponseStream(), Encoding.UTF8);
    //            var docStockContext = new HtmlDocument();
    //            docStockContext.LoadHtml(
    //                doc.DocumentNode.SelectSingleNode("/html[1]/head[1]/body[1]/div[2]/div[2]").InnerHtml);
    //            HtmlNodeCollection nodeHeaders = docStockContext.DocumentNode.ChildNodes;
    //            try
    //            {
    //                foreach (HtmlNode node in nodeHeaders)
    //                {
    //                    string beatmap = node.SelectSingleNode("./a[1]").InnerText;
    //                    string id = node.SelectSingleNode("./div[1]/a[1]").InnerText;
    //                    resultDictionary.Add(id, beatmap);
    //                }
    //            }
    //            catch (Exception)
    //            {
    //            }
    //            return resultDictionary;
    //        }
    //        catch (Exception)
    //        {
    //        }
    //        return resultDictionary;
    //    }
    //}

}
