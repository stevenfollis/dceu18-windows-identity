<%@ Page Language="C#" AutoEventWireup="true" Inherits="IISSite.Pages.Secure.Forms.Login" MasterPageFile="FormsLayout.Master" %>

<asp:Content ContentPlaceHolderID="MainBody" runat="server">
    <form runat="server">
        <asp:ScriptManager runat="server"></asp:ScriptManager>

        <div class="row justify-content-center">
            <div class="col-xl-4 col-lg-5 col-md-6">
                <div class="text-center">
                    <h1>Welcome Back</h1>
                    <p class="lead">Log in to your account to continue</p>
                    <asp:Panel ID="errorPanel" CssClass="alert alert alert-danger" runat="server" Visible="false">
                        <asp:Label ID="Msg" CssClass="" runat="server" />
                    </asp:Panel>
                    <div class="form-group">
                        <label for="UsernameTextbox">Email address</label>
                        <asp:TextBox ID="UsernameTextbox" runat="server" TextMode="Email" CssClass="form-control" />
                    </div>
                    <div class="form-group">
                        <label for="PasswordTextbox">Password</label>
                        <asp:TextBox ID="PasswordTextbox" runat="server" TextMode="Password" CssClass="form-control" />
                    </div>
                    <div class="form-group">
                        <p class="lead">User names are hard coded in the web.config for this example:</p>
                    </div>
                    <div class="form-group">
                        <b>User Name:</b> <i>user1@winid.net</i> <b>Password:</b> <i>password</i> or
                                        <br />
                        <b>User Name:</b> <i>user2@winid.net</i> <b>Password:</b> <i>password</i>
                    </div>
                    <div class="form-group">
                        <div class="checkbox mb-3">
                            <label>
                                <asp:CheckBox ID="NotPublicCheckBox" runat="server" />
                                Check here if this is <span style="text-decoration: underline">not</span> a public computer.
                            </label>
                        </div>
                    </div>
                    <asp:Button ID="LoginButton" Text="Login" OnClick="Login_OnClick" runat="server" CssClass="btn btn-lg btn-primary btn-block" />
                </div>
            </div>
        </div>
    </form>
</asp:Content>
