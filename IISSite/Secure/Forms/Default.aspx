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
                            <asp:Panel ID="claimsTabErrorPanel" CssClass="alert alert-danger" runat="server" Visible="false">
                                <asp:Label ID="claimsTabError" runat="server"></asp:Label>
                            </asp:panel>
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
                        <asp:Panel ID="winPrincipalTabErrorPanel" CssClass="alert alert-danger" runat="server" Visible="false">
                            <asp:Label ID="winPrincipalTabError" runat="server" ></asp:Label>
                        </asp:panel>
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
            <asp:UpdatePanel runat="server" RenderMode="Inline" UpdateMode="Conditional" ValidateRequestMode="Disabled">
                <ContentTemplate>
                    <div class="row">
                        <div class="col-md-12">
                            <asp:ListBox ID="dbMessages" runat="server"></asp:ListBox>
                        </div>
                        <asp:Panel ID="databaseTabErrorPanel" CssClass="alert alert-danger" runat="server" Visible="false">
                            <asp:Label ID="databaseTabError" runat="server" Visible="false"></asp:Label>
                        </asp:Panel>
                    </div>
                    <div class="form-group">
                        <label for="dbServerName">Server Name</label>
                        <asp:TextBox runat="server" CssClass="form-control" ID="dbServerName" placeholder="Enter Servername" />
                    </div>
                    <div class="form-group">
                        <label for="dbDatabaseName">Database Name</label>
                        <asp:TextBox runat="server" CssClass="form-control" ID="dbDatabaseName" placeholder="Database Name" />
                    </div>
                    <div class="form-group">
                        <label for="dbQuery">Sql Query</label>
                        <asp:TextBox runat="server" CssClass="form-control" ID="dbQuery" placeholder="SQL Query" Columns="10" Rows="5" TextMode="MultiLine" Text="SELECT TOP 100 * FROM Categories" />
                    </div>
                    <asp:Button OnClick="GetSQLData_Click" ID="GetData" Text="Get SQL Data" runat="server" />
                    <div class="card">
                        <div class="card-body">
                            <asp:Panel ID="sqlResults" runat="server" Visible="false">
                                <asp:DataGrid ID="sqlDataGrid" AutoGenerateColumns="true" runat="server" CssClass="table table-striped table-bordered">
                                </asp:DataGrid>
                            </asp:Panel>
                        </div>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </form>
</asp:Content>
