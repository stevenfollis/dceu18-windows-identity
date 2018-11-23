using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.DirectoryServices;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Messaging;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Web.Helpers;
using System.Web.UI;
using System.Web.UI.WebControls;
using IISSite.Common.Helpers;
using IISSite.Common.Models;

namespace IISSite
{
    public partial class Default : System.Web.UI.Page
    {
        public string FileShareRootPath { get; set; }

        //string fileShareVirtualDirectory = string.Empty;
        //string currentPath = @"\";
        //string decodedVal = string.Empty;
        //string fileShareConfigId = string.Empty;
        //DirectoryInfo currentDirectory = null;
        //DirectoryInfo[] curFolders = null;
        //FileInfo[] curDirFiles = null;
        //DirectoryInfo currentParent = null;
        //string FileSharePath = string.Empty;

        protected override void OnLoad(EventArgs e)
        {
            if (!Page.IsPostBack)
            {
  
            }
        }

        
    }
}