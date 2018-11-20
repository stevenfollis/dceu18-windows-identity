using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Web;

namespace IISSite.Helpers
{
    public static class UrlUtility
    {
        public static string QSTRINGPATH = "p"; //The path to the file
        public static string QSTRINGSOURCE = "s"; //The source id mapped to the web.config of the file share

        public static Uri BuildFileUri(string fullFilePath, string rootFileShare)
        {
            //string relativePath = fullFilePath.Replace(rootFileShare, vdirPath);
            string relativePath = $"/FS/{fullFilePath}";
            relativePath = relativePath.Replace(@"\", "/");

            //Make the URL relative
            UriBuilder newFileUri = new UriBuilder
            {
                Scheme = HttpContext.Current.Request.Url.Scheme,
                Host = HttpContext.Current.Request.Url.Host,
                Port = HttpContext.Current.Request.Url.Port,
                Path = HttpContext.Current.Request.ApplicationPath + relativePath
            };
            return newFileUri.Uri;
        }

        public static Uri BuildFolderUri(string rootFileShare)
        {
            //string relativePath = fullFolderName.Replace(rootFileShare, "");
            UriBuilder folderUri = new UriBuilder
            {
                Scheme = HttpContext.Current.Request.Url.Scheme,
                Host = HttpContext.Current.Request.Url.Host,
                Port = HttpContext.Current.Request.Url.Port,
                Path = HttpContext.Current.Request.Url.AbsolutePath
            };
            string folderQString = $"{GetCleanQueryString()}&{QSTRINGPATH}={HttpContext.Current.Server.UrlEncode(rootFileShare)}";
            folderUri.Query = folderQString;
            return folderUri.Uri;
        }

        public static Uri BuildShareUri(string rootFileShare, string ShareDisplayName)
        {
            //Make the URL relative
            UriBuilder fileShareUri = new UriBuilder
            {
                Scheme = HttpContext.Current.Request.Url.Scheme,
                Host = HttpContext.Current.Request.Url.Host,
                Port = HttpContext.Current.Request.Url.Port,
                Path = HttpContext.Current.Request.ApplicationPath + HttpContext.Current.Request.Url.AbsolutePath
            };
            string fileQString = $"{GetCleanQueryString()}&{QSTRINGPATH}={HttpContext.Current.Server.UrlEncode(ShareDisplayName)}";
            fileShareUri.Query = fileQString;
            return fileShareUri.Uri;
        }

        //Removes the known query strings from the current url
        static string GetCleanQueryString()
        {
            string retQString = string.Empty;
            NameValueCollection curQString = HttpContext.Current.Request.QueryString;
            StringBuilder cleanString = new StringBuilder();

            foreach (string parmKey in curQString)
            {
                if (!KnownQstrings().Contains(parmKey))
                {
                    cleanString.Append(parmKey);
                    cleanString.Append("=");
                    cleanString.Append(curQString[parmKey]);
                    cleanString.Append("&");
                }
            }
            return cleanString.ToString().TrimEnd('&');
        }

        static StringCollection KnownQstrings()
        {
            StringCollection knownKeys = new StringCollection
            {
                QSTRINGPATH,
                QSTRINGSOURCE
            };
            return knownKeys;
        }
    }
}