<%@ Page Language="C#" AutoEventWireup="true" Inherits="IISSite.Pages.Secure.Forms.Logout" MasterPageFile="FormsLayout.Master" %>

<asp:Content ContentPlaceHolderID="MainBody" runat="server">
    <form runat="server">
        <asp:ScriptManager runat="server"></asp:ScriptManager>

        <div class="row justify-content-center">
            <div class="col-xl-4 col-lg-5 col-md-6">
                <div class="text-center">
                    <asp:Button ID="LogoutButton" CssClass="btn btn-primary" runat="server" OnClick="LogoutButton_OnClick" Text="Click to logout" />
                </div>
            </div>
        </div>  
    </form>
</asp:Content>
