using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IISSite.Models
{
    public enum FileShareItemType
    {
        File = 0,
        Folder=1
    }

    public class FileShareItem
    {
        public FileShareItem()
        {

        }

        public FileShareItemType ItemType { get; set; }
        // Create the element. 
        public FileShareItem(string displayName, string actualPath)
        {
            this.DisplayName = displayName;
            this.ActualPath = actualPath;
        }

        public long SizeInGB { get; set; }

        public String DisplayName
        {
            get; set;
        }

        public String ActualPath
        {
            get; set;
        }

        public String ParentFileShare
        {
            get; set;
        }
    }
}