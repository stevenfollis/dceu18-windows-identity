<%@ Page Language="C#" AutoEventWireup="true" Inherits="IISSite.Pages.Secure.Forms.Logout" MasterPageFile="FormsLayout.Master" %>

<asp:Content ContentPlaceHolderID="MainBody" runat="server">
    <form runat="server">
        <asp:ScriptManager runat="server"></asp:ScriptManager>

        <div class="container">
            <div class="row">
                <div class="col-md-6 offset-md-3">
                    <asp:Button ID="LogoutButton" CssClass="btn btn-primary" runat="server" OnClick="LogoutButton_OnClick" Text="Click to logout" />
                </div>
            </div>
        </div>  
    </form>
</asp:Content>
