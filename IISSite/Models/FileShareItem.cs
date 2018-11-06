using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IISSite.Models
{
    public class FileShareItem
    {
        public FileShareItem()
        {

        }

        // Create the element. 
        public FileShareItem(string displayName, string actualPath)
        {
            this.DisplayName = displayName;
            this.ActualPath = actualPath;
        }

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