using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;

namespace IISSite.Pages.Secure.Forms
{
    public class Logout : System.Web.UI.Page
    {
        protected System.Web.UI.WebControls.Button LogoutButton;
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        public void LogoutButton_OnClick(object sender, EventArgs args)
        {
            FormsAuthentication.SignOut();
            FormsAuthentication.RedirectToLoginPage();
        }

    }
}