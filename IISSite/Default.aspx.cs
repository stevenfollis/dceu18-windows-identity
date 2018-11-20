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
using System.Messaging;

namespace IISSite
{
    public partial class Default : System.Web.UI.Page
    {
        public string fileShareRootPath { get; set; }

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

        private void ToggleMessage(string msg, Label messageControl, bool isError = false, bool display = false)
        {
            if (messageControl != null)
            {
                messageControl.Text = msg;
                messageControl.Visible = display;
            }
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
                ToggleMessage($"BindClaimsTab::Error{ex.ToString()}", claimsTabError);
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
                ToggleMessage($"BindGroupsTab::{gex.ToString()}", winPrincipalTabError);
            }
        }

        protected void GetSQLData_Click(object sender, EventArgs e)
        {
            // Open the connection.
            ToggleMessage("", databaseTabError);
            dbMessages.Items.Clear();
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

                dbMessages.Items.Add($"Connecting to [{sqlConnectionString.ToString()}] {backendCallType.SelectedValue}");

                DataSet ds = new DataSet();
                DataTable dt = new DataTable();
                SqlConnection dbConnection = null;
                SqlDataAdapter dataadapter = null;

                if (backendCallType.SelectedValue == "asuser")
                {
                    //Impersonate the current user to get the files to browse
                    ADUser currentUser = LdapHelper.GetAdUser(Request.LogonUserIdentity.Name);
                    if (currentUser != null)
                    {
                        dbMessages.Items.Add($"Connecting as {currentUser.UserPrincipalName}");
                        //WindowsIdentity wi = new WindowsIdentity(currentUser.UserPrincipalName);
                        //using (WindowsImpersonationContext wCtx = wi.Impersonate())
                        //{
                        //    // Run the SQL statement, and then get the returned rows to the DataReader.
                        //    dbConnection.Open();
                        //    wCtx.Undo();
                        //}
                        using (WindowsImpersonationContext wi = ImpersonateEndUser(currentUser.UserPrincipalName))
                        {
                            try
                            {
                                dbMessages.Items.Add("Impersonating");
                                dbConnection = new SqlConnection(sqlConnectionString.ToString());
                                dbMessages.Items.Add("Opening DB");
                                dbConnection.Open();
                                dataadapter = new SqlDataAdapter(sqlQuery, dbConnection);
                                dbMessages.Items.Add("Running query");
                                dataadapter.Fill(ds, "data");
                                dbMessages.Items.Add("Disabling impersonation");
                            }
                            catch (Exception ex)
                            {
                                dbMessages.Items.Add("Error");
                                ToggleMessage($"GetSQLData_Click::Could not get data {ex.ToString()}", databaseTabError, true);
                            }
                            finally
                            {
                                wi.Undo();
                            }
                        }
                    }
                    else
                    {
                        dbMessages.Items.Add($"ERROR: {Request.LogonUserIdentity.Name} not found. Check UPN");
                        ToggleMessage("GetSQLData_Click::Could not bind AD User", databaseTabError);
                    }
                }
                else
                {
                    // Run the SQL statement, and then get the returned rows to the DataReader.
                    dbConnection = new SqlConnection(sqlConnectionString.ToString());
                    dbMessages.Items.Add("Opening DB");
                    dbConnection.Open();
                    dataadapter = new SqlDataAdapter(sqlQuery, dbConnection);
                    dbMessages.Items.Add("Fetching data");
                    dataadapter.Fill(ds, "data");
                }

                dbConnection.Close();
                sqlDataGrid.DataSource = ds.Tables["data"].DefaultView;
                sqlDataGrid.DataBind();
                sqlResults.Visible = true;
            }
            catch (Exception sqlex)
            {
                ToggleMessage($"GetSQLData_Click::{sqlex.ToString()}", databaseTabError);
                sqlResults.Visible = false;
            }
        }

        private WindowsImpersonationContext ImpersonateEndUser(string userUpn)
        {
            // Obtain the user name (from forms authentication)
            //string identity = User.Identity.Name;

            // Convert from domainName\userName format to userName@domainName format
            // if necessary
            //int slash = identity.IndexOf("\\");
            //if (slash > 0)
            //{
            //    string domain = identity.Substring(0, slash);
            //    string user = identity.Substring(slash + 1);
            //    identity = user + "@" + domain;
            //}

            // The WindowsIdentity(string) constructor uses the new
            // Kerberos S4U extension to get a logon for the user
            // without a password.
            WindowsIdentity wi = new WindowsIdentity(userUpn);
            return wi.Impersonate();
        }

        protected void FileShareBind_Click(object sender, EventArgs e)
        {
            fileShareMsgs.Items.Clear();

            try
            {
                string serverName = fileShareServerName.Text;
                string fileFolderName = fileShareName.Text;
                fileShareRootPath = $@"\\{serverName}\{fileFolderName}";
                fileShareMsgs.Items.Add($"Fetching files from {fileShareRootPath}");

                FileShareItem rootFileShareItem = new FileShareItem()
                {
                    ParentFileShare = fileShareRootPath,
                    ActualPath = fileShareRootPath,
                    DisplayName = fileFolderName
                };

                fileShareMsgs.Items.Add("Binding folders");
                BindFolders(rootFileShareItem);

                fileShareMsgs.Items.Add("Binding files");
                BindFiles(rootFileShareItem);
            }
            catch(Exception ex)
            {
                ToggleMessage($"FileShareBind_Click::Error::{ex.ToString()}", fileshareTabError);
            }
        }

        private void BindFolders(FileShareItem fileShareItem)
        {
            string currentPath = fileShareItem.ActualPath;
            DirectoryInfo currentDirectory = null;
            DirectoryInfo[] curFolders = null;
            DirectoryInfo currentParent = null;

            fileShareMsgs.Items.Add($"BindFolders::{currentPath}");
            try
            {
                //Impersonate the current user to get the files to browse
                if (backendCallType.SelectedValue == "asuser")
                {
                    ADUser currentUser = LdapHelper.GetAdUser(Request.LogonUserIdentity.Name);
                    if (currentUser != null)
                    {
                        fileShareMsgs.Items.Add($"BindFolders::Impersonating user {currentUser.UserPrincipalName}");
                        WindowsIdentity wi = new WindowsIdentity(currentUser.UserPrincipalName);

                        using (WindowsImpersonationContext wCtx = wi.Impersonate())
                        {
                            try
                            {
                                currentDirectory = new DirectoryInfo(fileShareItem.ActualPath);
                                fileShareMsgs.Items.Add($"Current directory is {fileShareItem.ActualPath}");
                                if (System.IO.Directory.Exists(currentPath))
                                {
                                    fileShareMsgs.Items.Add("Path exists, get directories");
                                    curFolders = currentDirectory.GetDirectories();
                                    //if there are no children and we are not at the root, show parents
                                    if (currentDirectory.Parent != null)
                                    {
                                        if (currentDirectory.Parent.FullName == currentPath)
                                        {
                                            //We are at the root
                                            currentParent = currentDirectory;
                                            fileShareMsgs.Items.Add($"Current is root {currentParent}");
                                        }
                                    }
                                    else
                                    {
                                        //We are at the root
                                        currentParent = currentDirectory;
                                        fileShareMsgs.Items.Add($"At the root. Set the parent to {currentParent}");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                fileShareMsgs.Items.Add("Error");
                            }
                            finally
                            {
                                fileShareMsgs.Items.Add("Undoing impersonation");
                                wCtx.Undo();
                            }
                        }
                    }
                }
                else
                {
                    currentDirectory = new DirectoryInfo(fileShareItem.ActualPath);
                    fileShareMsgs.Items.Add($"Looking for directory {currentDirectory}");
                    if (System.IO.Directory.Exists(currentPath))
                    {
                        fileShareMsgs.Items.Add("Path exists, get sub directories");
                        curFolders = currentDirectory.GetDirectories();
                        //if there are no children and we are not at the root, show parents
                        if (currentDirectory.Parent != null)
                        {
                            if (currentDirectory.Parent.FullName == currentPath)
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

                fileShareMsgs.Items.Add($"Bind folders {curFolders?.Length.ToString()}");
                directoryList.DataSource = curFolders;
                directoryList.DataBind();

                fileShareMsgs.Items.Add($"Bind files with {currentPath}");
                BindFiles(fileShareItem);

                //Build the breadcrumb for the page
                fileShareMsgs.Items.Add($"Build breadcrumb with {currentPath}");
                BuildBreadcrumb(fileShareItem.ParentFileShare, currentPath);
            }
            catch (System.Security.SecurityException)
            {
                fileShareMsgs.Items.Add("ERROR Security exception");
                ToggleMessage("BindFolders::Error SecurityException", fileshareTabError);
            }
            catch (System.UnauthorizedAccessException)
            {
                fileShareMsgs.Items.Add("ERROR Unauthorized exception");
                ToggleMessage("BindFolders::Error Unauthorized", fileshareTabError);
            }
            catch (Exception ex)
            {
                fileShareMsgs.Items.Add("ERROR");
                ToggleMessage($"BindFolders::Error::{ex.ToString()}", fileshareTabError);
            }
}

        private void BindFiles(FileShareItem fileShareItem)
        {
            //string fileShareRootPath = fileShareItem.ActualPath;

            DirectoryInfo currentDirectory = null;
            FileInfo[] curDirFiles = null;

            try
            {
                //Impersonate the current user to get the files to browse
                if (backendCallType.SelectedValue == "asuser")
                {
                    ADUser currentUser = LdapHelper.GetAdUser(Request.LogonUserIdentity.Name);
                    if (currentUser != null)
                    {
                        fileShareMsgs.Items.Add($"BindFiles::Impersonating user {currentUser.UserPrincipalName}");
                        WindowsIdentity wi = new WindowsIdentity(currentUser.UserPrincipalName);
                        using (WindowsImpersonationContext wCtx = wi.Impersonate())
                        {
                            try
                            {
                                currentDirectory = new DirectoryInfo(fileShareItem.ActualPath);
                                fileShareMsgs.Items.Add($"BindFiles::Checking for files in directory {currentDirectory}");
                                if (System.IO.Directory.Exists(fileShareItem.ActualPath))
                                {
                                    fileShareMsgs.Items.Add("BindFiles::Getting files");
                                    curDirFiles = currentDirectory.GetFiles();
                                    fileShareMsgs.Items.Add($"BindFiles::Found {curDirFiles?.Length.ToString()} files");
                                    fileList.DataSource = curDirFiles;
                                    fileList.DataBind();
                                }
                                else
                                {
                                    fileShareMsgs.Items.Add("BindFiles::directory not found");
                                }
                            }
                            catch (Exception ex)
                            {
                                fileShareMsgs.Items.Add("BindFiles::Error");
                            }
                            finally
                            {
                                fileShareMsgs.Items.Add("BindFiles::Undo impersonation");
                                wCtx.Undo();
                            }
                        }
                    }
                }
                else
                {
                    currentDirectory = new DirectoryInfo(fileShareItem.ActualPath);
                    fileShareMsgs.Items.Add($"BindFiles::Checking for files in directory {currentDirectory}");
                    if (System.IO.Directory.Exists(fileShareItem.ActualPath))
                    {
                        fileShareMsgs.Items.Add("BindFiles::Getting files");
                        curDirFiles = currentDirectory.GetFiles();
                        fileShareMsgs.Items.Add($"BindFiles::Found {curDirFiles?.Length.ToString()} files");
                        fileList.DataSource = curDirFiles;
                        fileList.DataBind();
                    }
                    else
                    {
                        fileShareMsgs.Items.Add("BindFiles::directory not found");
                    }
                }
            }
            catch (System.Security.SecurityException)
            {
                fileShareMsgs.Items.Add("BindFiles::Security exception");
                ToggleMessage("BindFiles::Error SecurityException", fileshareTabError);
            }
            catch (System.UnauthorizedAccessException)
            {
                fileShareMsgs.Items.Add("BindFiles::Unauthorized");
                ToggleMessage("BindFiles::Error Unauthorized", fileshareTabError);
            }
            catch (Exception ex)
            {
                fileShareMsgs.Items.Add("BindFiles::Error");
                ToggleMessage($"BindFiles::Error::{ex.ToString()}", fileshareTabError);
            }
            //}
        }

        public void BuildBreadcrumb(string RootFileShare, string relativeFileSharePath)
        {
            //Remove the current root from the path and trim the extra backslashes
            fileShareMsgs.Items.Add($"BuildBreadcrumb::RootFileShare={RootFileShare} Relative path={relativeFileSharePath}");
            string trimmedPath = relativeFileSharePath.Replace(RootFileShare, "")?.TrimStart('\\')?.TrimEnd('\\');
            fileShareMsgs.Items.Add($"BuildBreadcrumb::trimmed path {trimmedPath}");
            //string trimmedPath = relativeFileSharePath.TrimStart('\\').TrimEnd('\\');
            string[] crumbPath = trimmedPath.Split('\\');
            List<HyperLink> links = new List<HyperLink>();

            HyperLink link = new HyperLink
            {
                Text = RootFileShare,
                NavigateUrl = UrlUtility.BuildFolderUri(RootFileShare).ToString()
            };
            links.Add(link);

            fileShareMsgs.Items.Add($"BuildBreadcrumb::Link1={link.NavigateUrl}");
            string curPathNode = string.Empty;
            StringBuilder curPath = new StringBuilder();
            string currentNode = string.Empty;

            for (int i = 0; i < crumbPath.Length; i++)
            {
                currentNode = crumbPath[i];
                fileShareMsgs.Items.Add($"BuildBreadcrumb::Link {i.ToString()}={currentNode}");
                if (i + 1 != crumbPath.Length)
                {
                    link = new HyperLink
                    {
                        Text = currentNode
                    };
                    curPath.Append(currentNode);
                    link.NavigateUrl = UrlUtility.BuildFolderUri(curPath.ToString()).ToString();
                    links.Add(link);
                }
                fileShareMsgs.Items.Add($"BuildBreadcrumb::Link={link.NavigateUrl}");
                curPathNode = currentNode;
            }


            //If we are at the root, the trimmed path will be empty, dont add a >
            if (!string.IsNullOrEmpty(trimmedPath))
            {
                HyperLink lastNode = new HyperLink() {
                   Text = curPathNode,
                   NavigateUrl = "#"
                };
                links.Add(lastNode);
            }

            fileShareMsgs.Items.Add($"BuildBreadcrumb::Bind {links.Count.ToString()} links");
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
                ToggleMessage(ex.ToString(), ldapTabError);
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
                    //Impersonate the current user to get the files to browse
                    if (backendCallType.SelectedValue == "asuser")
                    {
                        ADUser currentUser = LdapHelper.GetAdUser(Request.LogonUserIdentity.Name);
                        if (currentUser != null)
                        {
                            fileShareMsgs.Items.Add($"BindFiles::Impersonating user {currentUser.UserPrincipalName}");
                            WindowsIdentity wi = new WindowsIdentity(currentUser.UserPrincipalName);

                            using (WindowsImpersonationContext wCtx = wi.Impersonate())
                            {
                                try
                                {

                                    count.Text = currentFolder.EnumerateFileSystemInfos().Count().ToString();
                                    folderName.Text = string.Format("<span class='glyphicon {0}'></span>&nbsp;<span class=''>{1}</span>", "glyphicon-ok-circle", currentFolder.Name);
                                    FileShareItem fs = new FileShareItem()
                                    {
                                        ActualPath = currentFolder.FullName,
                                        DisplayName = currentFolder.Name,
                                        ParentFileShare = fileShareRootPath
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
                                finally
                                {
                                    wCtx.Undo();
                                }
                            }
                        }
                    }
                    else
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
                curLink.NavigateUrl = UrlUtility.BuildShareUri(curFileShare.ActualPath, curFileShare.DisplayName).ToString();
                curLink.Text = curFileShare.DisplayName;
                fileShareMsgs.Items.Add($"DirectoryList_ItemCommand::ListFiles {curFileShare.ActualPath}");
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
                fileShareMsgs.Items.Add($"DirectoryList_ItemCommand::ListFiles {fs.ActualPath}");
                BindFiles(fs);
                BuildBreadcrumb(fs.ParentFileShare, fs.ActualPath);
            }
        }

        protected void msmqQueueSendMessage_Click(object sender, EventArgs e)
        {
            // check if queue exists, if not create it
            MessageQueue msMq = null;
            if (!MessageQueue.Exists(msmqQueueName.Text))
            {
                msMq = MessageQueue.Create(msmqQueueName.Text);
            }
            else
            {
                msMq = new MessageQueue(msmqQueueName.Text);
            }
            try
            {
                LogMessage logMessage = new LogMessage()
                {
                    Id = Guid.NewGuid().ToString(),
                    Created = DateTime.UtcNow,
                    Message = msmqQueueMsg.Text,
                    Name = Request.LogonUserIdentity.Name
                };
                msMq.Send(logMessage);
            }
            catch (MessageQueueException ee)
            {
                
            }
            catch (Exception eee)
            {
                
            }
            finally
            {
                msMq.Close();
            }
            Console.WriteLine("Message sent ......");
            UpdateMsmqMessageCount();
        }

        void UpdateMsmqMessageCount()
        {
            MessageQueue msMq = new MessageQueue(msmqQueueName.Text, QueueAccessMode.Peek);
            long messageCount = msMq.GetAllMessages().Length;
            msmqQueueMsgCount.Text = messageCount.ToString();
        }

        protected void msmqQueueReadMessage_Click(object sender, EventArgs e)
        {
            MessageQueue msMq = new MessageQueue(msmqQueueName.Text);

            try
            {

                // msMq.Formatter = new XmlMessageFormatter(new Type[] {typeof(string)});
                msMq.Formatter = new XmlMessageFormatter(new Type[] { typeof(LogMessage) });
                var message = (LogMessage)msMq.Receive().Body;
                msmqQueueReadMsg.Text = Json.Encode(message);
            }
            catch (MessageQueueException ee)
            {
                Console.Write(ee.ToString());
            }
            catch (Exception eee)
            {
                Console.Write(eee.ToString());
            }
            finally
            {
                msMq.Close();
            }
            Console.WriteLine("Message received ......");
            UpdateMsmqMessageCount();
        }

        protected void msmqQueueReadMessageCount_Click(object sender, EventArgs e)
        {
            UpdateMsmqMessageCount();
        }

        //protected void CreateRandomFile_Click(object sender, EventArgs e)
        //{
        //    TODO: Validate this URL
        //    fullFileSharePath = fileShareRootPath + currentPath;

        //    Impersonate the current user to get the files to browse
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
        //                        Do something with this error
        //                    }
        //                }
        //                Rebind the UI
        //                bindFiles(userUpn);
        //            }
        //        }
        //    }
        //}
    }
}