using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using IISSite.Common.Helpers;
using IISSite.Common.Models;

namespace IISSite.Pages.Secure.Forms
{
    public class Default : System.Web.UI.Page
    {
        #region WebControls
        /// <summary>
        /// loginName control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Label loginName;

        /// <summary>
        /// UserSourceLocation control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Label UserSourceLocation;

        /// <summary>
        /// claimsTabError control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Panel claimsTabErrorPanel;

        /// <summary>
        /// claimsTabError control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Label claimsTabError;

        /// <summary>
        /// claimsGrid control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.DataGrid claimsGrid;

        /// <summary>
        /// winPrincipalTabError control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Label winPrincipalTabError;

        /// <summary>
        /// winPrincipalTabError control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Panel winPrincipalTabErrorPanel;

        /// <summary>
        /// userGroupsGrid control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.DataGrid userGroupsGrid;

        /// <summary>
        /// dbMessages control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.ListBox dbMessages;

        /// <summary>
        /// databaseTabErrorPanel control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Panel databaseTabErrorPanel;

        /// <summary>
        /// databaseTabError control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Label databaseTabError;

        /// <summary>
        /// dbServerName control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.TextBox dbServerName;

        /// <summary>
        /// dbDatabaseName control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.TextBox dbDatabaseName;
        /// <summary>
        /// dbQuery control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.TextBox dbQuery;
        /// <summary>
        /// GetData control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Button GetData;

        /// <summary>
        /// sqlResults control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Panel sqlResults;

        /// <summary>
        /// sqlDataGrid control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.DataGrid sqlDataGrid;
        #endregion

        protected override void OnLoad(EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                //ShowData.Attributes["class"] = "btn btn-link collapsed";
                BindUserData();
                BindClaimsTab();
                BindGroupsTab();
            }
        }

        protected void BindClaimsTab()
        {
            try
            {
                //ClaimsIdentity claimsIdentity = Context.User.Identity;
                FormsIdentity formsIdentity = (FormsIdentity)User.Identity;

                string networkClaimType = "http://schemas.microsoft.com/ws/2012/01/insidecorporatenetwork";
                string userLocationLabel = "{0} the corporate network";
                string userLocation = "UNKNOWN";
                Claim sourceClaim = formsIdentity.Claims.FirstOrDefault(c => c.Type.ToString(CultureInfo.InvariantCulture) == networkClaimType);
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
                claimsGrid.DataSource = formsIdentity.Claims;
                claimsGrid.AutoGenerateColumns = false;
                claimsGrid.DataBind();
            }
            catch (Exception ex)
            {
                ToggleMessage($"BindClaimsTab::Error{ex.ToString()}", claimsTabErrorPanel, claimsTabError);
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
                ToggleMessage($"BindGroupsTab::{gex.ToString()}", winPrincipalTabErrorPanel, winPrincipalTabError);
            }
        }

        private void ToggleMessage(string msg, Panel messagePanel, Label messageControl, bool isError = false, bool display = false)
        {
            if (messagePanel != null && messageControl != null)
            {
                messageControl.Text = msg;
                messagePanel.Visible = display || isError;
            }
        }

        protected void GetSQLData_Click(object sender, EventArgs e)
        {
            // Open the connection.
            ToggleMessage("", databaseTabErrorPanel, databaseTabError);
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

                dbMessages.Items.Add($"Connecting to [{sqlConnectionString.ToString()}]");

                DataSet ds = new DataSet();
                DataTable dt = new DataTable();
                SqlConnection dbConnection = null;
                SqlDataAdapter dataadapter = null;

                // Run the SQL statement, and then get the returned rows to the DataReader.
                dbConnection = new SqlConnection(sqlConnectionString.ToString());
                dbMessages.Items.Add("Opening DB");
                dbConnection.Open();
                dataadapter = new SqlDataAdapter(sqlQuery, dbConnection);
                dbMessages.Items.Add("Fetching data");
                dataadapter.Fill(ds, "data");

                dbConnection.Close();
                sqlDataGrid.DataSource = ds.Tables["data"].DefaultView;
                sqlDataGrid.DataBind();
                sqlResults.Visible = true;
            }
            catch (Exception sqlex)
            {
                ToggleMessage($"GetSQLData_Click::{sqlex.ToString()}", databaseTabErrorPanel, databaseTabError, true, true);
                sqlResults.Visible = false;
            }
        }

    }
}