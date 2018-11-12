using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.IO;
using System.Security.Principal;
using System.Text;
using System.Configuration;
using System.Security.Claims;
using System.Globalization;
using System.Data.SqlClient;
using System.Data;
using IISSite.Helpers;
using IISSite.Models;
using System.DirectoryServices;
using System.Web.Helpers;
using Microsoft.SqlServer.Server;

namespace IISSite
{
    public partial class Default : System.Web.UI.Page
    {
       // private string fileShareRootPath;

        //string fileShareRootPath = string.Empty;
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
                //ShowData.Attributes["class"] = "btn btn-link collapsed";
                ToggleError();
                BindUserData();
                BindClaimsTab();
                BindGroupsTab();
                BindSqlTab();
                BindLdapTab();
                BindMsmqTab();
                BindFileshareTab();
            }
        }

        private void BindSqlTab()
        {
            //If there is a SQL connection string configured, set it in the UI
            string connString = ConfigurationManager.ConnectionStrings["RemoteSqlServer"]?.ToString();
            if (!string.IsNullOrEmpty(connString))
            {
                try
                {
                    SqlConnectionStringBuilder sqlConnectionString = new SqlConnectionStringBuilder(connString);
                    dbServerName.Text = sqlConnectionString.DataSource;
                    dbDatabaseName.Text = sqlConnectionString.InitialCatalog;
                    dbConnString.Text = sqlConnectionString.ToString();
                }
                catch
                {
                }
            }
        }

        private void BindLdapTab()
        {
            //Prefill with the current domain
            ADUser currentUser = LdapHelper.GetAdUser(Request.LogonUserIdentity.Name);
            if (currentUser != null)
            {
                ldapDomainname.Text = currentUser.ParentForest.Name;
            }
        }

        private void BindMsmqTab()
        {
            
        }

        private void BindFileshareTab()
        {

        }

        private void BindUserData()
        {
            ADUser currentUser = LdapHelper.GetAdUser(Request.LogonUserIdentity.Name);
            if (currentUser != null)
            {
                loginName.Text = currentUser.DisplayName;
            }
        }

        private void ToggleError(string p, Label errorControl)
        {
            if (errorControl != null)
            {
                errorControl.Text = p;
                errorControl.Visible = true;
            }
        }

        private void ToggleError()
        {
            errLabel.Text = string.Empty;
            errLabel.Visible = false;
        }
        private void BindClaimsTab()
        {
            try
            {
                ClaimsIdentity claimsIdentity = Request.LogonUserIdentity;

                string networkClaimType = "http://schemas.microsoft.com/ws/2012/01/insidecorporatenetwork";
                string userLocationLabel = "{0} the corporate network";
                string userLocation = "UNKNOWN";
                Claim sourceClaim = claimsIdentity.Claims.FirstOrDefault(c => c.Type.ToString(CultureInfo.InvariantCulture) == networkClaimType);
                if (sourceClaim != null)
                {
                    if (sourceClaim.Value.ToUpper() == "TRUE")
                    {
                        userLocation = "INSIDE";
                    }
                    else
                    {
                        userLocation = "OUTSIDE";
                    }
                }
                else
                {
                    //Could not determine the network type
                    userLocationLabel = "{0} network";
                }
                UserSourceLocation.Text = string.Format(userLocationLabel, userLocation);
                claimsGrid.DataSource = claimsIdentity.Claims;
                claimsGrid.AutoGenerateColumns = false;
                claimsGrid.DataBind();
            }
            catch (Exception ex)
            {
                ToggleError($"BindClaimsTab::Error{ex.ToString()}", claimsTabError);
            }
        }

        private void BindGroupsTab()
        {
            List<ADGroup> groups = new List<ADGroup>();
            try
            {
                groups = LdapHelper.GetADGroups(Request.LogonUserIdentity.Groups);
                userGroupsGrid.DataSource = groups;
                userGroupsGrid.DataBind();
            }
            catch (Exception gex)
            {
                ToggleError($"BindGroupsTab::{gex.ToString()}", winPrincipalTabError);
            }
        }

        protected void GetSQLData_Click(object sender, EventArgs e)
        {
            // Open the connection.
            ToggleError("", databaseTabError);
            //string sqlQuery = "SELECT TOP 1000 [Id],[ParentCategoryId],[Path],[Name],[NumActiveAds] FROM [Classifieds].[dbo].[Categories]";
            string sqlQuery = dbQuery.Text;

            try
            {
                SqlConnectionStringBuilder sqlConnectionString = new SqlConnectionStringBuilder
                {
                    IntegratedSecurity = true,
                    DataSource = dbServerName.Text,
                    InitialCatalog = dbDatabaseName.Text,
                    //Authentication = SqlAuthenticationMethod.ActiveDirectoryIntegrated,
                    TrustServerCertificate = true
                };
                DataSet ds = new DataSet();
                SqlConnection dbConnection = null;
                SqlDataAdapter dataadapter = null;
                dbConnection = new SqlConnection(sqlConnectionString.ToString());
                dataadapter = new SqlDataAdapter(sqlQuery, dbConnection);

                if (backendCallType.SelectedValue == "asuser")
                {
                    //Impersonate the current user to get the files to browse
                    ADUser currentUser = LdapHelper.GetAdUser(Request.LogonUserIdentity.Name);
                    if (currentUser != null)
                    {
                        WindowsIdentity wi = new WindowsIdentity(currentUser.UserPrincipalName);
                        using (WindowsImpersonationContext wCtx = wi.Impersonate())
                        {
                            // Run the SQL statement, and then get the returned rows to the DataReader.
                            dbConnection.Open();
                            wCtx.Undo();
                        }
                    }
                    else
                    {
                        ToggleError("GetSQLData_Click::Could not bind AD User", databaseTabError);
                    }
                }
                else
                {
                    // Run the SQL statement, and then get the returned rows to the DataReader.
                    dbConnection.Open();
                }

                dataadapter.Fill(ds, "data");
                dbConnection.Close();
                sqlDataGrid.DataSource = ds.Tables["data"].DefaultView;
                sqlDataGrid.DataBind();
                sqlResults.Visible = true;
            }
            catch (Exception sqlex)
            {
                ToggleError($"GetSQLData_Click::{sqlex.ToString()}", databaseTabError);
                sqlResults.Visible = false;
            }
        }

        protected void FileShareBind_Click(object sender, EventArgs e)
        {
            //ClaimsIdentity currentClaims = Request.LogonUserIdentity;
            //Claim upnClaim = currentClaims.Claims.FirstOrDefault(c => c.Type.ToString(CultureInfo.InvariantCulture) == ClaimTypes.Upn);
            //if (upnClaim != null)
            //{ 
            try
            {
                string serverName = fileShareServerName.Text;
                string fileFolderName = fileShareName.Text;
                string fileShareRootPath = $@"\\{serverName}\{fileFolderName}";
                FileShareItem rootFileShareItem = new FileShareItem()
                {
                    ParentFileShare = serverName,
                    ActualPath = fileShareRootPath,
                    DisplayName = fileFolderName

                };

                BindFolders(rootFileShareItem);

                BindFiles(rootFileShareItem);
            }
            catch(Exception ex)
            {
                ToggleError($"FileShareBind_Click::Error::{ex.ToString()}", fileshareTabError);
            }
            //}
            //else
            //{
            //    toggleError("No UPN");
            //}
        }

        private void BindFolders(FileShareItem fileShareItem)
        {
            string fileShareRootPath = fileShareItem.ActualPath;

            //string fileShareVirtualDirectory = string.Empty;
            //string currentPath = @"\";
            //string decodedVal = string.Empty;
            //string fileShareConfigId = string.Empty;
            DirectoryInfo currentDirectory = null;
            DirectoryInfo[] curFolders = null;
            DirectoryInfo currentParent = null;

            try
            {
                //Impersonate the current user to get the files to browse
                if (backendCallType.SelectedValue == "asuser")
                {
                    ADUser currentUser = LdapHelper.GetAdUser(Request.LogonUserIdentity.Name);
                    WindowsIdentity wi = new WindowsIdentity(currentUser.UserPrincipalName);
                    using (WindowsImpersonationContext wCtx = wi.Impersonate())
                    {
                        currentDirectory = new DirectoryInfo(fileShareItem.ActualPath);
                        if (System.IO.Directory.Exists(fileShareRootPath))
                        {
                            curFolders = currentDirectory.GetDirectories();
                            //if there are no children and we are not at the root, show parents
                            if (currentDirectory.Parent != null)
                            {
                                if (currentDirectory.Parent.FullName == fileShareRootPath)
                                {
                                    //We are at the root
                                    currentParent = currentDirectory;
                                }
                            }
                            else
                            {
                                //We are at the root
                                currentParent = currentDirectory;
                            }
                        }
                        wCtx.Undo();
                    }
                }
                else
                {
                    currentDirectory = new DirectoryInfo(fileShareItem.ActualPath);
                    if (System.IO.Directory.Exists(fileShareRootPath))
                    {
                        curFolders = currentDirectory.GetDirectories();
                        //if there are no children and we are not at the root, show parents
                        if (currentDirectory.Parent != null)
                        {
                            if (currentDirectory.Parent.FullName == fileShareRootPath)
                            {
                                //We are at the root
                                currentParent = currentDirectory;
                            }
                        }
                        else
                        {
                            //We are at the root
                            currentParent = currentDirectory;
                        }
                    }
                }


                directoryList.DataSource = curFolders;
                directoryList.DataBind();

                //Build the breadcrumb for the page
                BuildBreadcrumb(fileShareRootPath, fileShareRootPath);
            }
            catch (System.Security.SecurityException)
            {
                ToggleError("BindFolders::Error SecurityException", fileshareTabError);
            }
            catch (System.UnauthorizedAccessException)
            {
                ToggleError("BindFolders::Error Unauthorized", fileshareTabError);
            }
            catch (Exception ex)
            {
                ToggleError($"BindFolders::Error::{ex.ToString()}", fileshareTabError);
            }
}
        private void BindFiles(FileShareItem fileShareItem)
        {
            string fileShareRootPath = fileShareItem.ActualPath;

            DirectoryInfo currentDirectory = null;
            FileInfo[] curDirFiles = null;

            //if (userUpn != null)
            //{
            try
            {
                //Impersonate the current user to get the files to browse
                //WindowsIdentity wi = new WindowsIdentity(userUpn.Value);
                //using (WindowsImpersonationContext wCtx = wi.Impersonate())
                //{
                currentDirectory = new DirectoryInfo(fileShareItem.ActualPath);
                if (System.IO.Directory.Exists(fileShareRootPath))
                {
                    curDirFiles = currentDirectory.GetFiles();

                    fileList.DataSource = curDirFiles;
                    fileList.DataBind();
                }
                else
                {
                    //The configured directory does not exist or is invalid
                    //}
                }
            }
            catch (System.Security.SecurityException)
            {
                ToggleError("BindFiles::Error SecurityException", fileshareTabError);
            }
            catch (System.UnauthorizedAccessException)
            {
                ToggleError("BindFiles::Error Unauthorized", fileshareTabError);
            }
            catch (Exception ex)
            {
                ToggleError($"BindFiles::Error::{ex.ToString()}", fileshareTabError);
            }
            //}
        }

        public void BuildBreadcrumb(string relativeFileSharePath, string RootFileShare)
        {
            //Remove the current root from the path and trim the extra backslashes
            string trimmedPath = relativeFileSharePath.Replace(RootFileShare, "").TrimStart('\\').TrimEnd('\\');
            string[] crumbPath = trimmedPath.Split('\\');
            //HyperLink breadCrumbSepChar = new HyperLink() { NavigateUrl = "#", Text = "/" };
            List<HyperLink> links = new List<HyperLink>();
            HyperLink link = new HyperLink
            {
                Text = RootFileShare,
                NavigateUrl = UrlUtility.BuildShareUri(RootFileShare).ToString()
            };
            links.Add(link);
            Label sepChar;
            string curPathNode = string.Empty;
            StringBuilder curPath = new StringBuilder();
            string currentNode = string.Empty;

            curPath.Append(@"\");
            for (int i = 0; i < crumbPath.Length; i++)
            {
                currentNode = crumbPath[i];

                if (i + 1 != crumbPath.Length)
                {
                    sepChar = new Label();
                    HyperLink breadCrumbSepChar = new HyperLink() { NavigateUrl = "#", Text = "/" };
                    links.Add(breadCrumbSepChar);
                    link = new HyperLink
                    {
                        Text = currentNode
                    };
                    curPath.Append(currentNode);
                    curPath.Append(@"\");
                    link.NavigateUrl = UrlUtility.BuildFolderUri(curPath.ToString(), RootFileShare).ToString();
                    links.Add(link);
                }
                curPathNode = currentNode;
            }


            //If we are at the root, the trimmed path will be empty, dont add a >
            if (!string.IsNullOrEmpty(trimmedPath))
            {
                //Add the last node
                sepChar = new Label();
                HyperLink breadCrumbSepChar = new HyperLink() { NavigateUrl = "#", Text = "/" };
                links.Add(breadCrumbSepChar);
                HyperLink lastNode = new HyperLink() {
                   Text = curPathNode,
                   NavigateUrl = "#"
                };
                links.Add(lastNode);
            }
            
            breadcrumbLinks.DataSource = links;
            breadcrumbLinks.DataBind();
        }

        protected void LdapBind_Click(object sender, EventArgs e)
        {
            string fqdnToSearch = ldapDomainname.Text;
            string searchString = Request.LogonUserIdentity.Name;

            try
            {
                DirectoryEntry rootEntry = new DirectoryEntry($@"LDAP://{fqdnToSearch}");
                rootEntry.AuthenticationType = System.DirectoryServices.AuthenticationTypes.Secure;
                SearchResultCollection searchResults = null;
                DirectorySearcher searcher = new DirectorySearcher(rootEntry);
                searcher.SearchScope = SearchScope.Subtree;
                searcher.SizeLimit = 50;

                switch (ldapUsersOrGroups.SelectedValue)
                {
                    case "users":
                        {
                            searcher.Filter = ("(&(objectCategory=person))");
                            break;
                        }
                    case "groups":
                        {
                            searcher.Filter = ("(&(objectCategory=group))");
                            break;
                        }
                }

                //TODO: If there are no results, toggle an error
                searchResults = searcher.FindAll();
                ldapDataGrid.DataSource = searchResults;
                ldapDataGrid.DataBind();

                //var queryFormat = "(&(objectClass=user)(objectCategory=person)(|(SAMAccountName=*{0}*)(cn=*{0}*)(gn=*{0}*)(sn=*{0}*)(email=*{0}*)))";
                //searcher.Filter = string.Format(queryFormat, searchString);
                //var searchResults = searcher.FindAll();
                ////TODO: If there are no results, toggle an error
                //ldapDataGrid.DataSource = searchResults;
                //ldapDataGrid.DataBind();
            }
            catch (Exception ex)
            {
                ToggleError(ex.ToString(), ldapTabError);
            }
        }

        protected void FileList_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            RepeaterItem parentItem = e.Item;
            FileInfo currentFile;
            if (parentItem.ItemType == ListItemType.Item || parentItem.ItemType == ListItemType.AlternatingItem)
            {
                currentFile = parentItem.DataItem as FileInfo;
                HyperLink fileName = parentItem.FindControl("fileName") as HyperLink;
                Label size = parentItem.FindControl("fileSize") as Label;
                Label ext = parentItem.FindControl("fileExt") as Label;
                Label attributes = parentItem.FindControl("fileAttributes") as Label;
                Label created = parentItem.FindControl("fileCreated") as Label;
                Label lastModifed = parentItem.FindControl("fileLastModified") as Label;
                //HtmlButton shareLink = parentItem.FindControl("shareLink") as HtmlButton;
                string fileDisplayName;
                string filePath;
                string fileUrl;
                //Make the file name relative
                if (currentFile != null)
                {
                    fileDisplayName = currentFile.Name;
                    filePath = currentFile.DirectoryName;
                    //fileUrl = currentFile.FullName;
                    fileName.Text = fileDisplayName;
                    fileName.ToolTip = filePath;
                    fileUrl = Server.UrlPathEncode(UrlUtility.BuildFileUri(currentFile.FullName, currentFile.DirectoryName).ToString());
                    fileName.NavigateUrl = fileUrl;
                    size.Text = FileShareHelper.DisplayFileSizeFromBytes(currentFile.Length);
                    ext.Text = currentFile.Extension;
                    attributes.Text = currentFile.Attributes.ToString();
                    created.Text = currentFile.CreationTime.ToString();
                    lastModifed.Text = currentFile.LastWriteTime.ToString();
                    //shareLink.Attributes.Add("href", "sharelink.aspx?u=" + fileUrl);
                }
            }
        }

        protected void DirectoryList_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            RepeaterItem parentItem = e.Item;
            DirectoryInfo currentFolder;
            if (parentItem.ItemType == ListItemType.Item || parentItem.ItemType == ListItemType.AlternatingItem)
            {
                currentFolder = parentItem.DataItem as DirectoryInfo;
                LinkButton folderName = parentItem.FindControl("folderName") as LinkButton;
                Label count = parentItem.FindControl("fileCount") as Label;
                Label created = parentItem.FindControl("folderCreated") as Label;
                Label lastModifed = parentItem.FindControl("folderLastModified") as Label;

                //Make the file name relative
                if (currentFolder != null)
                {
                    //Make the URL relative
                    //see if we have access to the sub folders. if not, do something
                    try
                    {
                        count.Text = currentFolder.EnumerateFileSystemInfos().Count().ToString();
                        folderName.Text = string.Format("<span class='glyphicon {0}'></span>&nbsp;<span class=''>{1}</span>", "glyphicon-ok-circle", currentFolder.Name);
                        FileShareItem fs = new FileShareItem()
                        {
                            ActualPath = currentFolder.FullName,
                            DisplayName = currentFolder.Name,
                            ParentFileShare = currentFolder.Root.Name
                        };

                        if (currentFolder.GetDirectories().Length > 0)
                        {
                            //Folder has sub folders, enumerate those
                        }
                        folderName.CommandArgument = Json.Encode(fs);
                        folderName.CommandName = "listfiles";
                        created.Text = currentFolder.CreationTime.ToString();
                        lastModifed.Text = currentFolder.LastWriteTime.ToString();
                    }
                    catch
                    {
                        //User does not have access to the file share.
                        folderName.Text = string.Format("<span class='glyphicon {0}'></span>&nbsp;<span class=''>{1}</span>", "glyphicon-ban-circle", currentFolder.Name);
                    }
                }
            }
        }

        protected void FileShareList_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            HyperLink curLink = null;
            FileShareItem curFileShare = null;
            if (e.Item.ItemType.Equals(ListItemType.Item) || e.Item.ItemType.Equals(ListItemType.AlternatingItem))
            {
                curFileShare = (FileShareItem)e.Item.DataItem;
                curLink = e.Item.FindControl("fileShareUrl") as HyperLink;
                curLink.NavigateUrl = UrlUtility.BuildShareUri(curFileShare.DisplayName).ToString();
                curLink.Text = curFileShare.DisplayName;
            }
        }

        protected void DirectoryList_ItemCommand(object sender, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "listfiles")
            {
                // Get the value of command argument
                var value = e.CommandArgument;
                // Do whatever operation you want.  
                FileShareItem fs = Json.Decode<FileShareItem>(e.CommandArgument.ToString());
                BindFiles(fs);
            }
        }

        protected void CreateRandomFile_Click(object sender, EventArgs e)
        {
            //setFileShareConfigContext();
            //if (curFileShareConfig != null)
            //{
            //    //TODO: Validate this URL
            //    //fileShareRootPath = curFileShareConfig.ActualPath;
            //    fullFileSharePath = fileShareRootPath + currentPath;

            //    //Impersonate the current user to get the files to browse
            //    if (!String.IsNullOrEmpty(fullFileSharePath))
            //    {
            //        currentDirectory = new DirectoryInfo(fullFileSharePath);
            //        if (System.IO.Directory.Exists(fullFileSharePath))
            //        {
            //            ClaimsPrincipal currentClaims = ClaimsPrincipal.Current;
            //            Claim userUpn = currentClaims.Claims.FirstOrDefault(c => c.Type.ToString(CultureInfo.InvariantCulture) == ClaimTypes.Upn);

            //            if (userUpn != null)
            //            {
            //                WindowsIdentity wi = new WindowsIdentity(userUpn.Value);
            //                using (WindowsImpersonationContext wCtx = wi.Impersonate())
            //                {
            //                    try
            //                    {
            //                        System.IO.File.CreateText(currentDirectory.FullName + "\\" + Path.GetRandomFileName());
            //                        wCtx.Undo();
            //                    }
            //                    catch (System.UnauthorizedAccessException)
            //                    {
            //                        //Do something with this error
            //                    }
            //                }
            //                //Rebind the UI
            //                bindFiles(userUpn);
            //            }
            //        }
            //    }
        }
    }
}