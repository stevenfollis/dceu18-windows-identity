using System;
using System.Collections.Generic;
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
        /// backendCallType control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.RadioButtonList backendCallType;

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
        /// userGroupsGrid control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.DataGrid userGroupsGrid;

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
                ToggleMessage($"BindClaimsTab::Error{ex.ToString()}", claimsTabError);
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
                ToggleMessage($"BindGroupsTab::{gex.ToString()}", winPrincipalTabError);
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
    }
}