using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace IISSite.Pages.Secure.Forms
{
    public partial class Login : System.Web.UI.Page
    {
        public TextBox UsernameTextbox { get; set; }
        public TextBox PasswordTextbox { get; set; }
        public CheckBox NotPublicCheckBox { get; set; }
        public Label Msg { get; set; }
        public Panel errorPanel { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                errorPanel.Visible = false;
            }
        }

        public void Login_OnClick(object sender, EventArgs args)
        {
            string userName = UsernameTextbox.Text;
            string password = PasswordTextbox.Text;
            int expirationTime = 5;

            if (FormsAuthentication.Authenticate(userName, password))
            {
                // Clear any lingering authentication data
            FormsAuthentication.SignOut();

            //Create a new Forms Authentication Ticket
            FormsAuthenticationTicket newAuthTicket = new FormsAuthenticationTicket(userName, true, expirationTime);

            //Create the FormsIdentity object
            FormsIdentity formsIdentity = new FormsIdentity(newAuthTicket);

            List<Claim> claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.Name, userName));

            //Add a claim for the User Name
            formsIdentity.AddClaims(claims);

            //Set the Forms Authentication Cookie
            FormsAuthentication.SetAuthCookie(userName, true);

                FormsAuthentication.RedirectFromLoginPage(UsernameTextbox.Text, NotPublicCheckBox.Checked);
            }
            else
            {
                Msg.Text = "Login failed. Please check your user name and password and try again.";
                errorPanel.Visible = true;
            }
        }
    }
}