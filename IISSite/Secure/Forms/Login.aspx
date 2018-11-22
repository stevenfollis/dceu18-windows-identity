<%@ Page Language="C#" AutoEventWireup="true" Inherits="IISSite.Pages.Secure.Forms.Login" MasterPageFile="FormsLayout.Master" %>

<asp:Content ContentPlaceHolderID="MainBody" runat="server">
    <form runat="server">
        <asp:ScriptManager runat="server"></asp:ScriptManager>
        <div class="row justify-content-center">
            <div class="col-xl-5 col-lg-6 col-md-7">
                <div class="text-center">
                    <h1 class="h2">Welcome Back</h1>
                    <p class="lead">Log in to your account to continue</p>
                    <div class="form-group">
                        <asp:Label ID="Msg" CssClass="alert alert-danger" runat="server" />
                    </div>
                    <div class="form-group">
                        <label for="UsernameTextbox" type="email" placeholder="Email Address">Email address</label>
                        <asp:TextBox ID="UsernameTextbox" runat="server" TextMode="Email" CssClass="form-control" />
                    </div>
                    <div class="form-group">
                        <label for="PasswordTextbox" type="password" placeholder="Password">Password</label>
                        <asp:TextBox ID="PasswordTextbox" runat="server" TextMode="Password" CssClass="form-control" />
                    </div>
                    <div>
                        <p>
                            Use: User Name: <i>user1@winid.net</i> Password: <i>password</i> or
                                        <br />
                            User Name: <i>user2@winid.net</i> Password: <i>password</i>
                        </p>
                    </div>
                    <div class="checkbox mb-3">
                        <label>
                            <asp:CheckBox ID="NotPublicCheckBox" runat="server" />
                            Check here if this is <span style="text-decoration: underline">not</span> a public computer.
                        </label>
                    </div>
                    <asp:Button ID="LoginButton" Text="Login" OnClick="Login_OnClick" runat="server" CssClass="btn btn-lg btn-primary btn-block" />
                </div>
            </div>
        </div>
    </form>
</asp:Content>
