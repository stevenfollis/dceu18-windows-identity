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
            string relativePath = fullFilePath.Replace(rootFileShare, "/FS/" + rootFileShare);
            relativePath = relativePath.Replace(@"\", "/");

            //Make the URL relative
            UriBuilder newFileUri = new UriBuilder();
            newFileUri.Scheme = HttpContext.Current.Request.Url.Scheme;
            newFileUri.Host = HttpContext.Current.Request.Url.Host;
            newFileUri.Port = HttpContext.Current.Request.Url.Port;
            newFileUri.Path = relativePath;
            return newFileUri.Uri;
        }

        public static Uri BuildFolderUri(string fullFolderName, string rootFileShare)
        {
            string relativePath = fullFolderName.Replace(rootFileShare, "");
            UriBuilder folderUri = new UriBuilder();
            folderUri.Scheme = HttpContext.Current.Request.Url.Scheme;
            folderUri.Host = HttpContext.Current.Request.Url.Host;
            folderUri.Port = HttpContext.Current.Request.Url.Port;
            folderUri.Path = HttpContext.Current.Request.Url.AbsolutePath;
            string folderQString = string.Format("{0}&{1}={2}&{3}={4}", getCleanQueryString(), QSTRINGSOURCE, HttpContext.Current.Server.UrlEncode(rootFileShare), QSTRINGPATH, HttpContext.Current.Server.UrlEncode(relativePath));
            folderUri.Query = folderQString;
            return folderUri.Uri;
        }

        public static Uri BuildShareUri(string ShareDisplayName)
        {
            //Make the URL relative
            UriBuilder fileShareUri = new UriBuilder();
            fileShareUri.Scheme = HttpContext.Current.Request.Url.Scheme;
            fileShareUri.Host = HttpContext.Current.Request.Url.Host;
            fileShareUri.Port = HttpContext.Current.Request.Url.Port;
            fileShareUri.Path = HttpContext.Current.Request.Url.AbsolutePath;
            string fileQString = string.Format("{0}&{1}={2}", getCleanQueryString(), QSTRINGSOURCE, HttpContext.Current.Server.UrlEncode(ShareDisplayName));
            fileShareUri.Query = fileQString;
            return fileShareUri.Uri;
        }

        //Removes the known query strings from the current url
        static string getCleanQueryString()
        {
            string retQString = string.Empty;
            NameValueCollection curQString = HttpContext.Current.Request.QueryString;
            StringBuilder cleanString = new StringBuilder();

            foreach (string parmKey in curQString)
            {
                if (!knownQstrings().Contains(parmKey))
                {
                    cleanString.Append(parmKey);
                    cleanString.Append("=");
                    cleanString.Append(curQString[parmKey]);
                    cleanString.Append("&");
                }
            }
            return cleanString.ToString().TrimEnd('&');
        }

        static StringCollection knownQstrings()
        {
            StringCollection knownKeys = new StringCollection();
            knownKeys.Add(QSTRINGPATH);
            knownKeys.Add(QSTRINGSOURCE);
            return knownKeys;
        }
    }
}