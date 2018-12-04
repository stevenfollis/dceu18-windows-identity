using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Web.Security;
using System.Web.UI;
using IISSite.Common.Helpers;
using IISSite.Common.Models;

namespace IISSite.Pages.Secure.Forms
{
    public class Default : System.Web.UI.Page
    {
        #region WebControls
        protected global::System.Web.UI.WebControls.Label loginName;
        protected global::System.Web.UI.WebControls.Label UserSourceLocation;
        protected global::System.Web.UI.WebControls.Panel claimsTabErrorPanel;
        protected global::System.Web.UI.WebControls.Label claimsTabError;
        protected global::System.Web.UI.WebControls.DataGrid claimsGrid;
        protected global::System.Web.UI.WebControls.Label winPrincipalTabError;
        protected global::System.Web.UI.WebControls.Panel winPrincipalTabErrorPanel;
        protected global::System.Web.UI.WebControls.DataGrid userGroupsGrid;
        protected global::System.Web.UI.WebControls.Repeater resultsMessages;
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
        #endregion

        public StringCollection ErrorMessages { get; set; }

        protected override void OnLoad(EventArgs e)
        {
            if (!Page.IsPostBack)
            {
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
                ToggleMessage($"BindClaimsTab::Error{ex.ToString()}", true, true);
            }
        }

        protected void BindUserData()
        {
            loginName.Text = User.Identity.Name;
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
                ToggleMessage($"BindGroupsTab::{gex.ToString()}", true, true);
            }
        }

        private void ToggleMessage(string errMsg, bool isError = false, bool display = true)
        {
            resultsMessagePanel.Visible = display;
            resultsErrorMessage.Text = errMsg;
            resultsErrorMessage.Visible = isError;
            resultsErrorMessagePanel.Visible = isError;
        }

        private void ShowResultMessages()
        {
            resultsMessages.DataSource = ErrorMessages;
            resultsMessages.DataBind();
            resultsMessagePanel.Visible = true;
        }

        protected void GetSQLData_Click(object sender, EventArgs e)
        {
            ClearResultPanels();
            // Open the connection.
            string sqlQuery = dbQuery.Text;
            SqlConnection dbConnection = null; SqlConnectionStringBuilder sqlConnectionString = new SqlConnectionStringBuilder
            {
                IntegratedSecurity = true,
                DataSource = dbServerName.Text,
                InitialCatalog = dbDatabaseName.Text,
                TrustServerCertificate = true
            };

            ErrorMessages.Add($"GetSQLData_Click::Connecting to [{sqlConnectionString.ToString()}]");

            try
            {

                DataSet ds = new DataSet();
                DataTable dt = new DataTable();

                SqlDataAdapter dataadapter = null;

                // Run the SQL statement, and then get the returned rows to the DataReader.
                dbConnection = new SqlConnection(sqlConnectionString.ToString());
                ErrorMessages.Add("GetSQLData_Click::Opening DB");
                dbConnection.Open();
                dataadapter = new SqlDataAdapter(sqlQuery, dbConnection);
                ErrorMessages.Add("GetSQLData_Click::Fetching data");
                dataadapter.Fill(ds, "data");

                if (ds != null)
                {
                    ErrorMessages.Add("GetSQLData_Click::Binding data");
                    dataResultsGrid.DataSource = ds.Tables["data"].DefaultView;
                    dataResultsGrid.DataBind();
                    dataResultsPanel.Visible = true;
                }
            }
            catch (Exception sqlex)
            {
                ToggleMessage($"GetSQLData_Click::{sqlex.ToString()}", true, true);
            }
            finally
            {
                if (dbConnection != null)
                {
                    if (dbConnection.State == ConnectionState.Open)
                    {
                        ErrorMessages.Add("GetSQLData_Click::Closing database");

                        dbConnection.Close();
                    }
                }
                ShowResultMessages();
            }
        }

        private void ClearResultPanels()
        {
            ToggleMessage("", false, false);
            resultsMessages.DataSource = null;
            resultsMessagePanel.Visible = false;
            dataResultsPanel.Visible = false;
            ErrorMessages = new StringCollection();
        }
    }
}