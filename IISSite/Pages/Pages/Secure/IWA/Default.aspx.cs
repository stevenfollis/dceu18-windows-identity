using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Messaging;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using IISSite.Common.Helpers;
using IISSite.Common.Models;
using Newtonsoft.Json;

namespace IISSite.Pages.Secure.IWA
{
    public class Default : System.Web.UI.Page
    {
        #region "WebControls"
        protected global::System.Web.UI.WebControls.Label loginName;
        protected global::System.Web.UI.WebControls.Label UserSourceLocation;
        protected global::System.Web.UI.WebControls.RadioButtonList backendCallType;
        protected global::System.Web.UI.WebControls.Label claimsTabError;
        protected global::System.Web.UI.WebControls.DataGrid userGroupsGrid;
        protected global::System.Web.UI.WebControls.DataGrid claimsGrid;
        protected global::System.Web.UI.WebControls.Label winPrincipalTabError;
        protected global::System.Web.UI.WebControls.ListBox resultsMessages;
        protected global::System.Web.UI.WebControls.Panel resultsMessagePanel;
        protected global::System.Web.UI.WebControls.Panel resultsErrorMessagePanel;
        protected global::System.Web.UI.WebControls.Panel dataResultsPanel;
        protected global::System.Web.UI.WebControls.Panel fileResultsPanel;
        protected global::System.Web.UI.WebControls.Label resultsErrorMessage;
        protected global::System.Web.UI.WebControls.DataGrid dataResultsGrid;
        protected global::System.Web.UI.WebControls.TextBox dbServerName;
        protected global::System.Web.UI.WebControls.TextBox dbDatabaseName;
        protected global::System.Web.UI.WebControls.TextBox dbQuery;
        protected global::System.Web.UI.WebControls.Button GetData;
        protected global::System.Web.UI.WebControls.TextBox ldapDomainname;
        protected global::System.Web.UI.WebControls.TextBox ldapSearchString;
        protected global::System.Web.UI.WebControls.RadioButtonList ldapUsersOrGroups;
        protected global::System.Web.UI.WebControls.Button ldapBind;
        protected global::System.Web.UI.WebControls.TextBox msmqQueueName;
        protected global::System.Web.UI.WebControls.TextBox msmqMachineName;
        protected global::System.Web.UI.WebControls.TextBox msmqQueueMsg;
        protected global::System.Web.UI.WebControls.Button msmqQueueSendMessage;
        protected global::System.Web.UI.WebControls.Label msmqQueueMsgCount;
        protected global::System.Web.UI.WebControls.Button msmqQueueReadMessageCount;
        protected global::System.Web.UI.WebControls.Button msmqQueueReadMessage;
        protected global::System.Web.UI.WebControls.TextBox msmqQueueReadMsg;
        protected global::System.Web.UI.WebControls.TextBox fileShareServerName;
        protected global::System.Web.UI.WebControls.TextBox fileShareName;
        protected global::System.Web.UI.WebControls.Button fileShareBind;
        protected global::System.Web.UI.WebControls.DataList breadcrumbLinks;
        protected global::System.Web.UI.WebControls.Repeater fileShareList;
        protected global::System.Web.UI.WebControls.Repeater directoryList;
        protected global::System.Web.UI.WebControls.Repeater fileList;
        protected global::System.Web.UI.WebControls.Panel GMSAInfoErrorPanel;
        protected global::System.Web.UI.WebControls.Panel gmsaResultsPanel;
        protected global::System.Web.UI.WebControls.Label GMSAInfoError;
        protected global::System.Web.UI.WebControls.TextBox gmsaName;
        protected global::System.Web.UI.WebControls.Label gmsaSamAccountName;
        protected global::System.Web.UI.WebControls.Label gmsaSPNs;
        protected global::System.Web.UI.WebControls.Label gmsaAccountControl;
        protected global::System.Web.UI.WebControls.Label gmsaDelegationInfo;
        protected global::System.Web.UI.WebControls.Label gmsaSid;

        public string FileShareRootPath { get; private set; }
        #endregion

        protected override void OnLoad(EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                //ShowData.Attributes["class"] = "btn btn-link collapsed";
                BindUserData();
                BindClaimsTab();
                BindGroupsTab();
                BindImpersonationOptions();
            }
        }

        private void BindGmsaInfo(ADUser gmsa)
        {
            if (gmsa != null)
            {
                gmsaSamAccountName.Text = gmsa?.SamAccountName;
                gmsaSPNs.Text = gmsa?.ServicePrincipalNames;
                gmsaAccountControl.Text = gmsa?.UserAccountControl;
                gmsaDelegationInfo.Text = gmsa?.AllowedToDelegateTo;
                gmsaSid.Text = gmsa?.Sid.ToString();
            }
        }

        private void BindImpersonationOptions()
        {
            ListItem gmsaListItem = new ListItem()
            {
                Text = $"GMSA [{System.Security.Principal.WindowsIdentity.GetCurrent().Name}] (Trusted subsystem, service account/app pool account)",
                Value = "gmsa",
                Selected = true
            };

            ListItem userListItem = new ListItem()
            {
                Text = $"User [{Request.LogonUserIdentity.Name}] (Kerberos [Un-]Constrained Delegation)",
                Value = "user"
            };

            backendCallType.Items.Add(gmsaListItem);
            backendCallType.Items.Add(userListItem);
        }

        protected void BindClaimsTab()
        {
            try
            {
                ClaimsIdentity claimsIdentity = (ClaimsIdentity)User.Identity;

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
                ToggleMessage($"BindClaimsTab::Error{ex.ToString()}", true, true);
            }
        }

        protected void BindUserData()
        {
            ADUser currentUser = LdapHelper.GetAdUser(Request.LogonUserIdentity.Name);
            if (currentUser != null)
            {
                loginName.Text = currentUser.DisplayName;
            }
        }

        protected void BindGroupsTab()
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
                ToggleMessage($"BindGroupsTab::{gex.ToString()}", true);
            }
        }

        private void ToggleMessage(string errMsg, bool isError = false, bool display = true)
        {
            resultsMessagePanel.Visible = display;
            resultsErrorMessage.Text = errMsg;
            resultsErrorMessage.Visible = isError;
            resultsErrorMessagePanel.Visible = isError;
        }

        protected void GetSQLData_Click(object sender, EventArgs e)
        {
            // Open the connection.
            ClearResultPanels();

            string sqlQuery = dbQuery.Text;
            SqlConnection dbConnection = null;

            try
            {
                DataSet ds = new DataSet();
                SqlDataAdapter dataadapter = null;
                SqlConnectionStringBuilder sqlConnectionString = new SqlConnectionStringBuilder
                {
                    IntegratedSecurity = true,
                    DataSource = dbServerName.Text,
                    InitialCatalog = dbDatabaseName.Text,
                    TrustServerCertificate = true
                };


                if (backendCallType.SelectedValue == "user")
                {
                    //Impersonate the current user to get the files to browse
                    ADUser currentUser = LdapHelper.GetAdUser(Request.LogonUserIdentity.Name);
                    if (currentUser != null)
                    {
                        resultsMessages.Items.Add($"GetSQLData_Click::Impersonating as {currentUser.UserPrincipalName}");
                        using (WindowsImpersonationContext wi = ImpersonateEndUser(currentUser.UserPrincipalName))
                        {
                            try
                            {
                                resultsMessages.Items.Add($"GetSQLData_Click::Connecting to [{sqlConnectionString.ToString()}] {backendCallType.SelectedValue}");
                                dbConnection = new SqlConnection(sqlConnectionString.ToString());
                                resultsMessages.Items.Add("GetSQLData_Click::Opening DB");
                                dbConnection.Open();
                                dataadapter = new SqlDataAdapter(sqlQuery, dbConnection);
                                resultsMessages.Items.Add("GetSQLData_Click::Running query");
                                dataadapter.Fill(ds, "data");
                            }
                            catch (Exception ex)
                            {
                                resultsMessages.Items.Add($"GetSQLData_Click::Could not get data {ex.ToString()}");
                            }
                            finally
                            {
                                resultsMessages.Items.Add("GetSQLData_Click::Disabling impersonation");
                                wi.Undo();
                            }
                        }
                    }
                    else
                    {
                        resultsMessages.Items.Add($"GetSQLData_Click::ERROR: {Request.LogonUserIdentity.Name} not found. Check UPN");
                        ToggleMessage("GetSQLData_Click::Could not bind AD User", true, true);
                    }
                }
                else
                {
                    // Run the SQL statement, and then get the returned rows to the DataReader.
                    dbConnection = new SqlConnection(sqlConnectionString.ToString());
                    resultsMessages.Items.Add("GetSQLData_Click::Opening DB");
                    dbConnection.Open();
                    dataadapter = new SqlDataAdapter(sqlQuery, dbConnection);
                    resultsMessages.Items.Add("GetSQLData_Click::Fetching data");
                    dataadapter.Fill(ds, "data");
                }

                if (ds != null)
                {
                    resultsMessages.Items.Add("GetSQLData_Click::Data found. Binding to view");
                    dataResultsGrid.DataSource = ds.Tables["data"].DefaultView;
                    dataResultsGrid.DataBind();

                    dataResultsPanel.Visible = true;
                    dataResultsGrid.Visible = true;
                }
                else
                {
                    resultsMessages.Items.Add("GetSQLData_Click::ERROR:No data found.");
                }
            }
            catch (Exception sqlex)
            {
                ToggleMessage($"GetSQLData_Click::{sqlex.ToString()}", true, true);
                dataResultsPanel.Visible = false;
            }
            finally
            {
                if (dbConnection != null)
                {
                    if (dbConnection.State == ConnectionState.Open)
                    {
                        resultsMessages.Items.Add("GetSQLData_Click::Closing DB connection.");
                        dbConnection.Close();
                    }
                }
                resultsMessagePanel.Visible = true;
            }
        }

        private WindowsImpersonationContext ImpersonateEndUser(string userUpn)
        {
            // The WindowsIdentity(string) constructor uses the new
            // Kerberos S4U extension to get a logon for the user
            // without a password.
            WindowsIdentity wi = new WindowsIdentity(userUpn);
            return wi.Impersonate();
        }

        protected void GetFileShareData_Click(object sender, EventArgs e)
        {
            ClearResultPanels();

            try
            {
                string serverName = fileShareServerName.Text;
                string fileFolderName = fileShareName.Text;
                FileShareRootPath = $@"\\{serverName}\{fileFolderName}";
                resultsMessages.Items.Add($"Fetching files from {FileShareRootPath}");

                FileShareItem rootFileShareItem = new FileShareItem()
                {
                    ParentFileShare = FileShareRootPath,
                    ActualPath = FileShareRootPath,
                    DisplayName = fileFolderName
                };

                resultsMessages.Items.Add("Binding folders");
                BindFolders(rootFileShareItem);

                resultsMessages.Items.Add("Binding files");
                BindFiles(rootFileShareItem);

                fileResultsPanel.Visible = true;
            }
            catch (Exception ex)
            {
                ToggleMessage($"FileShareBind_Click::Error::{ex.ToString()}", true, true);
            }
            finally
            {
                resultsMessagePanel.Visible = true;
            }
        }

        private void ClearResultPanels()
        {
            ToggleMessage("", false, false);
            resultsMessages.Items.Clear();
            resultsMessagePanel.Visible = false;
            dataResultsGrid.DataSource = null;
            dataResultsPanel.Visible = false;
            fileResultsPanel.Visible = false;
            gmsaResultsPanel.Visible = false;
        }

        private void BindFolders(FileShareItem rootFileShareItem)
        {
            string currentPath = rootFileShareItem.ActualPath;
            DirectoryInfo currentDirectory = null;
            DirectoryInfo[] curFolders = null;
            DirectoryInfo currentParent = null;

            resultsMessages.Items.Add($"BindFolders::{currentPath}");
            try
            {
                //Impersonate the current user to get the files to browse
                if (backendCallType.SelectedValue == "user")
                {
                    ADUser currentUser = LdapHelper.GetAdUser(Request.LogonUserIdentity.Name);
                    if (currentUser != null)
                    {
                        resultsMessages.Items.Add($"BindFolders::Impersonating user {currentUser.UserPrincipalName}");
                        using (WindowsImpersonationContext wi = ImpersonateEndUser(currentUser.UserPrincipalName))
                        {
                            try
                            {
                                currentDirectory = new DirectoryInfo(rootFileShareItem.ActualPath);
                                resultsMessages.Items.Add($"Current directory is {rootFileShareItem.ActualPath}");
                                if (System.IO.Directory.Exists(currentPath))
                                {
                                    resultsMessages.Items.Add("Path exists, get directories");
                                    curFolders = currentDirectory.GetDirectories();
                                    //if there are no children and we are not at the root, show parents
                                    if (currentDirectory.Parent != null)
                                    {
                                        if (currentDirectory.Parent.FullName == currentPath)
                                        {
                                            //We are at the root
                                            currentParent = currentDirectory;
                                            resultsMessages.Items.Add($"Current is root {currentParent}");
                                        }
                                    }
                                    else
                                    {
                                        //We are at the root
                                        currentParent = currentDirectory;
                                        resultsMessages.Items.Add($"At the root. Set the parent to {currentParent}");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                resultsMessages.Items.Add("Error");
                            }
                            finally
                            {
                                resultsMessages.Items.Add("Undoing impersonation");
                                wi.Undo();
                            }
                        }
                    }
                }
                else
                {
                    currentDirectory = new DirectoryInfo(rootFileShareItem.ActualPath);
                    resultsMessages.Items.Add($"Looking for directory {currentDirectory}");
                    if (System.IO.Directory.Exists(currentPath))
                    {
                        resultsMessages.Items.Add("Path exists, get sub directories");
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

                resultsMessages.Items.Add($"Bind folders {curFolders?.Length.ToString()}");
                directoryList.DataSource = curFolders;
                directoryList.DataBind();

                resultsMessages.Items.Add($"Bind files with {currentPath}");
                BindFiles(rootFileShareItem);

                //Build the breadcrumb for the page
                resultsMessages.Items.Add($"Build breadcrumb with {currentPath}");
                BuildBreadcrumb(rootFileShareItem.ParentFileShare, currentPath);

            }
            catch (System.Security.SecurityException)
            {
                resultsMessages.Items.Add("ERROR Security exception");
                ToggleMessage("BindFolders::Error SecurityException", true, true);
            }
            catch (System.UnauthorizedAccessException)
            {
                resultsMessages.Items.Add("ERROR Unauthorized exception");
                ToggleMessage("BindFolders::Error Unauthorized", true, true);
            }
            catch (Exception ex)
            {
                resultsMessages.Items.Add("ERROR");
                ToggleMessage($"BindFolders::Error::{ex.ToString()}", true, true);
            }
            finally
            {
                resultsMessagePanel.Visible = true;
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
                if (backendCallType.SelectedValue == "user")
                {
                    ADUser currentUser = LdapHelper.GetAdUser(Request.LogonUserIdentity.Name);
                    if (currentUser != null)
                    {
                        resultsMessages.Items.Add($"BindFiles::Impersonating user {currentUser.UserPrincipalName}");
                        WindowsIdentity wi = new WindowsIdentity(currentUser.UserPrincipalName);
                        using (WindowsImpersonationContext wCtx = wi.Impersonate())
                        {
                            try
                            {
                                currentDirectory = new DirectoryInfo(fileShareItem.ActualPath);
                                resultsMessages.Items.Add($"BindFiles::Checking for files in directory {currentDirectory}");
                                if (System.IO.Directory.Exists(fileShareItem.ActualPath))
                                {
                                    resultsMessages.Items.Add("BindFiles::Getting files");
                                    curDirFiles = currentDirectory.GetFiles();
                                    resultsMessages.Items.Add($"BindFiles::Found {curDirFiles?.Length.ToString()} files");
                                    fileList.DataSource = curDirFiles;
                                    fileList.DataBind();
                                }
                                else
                                {
                                    resultsMessages.Items.Add("BindFiles::directory not found");
                                }
                            }
                            catch (Exception ex)
                            {
                                resultsMessages.Items.Add("BindFiles::Error");
                            }
                            finally
                            {
                                resultsMessages.Items.Add("BindFiles::Undo impersonation");
                                wCtx.Undo();
                            }
                        }
                    }
                }
                else
                {
                    currentDirectory = new DirectoryInfo(fileShareItem.ActualPath);
                    resultsMessages.Items.Add($"BindFiles::Checking for files in directory {currentDirectory}");
                    if (System.IO.Directory.Exists(fileShareItem.ActualPath))
                    {
                        resultsMessages.Items.Add("BindFiles::Getting files");
                        curDirFiles = currentDirectory.GetFiles();
                        resultsMessages.Items.Add($"BindFiles::Found {curDirFiles?.Length.ToString()} files");
                        fileList.DataSource = curDirFiles;
                        fileList.DataBind();
                    }
                    else
                    {
                        resultsMessages.Items.Add("BindFiles::directory not found");
                    }
                }
                resultsMessagePanel.Visible = true;
            }
            catch (System.Security.SecurityException)
            {
                resultsMessages.Items.Add("BindFiles::Security exception");
                ToggleMessage("BindFiles::Error SecurityException", true, true);
            }
            catch (System.UnauthorizedAccessException)
            {
                resultsMessages.Items.Add("BindFiles::Unauthorized");
                ToggleMessage("BindFiles::Error Unauthorized", true, true);
            }
            catch (Exception ex)
            {
                resultsMessages.Items.Add("BindFiles::Error");
                ToggleMessage($"BindFiles::Error::{ex.ToString()}", true, true);
            }
            //}
        }

        public void BuildBreadcrumb(string RootFileShare, string relativeFileSharePath)
        {
            //Remove the current root from the path and trim the extra backslashes
            resultsMessages.Items.Add($"BuildBreadcrumb::RootFileShare={RootFileShare} Relative path={relativeFileSharePath}");
            string trimmedPath = relativeFileSharePath.Replace(RootFileShare, "")?.TrimStart('\\')?.TrimEnd('\\');
            resultsMessages.Items.Add($"BuildBreadcrumb::trimmed path {trimmedPath}");
            //string trimmedPath = relativeFileSharePath.TrimStart('\\').TrimEnd('\\');
            string[] crumbPath = trimmedPath.Split('\\');
            List<HyperLink> links = new List<HyperLink>();

            HyperLink link = new HyperLink
            {
                Text = RootFileShare,
                NavigateUrl = UrlUtility.BuildFolderUri(RootFileShare).ToString()
            };
            links.Add(link);

            resultsMessages.Items.Add($"BuildBreadcrumb::Link1={link.NavigateUrl}");
            string curPathNode = string.Empty;
            StringBuilder curPath = new StringBuilder();
            string currentNode = string.Empty;

            for (int i = 0; i < crumbPath.Length; i++)
            {
                currentNode = crumbPath[i];
                resultsMessages.Items.Add($"BuildBreadcrumb::Link {i.ToString()}={currentNode}");
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
                resultsMessages.Items.Add($"BuildBreadcrumb::Link={link.NavigateUrl}");
                curPathNode = currentNode;
            }


            //If we are at the root, the trimmed path will be empty, dont add a >
            if (!string.IsNullOrEmpty(trimmedPath))
            {
                HyperLink lastNode = new HyperLink()
                {
                    Text = curPathNode,
                    NavigateUrl = "#"
                };
                links.Add(lastNode);
            }

            resultsMessages.Items.Add($"BuildBreadcrumb::Bind {links.Count.ToString()} links");
            breadcrumbLinks.DataSource = links;
            breadcrumbLinks.DataBind();
        }

        protected void GetGmsaData_Click(object sender, EventArgs e)
        {
            ClearResultPanels();

            try
            {
                string gmsaUserName = gmsaName.Text;
                if (!gmsaUserName.EndsWith("$"))
                {
                    gmsaUserName = gmsaUserName + "$";
                }
                resultsMessages.Items.Add($"GetGmsaData_Click::Getting gmsa data for {gmsaUserName}");

                ADUser gmsaUser = GetGmsaInfo(gmsaUserName);
                if (gmsaUser != null)
                {
                    BindGmsaInfo(gmsaUser);
                    resultsMessages.Items.Add($"GetGmsaData_Click::Binding gmsa data for GMSA SID:{gmsaUser.Sid}");
                    gmsaResultsPanel.Visible = true;
                }
                else
                {
                    resultsMessages.Items.Add($"GetGmsaData_Click::GMSA {gmsaUserName} not found");
                }
            }
            catch (Exception ex)
            {
                ToggleMessage($"GetGmsaData_Click::Error::{ex.ToString()}", true, true);
            }
            finally
            {
                resultsMessagePanel.Visible = true;
            }
        }

        protected ADUser GetGmsaInfo(string GmsaName)
        {
            SearchResultCollection searchResults = null;
            ADUser gmsaUser = null;
            // set up domain context
            PrincipalContext ctx = new PrincipalContext(ContextType.Domain);

            // find a user
            Principal user = Principal.FindByIdentity(ctx, GmsaName);

            if (user != null)
            {
                Domain userDomain = Domain.GetCurrentDomain();
                string userSearchString = $"(&(objectClass=msDS-GroupManagedServiceAccount)((sAMAccountName={user.SamAccountName})))";

                DirectoryEntry rootEntry = new DirectoryEntry($@"LDAP://{userDomain}")
                {
                    AuthenticationType = System.DirectoryServices.AuthenticationTypes.Secure
                };
                DirectorySearcher searcher = new DirectorySearcher(rootEntry)
                {
                    SearchScope = SearchScope.Subtree,
                    SizeLimit = 50,
                };

                searcher.PropertiesToLoad.Add("servicePrincipalName");
                searcher.PropertiesToLoad.Add("userAccountControl");
                searcher.PropertiesToLoad.Add("msDS-AllowedToDelegateTo");
                searcher.Filter = userSearchString;
                resultsMessages.Items.Add($"LdapBind_Click::Setting filter for for users using filter [{searcher.Filter}]");
                searchResults = searcher.FindAll();
                if (searchResults.Count == 1)
                {
                    SearchResult result = searchResults[0];
                    //Fill out the rest of the user infor
                    gmsaUser = new ADUser()
                    {
                        SamAccountName = user.SamAccountName,
                        AllowedToDelegateTo = ParseProp(result.Properties["msDS-AllowedToDelegateTo"]),
                        ServicePrincipalNames = ParseProp(result.Properties["servicePrincipalName"]),
                        UserAccountControl = ParseProp(result.Properties["userAccountControl"]),
                        Sid = user.Sid
                    };
                }
            }
            return gmsaUser;
        }

        private string ParseProp(ResultPropertyValueCollection resultPropertyValueCollection)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var r in resultPropertyValueCollection)
            {
                stringBuilder.AppendFormat("[{0}] ", r.ToString());
            }
            return stringBuilder.ToString();
        }

        protected void GetLdapData_Click(object sender, EventArgs e)
        {
            ClearResultPanels();

            string fqdnToSearch = ldapDomainname.Text;
            string searchString = ldapSearchString.Text;
            SearchResultCollection searchResults = null;
            string userSearchString = $"(&(objectCategory=user)(objectClass=user)(|(sAMAccountlName= **{searchString}**)(userPrincipalName=**{searchString}**)))";
            string groupSeachString = $"(&(objectCategory=group)(objectClass=group)(|(name=**{searchString}**)(displayName=**{searchString}**)))";

            resultsMessages.Items.Add($"LdapBind_Click::Searching {fqdnToSearch} for {searchString}");

            try
            {
                if (backendCallType.SelectedValue == "user")
                {
                    resultsMessages.Items.Add($"LdapBind_Click::Getting LDAP data as user {Request.LogonUserIdentity.Name}");

                    ADUser currentUser = LdapHelper.GetAdUser(Request.LogonUserIdentity.Name);
                    if (currentUser != null)
                    {
                        resultsMessages.Items.Add($"BindFiles::Impersonating user {currentUser.UserPrincipalName}");
                        WindowsIdentity wi = new WindowsIdentity(currentUser.UserPrincipalName);

                        using (WindowsImpersonationContext wCtx = wi.Impersonate())
                        {
                            try
                            {
                                DirectoryEntry rootEntry = new DirectoryEntry($@"GC://{fqdnToSearch}")
                                {
                                    AuthenticationType = System.DirectoryServices.AuthenticationTypes.Secure
                                };

                                DirectorySearcher searcher = new DirectorySearcher(rootEntry)
                                {
                                    SearchScope = SearchScope.Subtree,
                                    SizeLimit = 50
                                };

                                switch (ldapUsersOrGroups.SelectedValue)
                                {
                                    case "users":
                                        {
                                            searcher.Filter = userSearchString;
                                            resultsMessages.Items.Add($"LdapBind_Click::Setting filter for for users using filter [{searcher.Filter}]");
                                            break;
                                        }
                                    case "groups":
                                        {
                                            resultsMessages.Items.Add("LdapBind_Click::Setting filter for for groups");
                                            searcher.Filter = ("(&(objectCategory=group))");
                                            break;
                                        }
                                }

                                searchResults = searcher.FindAll();
                            }
                            catch (Exception searchEx)
                            {
                                resultsMessages.Items.Add($"LdapBind_Click::ERROR Impersonating as {currentUser.UserPrincipalName} {searchEx.ToString()}");
                            }
                            finally
                            {
                                wCtx.Undo();
                            }
                        }
                    }
                    else
                    {
                        resultsMessages.Items.Add($"LdapBind_Click::ERROR {Request.LogonUserIdentity.Name} not found to impersonate");
                    }
                }
                else
                {
                    resultsMessages.Items.Add("LdapBind_Click::Getting LDAP data as App Pool Account");

                    DirectoryEntry rootEntry = new DirectoryEntry($@"LDAP://{fqdnToSearch}")
                    {
                        AuthenticationType = System.DirectoryServices.AuthenticationTypes.Secure
                    };
                    DirectorySearcher searcher = new DirectorySearcher(rootEntry)
                    {
                        SearchScope = SearchScope.Subtree,
                        SizeLimit = 50
                    };

                    switch (ldapUsersOrGroups.SelectedValue)
                    {
                        case "users":
                            {
                                searcher.Filter = userSearchString;
                                resultsMessages.Items.Add($"LdapBind_Click::Setting filter for for users using filter [{searcher.Filter}]");
                                break;
                            }
                        case "groups":
                            {
                                resultsMessages.Items.Add("LdapBind_Click::Setting filter for for groups");
                                searcher.Filter = ("(&(objectCategory=group))");
                                break;
                            }
                    }

                    searchResults = searcher.FindAll();
                }

                resultsMessages.Items.Add($"LdapBind_Click::Found [{searchResults?.Count.ToString()}] results");
                dataResultsGrid.DataSource = searchResults;
                dataResultsGrid.DataBind();
                dataResultsPanel.Visible = true;
                resultsMessagePanel.Visible = true;
            }
            catch (Exception ex)
            {
                resultsMessages.Items.Add("LdapBind_Click::ERROR");
                ToggleMessage(ex.ToString(), true, true);
            }
            finally
            {
                resultsMessagePanel.Visible = true;
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
                    if (backendCallType.SelectedValue == "user")
                    {
                        ADUser currentUser = LdapHelper.GetAdUser(Request.LogonUserIdentity.Name);
                        if (currentUser != null)
                        {
                            resultsMessages.Items.Add($"BindFiles::Impersonating user {currentUser.UserPrincipalName}");
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
                                        ParentFileShare = FileShareRootPath
                                    };

                                    if (currentFolder.GetDirectories().Length > 0)
                                    {
                                        //Folder has sub folders, enumerate those
                                    }
                                    folderName.CommandArgument = JsonConvert.SerializeObject(fs);
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
                        folderName.CommandArgument = JsonConvert.SerializeObject(fs);
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
                resultsMessages.Items.Add($"DirectoryList_ItemCommand::ListFiles {curFileShare.ActualPath}");
            }
        }

        protected void DirectoryList_ItemCommand(object sender, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "listfiles")
            {
                // Get the value of command argument
                var value = e.CommandArgument;
                // Do whatever operation you want.  
                FileShareItem fs = JsonConvert.DeserializeObject<FileShareItem>(e.CommandArgument.ToString());
                resultsMessages.Items.Add($"DirectoryList_ItemCommand::ListFiles {fs.ActualPath}");
                BindFiles(fs);
                BuildBreadcrumb(fs.ParentFileShare, fs.ActualPath);
            }
        }

        protected void MsmqQueueSendMessage_Click(object sender, EventArgs e)
        {
            ClearResultPanels();

            // check if queue exists, if not create it
            MessageQueue msMq = null;
            StringCollection queMsgs = null;
            //List<LogMessage> msmqMessages = new List<LogMessage>();

            LogMessage logMessage = new LogMessage()
            {
                Id = Guid.NewGuid().ToString(),
                Created = DateTime.UtcNow,
                Message = msmqQueueMsg.Text
            };

            resultsMessages.Items.Add($"MsmqQueueSendMessage_Click::Creating a new message {logMessage.Id}");

            string qName = MsmqHelper.GetDirectFormatName(msmqMachineName.Text, msmqQueueName.Text, true);

            try
            {
                //Impersonate the current user to get the files to browse
                if (backendCallType.SelectedValue == "user")
                {
                    ADUser currentUser = LdapHelper.GetAdUser(Request.LogonUserIdentity.Name);
                    if (currentUser != null)
                    {
                        resultsMessages.Items.Add($"MsmqQueueSendMessage_Click::Impersonating user {currentUser.UserPrincipalName}");
                        using (WindowsImpersonationContext wi = ImpersonateEndUser(currentUser.UserPrincipalName))
                        {
                            try
                            {
                                msMq = MsmqHelper.GetOrCreateQueue(msmqMachineName.Text, msmqQueueName.Text, true, out queMsgs, true, true);

                                //Add any messages from the call for debugging
                                foreach (string s in queMsgs)
                                {
                                    resultsMessages.Items.Add(s);
                                }

                                if (msMq != null)
                                {
                                    msMq.Formatter = new XmlMessageFormatter(new Type[] { typeof(LogMessage) });
                                    logMessage.Name = currentUser.UserPrincipalName;
                                    msMq.Send(logMessage);
                                    resultsMessages.Items.Add($"MsmqQueueSendMessage_Click::Message sent to queue {msMq.FormatName}");
                                    resultsMessages.Items.Add($"MsmqQueueSendMessage_Click::reading messages from queue {msMq.FormatName}");
                                    UpdateMsmqMessageCount();
                                }
                                else
                                {
                                    resultsMessages.Items.Add($"MsmqQueueSendMessage_Click::Queue {qName} could not be found or you do not have rights");
                                }
                                resultsMessagePanel.Visible = true;
                                dataResultsPanel.Visible = true;
                            }
                            catch (Exception ex)
                            {
                                resultsMessages.Items.Add($"MsmqQueueSendMessage_Click::Error {ex.ToString()}");
                            }
                            finally
                            {
                                resultsMessages.Items.Add("MsmqQueueSendMessage_Click::Undoing impersonation");
                                if (msMq != null)
                                {
                                    resultsMessages.Items.Add("MsmqQueueSendMessage_Click::Closing MSMQ Queue");
                                    msMq.Close();
                                }
                                wi.Undo();
                            }
                        }
                    }
                }
                else
                {
                    msMq = MsmqHelper.GetOrCreateQueue(msmqMachineName.Text, msmqQueueName.Text, true, out queMsgs, true, true);

                    //Add any messages from the call for debugging
                    foreach (string s in queMsgs)
                    {
                        resultsMessages.Items.Add(s);
                    }
                    if (msMq != null)
                    {
                        msMq.Formatter = new XmlMessageFormatter(new Type[] { typeof(LogMessage) });
                        logMessage.Name = WindowsIdentity.GetCurrent().Name;
                        msMq.Send(logMessage);
                        resultsMessages.Items.Add($"MsmqQueueSendMessage_Click::Message sent to queue {msMq.FormatName}");

                        resultsMessages.Items.Add($"MsmqQueueSendMessage_Click::reading messages from queue {msMq.FormatName}");
                        UpdateMsmqMessageCount();
                    }
                    else
                    {
                        resultsMessages.Items.Add($"MsmqQueueSendMessage_Click::Queue {qName} could not be found");
                    }
                    dataResultsPanel.Visible = true;
                }
            }
            catch (System.Security.SecurityException)
            {
                resultsMessages.Items.Add("ERROR Security exception");
                ToggleMessage("MsmqQueueSendMessage_Click::Error SecurityException", true, true);
            }
            catch (System.UnauthorizedAccessException)
            {
                resultsMessages.Items.Add("ERROR Unauthorized exception");
                ToggleMessage("MsmqQueueSendMessage_Click::Error Unauthorized", true, true);
            }
            catch (MessageQueueException ee)
            {
                resultsMessages.Items.Add("MsmqQueueSendMessage_Click::ERROR MessageQueue");
                ToggleMessage($"MsmqQueueSendMessage_Click::ERROR {ee.ToString()}", true, true);
            }
            catch (Exception eee)
            {
                resultsMessages.Items.Add("MsmqQueueSendMessage_Click::ERROR");
                ToggleMessage($"MsmqQueueSendMessage_Click::ERROR {eee.ToString()}", true, true);
            }
            finally
            {
                if (msMq != null)
                {
                    msMq.Close();
                }
                resultsMessagePanel.Visible = true;
            }
        }

        protected Message PeekWithoutTimeout(MessageQueue q, Cursor cursor, PeekAction action)
        {
            Message ret = null;
            try
            {
                ret = q.Peek(new TimeSpan(1000), cursor, action);
            }
            catch (MessageQueueException mqe)
            {
                if (!mqe.Message.ToLower().Contains("timeout"))
                {
                    throw;
                }
            }
            return ret;
        }

        protected int GetMessageCount(MessageQueue q, out List<LogMessage> messages)
        {
            int count = 0;
            Cursor cursor = q.CreateCursor();
            messages = new List<LogMessage>();
            q.Formatter = new XmlMessageFormatter(new Type[] { typeof(LogMessage) });

            Message m = PeekWithoutTimeout(q, cursor, PeekAction.Current);
            if (m != null)
            {
                //Add the first message
                LogMessage logMessage = (LogMessage)m.Body;
                messages.Add(logMessage);

                while ((m = PeekWithoutTimeout(q, cursor, PeekAction.Next)) != null)
                {
                    //Add the other messages in the qu
                    logMessage = (LogMessage)m.Body;
                    messages.Add(logMessage);
                    count++;
                }
            }
            return messages.Count;
        }

        protected void UpdateMsmqMessageCount()
        {
            string qName = MsmqHelper.GetDirectFormatName(msmqMachineName.Text, msmqQueueName.Text, true);
            MessageQueue msMq = new MessageQueue(qName, QueueAccessMode.Peek);
            long messageCount = GetMessageCount(msMq, out List<LogMessage> msmqMessages);
            msmqQueueMsgCount.Text = messageCount.ToString();
            dataResultsGrid.DataSource = msmqMessages;
            dataResultsGrid.DataBind();
            dataResultsPanel.Visible = true;
            dataResultsPanel.Visible = true;
            resultsMessagePanel.Visible = true;
        }

        protected void MsmqQueueReadMessage_Click(object sender, EventArgs e)
        {
            ClearResultPanels();

            resultsMessages.Items.Add("MsmqQueueReadMessage_Click::Reading Messages");

            MessageQueue msMq = null;
            StringCollection queMsgs = null;

            string qName = MsmqHelper.GetDirectFormatName(msmqMachineName.Text, msmqQueueName.Text, true);

            try
            {
                //Impersonate the current user to get the files to browse
                if (backendCallType.SelectedValue == "user")
                {
                    ADUser currentUser = LdapHelper.GetAdUser(Request.LogonUserIdentity.Name);
                    if (currentUser != null)
                    {
                        resultsMessages.Items.Add($"MsmqQueueReadMessage_Click::Impersonating user {currentUser.UserPrincipalName}");
                        using (WindowsImpersonationContext wi = ImpersonateEndUser(currentUser.UserPrincipalName))
                        {
                            try
                            {
                                msMq = MsmqHelper.GetOrCreateQueue(msmqMachineName.Text, msmqQueueName.Text, true, out queMsgs, true, true);

                                //Add any messages from the call for debugging
                                foreach (string s in queMsgs)
                                {
                                    resultsMessages.Items.Add(s);
                                }

                                if (msMq != null)
                                {
                                    msMq.Formatter = new XmlMessageFormatter(new Type[] { typeof(LogMessage) });
                                    resultsMessages.Items.Add($"MsmqQueueReadMessage_Click::reading messages from queue {msMq.FormatName}");
                                    var message = (LogMessage)msMq.Receive().Body;
                                    resultsMessages.Items.Add($"MsmqQueueReadMessage_Click::Message={JsonConvert.SerializeObject(message)}");
                                    UpdateMsmqMessageCount();
                                }
                                else
                                {
                                    resultsMessages.Items.Add($"MsmqQueueReadMessage_Click::Queue {qName} could not be found or you do not have rights");
                                }
                                resultsMessagePanel.Visible = true;
                                dataResultsPanel.Visible = true;
                            }
                            catch (Exception ex)
                            {
                                resultsMessages.Items.Add("Error");
                            }
                            finally
                            {
                                resultsMessages.Items.Add("Undoing impersonation");
                                if (msMq != null)
                                {
                                    msMq.Close();
                                }
                                wi.Undo();
                            }
                        }
                    }
                }
                else
                {
                    msMq = MsmqHelper.GetOrCreateQueue(msmqMachineName.Text, msmqQueueName.Text, true, out queMsgs, true, true);

                    //Add any messages from the call for debugging
                    foreach (string s in queMsgs)
                    {
                        resultsMessages.Items.Add(s);
                    }
                    if (msMq != null)
                    {
                        msMq.Formatter = new XmlMessageFormatter(new Type[] { typeof(LogMessage) });
                        resultsMessages.Items.Add($"MsmqQueueReadMessage_Click::reading messages from queue {msMq.FormatName}");
                        var message = (LogMessage)msMq.Receive().Body;
                        resultsMessages.Items.Add($"MsmqQueueReadMessage_Click::Message={JsonConvert.SerializeObject(message)}");
                        UpdateMsmqMessageCount();
                    }
                    else
                    {
                        resultsMessages.Items.Add($"MsmqQueueReadMessage_Click::Queue {qName} could not be found");
                    }
                }
            }
            catch (System.Security.SecurityException)
            {
                resultsMessages.Items.Add("ERROR Security exception");
                ToggleMessage("MsmqQueueReadMessage_Click::Error SecurityException", true, true);
            }
            catch (System.UnauthorizedAccessException)
            {
                resultsMessages.Items.Add("ERROR Unauthorized exception");
                ToggleMessage("MsmqQueueReadMessage_Click::Error Unauthorized", true, true);
            }
            catch (MessageQueueException ee)
            {
                resultsMessages.Items.Add("MsmqQueueReadMessage_Click::ERROR MessageQueue");
                ToggleMessage($"MsmqQueueReadMessage_Click::ERROR {ee.ToString()}", true, true);
            }
            catch (Exception eee)
            {
                resultsMessages.Items.Add("MsmqQueueReadMessage_Click::ERROR");
                ToggleMessage($"MsmqQueueReadMessage_Click::ERROR {eee.ToString()}", true, true);
            }
            finally
            {
                if (msMq != null)
                {
                    msMq.Close();
                }
                resultsMessagePanel.Visible = true;
            }
        }

        protected void MsmqQueueReadMessageCount_Click(object sender, EventArgs e)
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

