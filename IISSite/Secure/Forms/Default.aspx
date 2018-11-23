<%@ Page Language="C#" AutoEventWireup="true" Inherits="IISSite.Pages.Secure.Forms.Default" MasterPageFile="FormsLayout.Master" %>

<asp:Content ContentPlaceHolderID="MainBody" runat="server">
    <form runat="server">
        <asp:ScriptManager runat="server"></asp:ScriptManager>
        <section class="jumbotron jumbotron-fluid">
            <div class="container">
                <h1 class="display-4">Hello <b><%= User.Identity.Name %></b></h1>
                <p>
                    You authenticated on <b><%= Server.MachineName %></b><br />
                    using AuthenticationType:<b><%= Request.LogonUserIdentity.AuthenticationType %></b><br />
                    with impersonation level:<b> <%= Request.LogonUserIdentity.ImpersonationLevel %></b><br />
                    App Pool running as:<b> <%= System.Security.Principal.WindowsIdentity.GetCurrent().Name %></b>
                </p>
            </div>
        </section>
        <div class="col-lg-12">
            <div class="accordion" id="logininfo">
                <div class="card">
                    <div class="card-header" id="claimsinfo">
                        <h5 class="mb-0">
                            <button class="btn btn-link" type="button" data-toggle="collapse" data-target="#claimsdata" aria-expanded="true" aria-controls="claimsdata">
                                Your claims
                            </button>
                        </h5>
                    </div>
                    <div id="claimsdata" class="collapse" aria-labelledby="claimsinfo" data-parent="#logininfo">
                        <div class="card-body">
                            <p>
                                <asp:Label ID="claimsTabError" CssClass="bg-danger" runat="server" Visible="false"></asp:Label>
                            </p>
                            <p>
                                <asp:DataGrid ID="claimsGrid" runat="server" CssClass="table table-striped table-bordered table-condensed table-responsive">
                                    <Columns>
                                        <asp:BoundColumn DataField="Type" HeaderText="Type"></asp:BoundColumn>
                                        <asp:BoundColumn DataField="Value" HeaderText="Value"></asp:BoundColumn>
                                    </Columns>
                                </asp:DataGrid>
                            </p>

                        </div>
                    </div>
                </div>
                <div class="card-header" id="winprincipalinfo">
                    <h5 class="mb-0">
                        <button class="btn btn-link" type="button" data-toggle="collapse" data-target="#winprincipaldata" aria-expanded="true" aria-controls="winprincipaldata">
                            Windows Principal Context
                        </button>
                    </h5>
                </div>

                <div id="winprincipaldata" class="collapse" aria-labelledby="winprincipalinfo" data-parent="#logininfo">
                    <div class="card-body">
                        <p>
                            <asp:Label ID="winPrincipalTabError" CssClass="bg-danger" runat="server" Visible="false"></asp:Label>
                        </p>
                        <p>
                            <asp:DataGrid ID="userGroupsGrid" runat="server" AutoGenerateColumns="false" CssClass="table table-striped table-bordered table-condensed table-responsive">
                                <Columns>
                                    <asp:BoundColumn DataField="Name" HeaderText="Name"></asp:BoundColumn>
                                    <asp:BoundColumn DataField="DisplayName" HeaderText="Display Name"></asp:BoundColumn>
                                </Columns>
                            </asp:DataGrid>
                        </p>

                    </div>
                </div>
            </div>
        </div>
    </form>
</asp:Content>
