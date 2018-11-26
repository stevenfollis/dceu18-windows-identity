<%@ Page Language="C#" AutoEventWireup="true" Inherits="IISSite.Pages.Secure.Forms.Default" MasterPageFile="FormsLayout.Master" %>

<asp:Content ContentPlaceHolderID="MainBody" runat="server">
    <form runat="server">
        <asp:ScriptManager runat="server"></asp:ScriptManager>
        <asp:UpdatePanel runat="server" RenderMode="Inline" UpdateMode="Conditional" ValidateRequestMode="Disabled">
            <ContentTemplate>
                <div class="row">
                    <div class="col-lg-12">
                        <div class="page-header mb-4">
                            <div class="media">
                                <div class="media-body ml-3">
                                    <h1 class="mb-0">Windows Container Tester</h1>
                                    <p class="lead">Use this page to test the GMSA Account for a Forms user</p>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-2">
                        <div class="accordion" id="dataCommands">
                            <div class="card">
                                <div class="card-header" id="sqlCommandSection">
                                    <h5 class="mb-0">
                                        <button class="btn btn-link" type="button" data-toggle="collapse" data-target="#sqlCommands" aria-expanded="true" aria-controls="sqlCommands">
                                            Connect to SQL
                                        </button>
                                    </h5>
                                </div>
                                <div id="sqlCommands" class="collapse show" aria-labelledby="sqlCommandSection" data-parent="#dataCommands">
                                    <div class="card-body">
                                        <div class="form-group">
                                            <label for="dbServerName">Database Server Name</label>
                                            <asp:TextBox runat="server" CssClass="form-control" ID="dbServerName" placeholder="Enter Servername" />
                                        </div>
                                        <div class="form-group">
                                            <label for="dbDatabaseName">Database Name</label>
                                            <asp:TextBox runat="server" CssClass="form-control" ID="dbDatabaseName" placeholder="Database Name" />
                                        </div>
                                        <div class="form-group">
                                            <label for="dbQuery">SQL Query</label>
                                            <asp:TextBox runat="server" CssClass="form-control" ID="dbQuery" placeholder="SQL Query" Columns="8" Rows="3" TextMode="MultiLine" Text="SELECT TOP 100 * FROM Categories" />
                                        </div>
                                        <asp:Button OnClick="GetSQLData_Click" ID="Button1" Text="Get SQL Data" runat="server" CssClass="btn btn-primary" />
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="col-md-8">
                        <div class="card">
                            <div class="card-header">
                                <asp:Panel ID="resultsMessagePanel" CssClass="alert alert-success" runat="server" Visible="false">
                                    <asp:ListBox ID="resultsMessages" CssClass="custom-select" runat="server"></asp:ListBox>
                                </asp:Panel>
                                <asp:Panel ID="resultsErrorMessagePanel" CssClass="alert alert-danger" runat="server" Visible="false">
                                    <asp:Label ID="resultsErrorMessage" runat="server" Visible="false"></asp:Label>
                                </asp:Panel>
                            </div>
                            <div class="card-body">
                                <h5 class="card-title">Data results</h5>
                                <asp:Panel ID="dataResultsPanel" runat="server" Visible="false">
                                    <asp:DataGrid ID="dataResultsGrid" AutoGenerateColumns="true" runat="server" CssClass="table table-striped table-bordered">
                                    </asp:DataGrid>
                                </asp:Panel>
                            </div>
                        </div>
                    </div>
                    <div class="col-md-2">
                        <div class="btn-group-vertical">
                            <!-- Button trigger modal -->
                            <button type="button" class="btn btn-secondary" data-toggle="modal" data-target="#claimsModal">
                                View Claims
                            </button>
                            <button type="button" class="btn btn-secondary" data-toggle="modal" data-target="#windowsCtxModal">
                                View Windows context
                            </button>
                            <button type="button" class="btn btn-secondary" data-toggle="modal" data-target="#loginInfoModal">
                                View Login Info
                            </button>
                            <!-- Claims Modal -->
                            <div class="modal" id="claimsModal" tabindex="-1" role="dialog" aria-labelledby="claimsModalTitle" aria-hidden="true">
                                <div class="modal-dialog modal-dialog-centered modal-lg" role="document">
                                    <div class="modal-content">
                                        <div class="modal-header">
                                            <h5 class="modal-title" id="claimsModalTitle">Claims Infomrmation</h5>
                                            <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                                                <span aria-hidden="true">&times;</span>
                                            </button>
                                        </div>
                                        <div class="modal-body">
                                            <asp:Panel ID="claimsTabErrorPanel" CssClass="alert alert-danger" runat="server" Visible="false">
                                                <asp:Label ID="claimsTabError" runat="server"></asp:Label>
                                            </asp:Panel>
                                            <p>
                                                <asp:DataGrid ID="claimsGrid" runat="server" CssClass="table table-striped table-bordered table-condensed table-responsive">
                                                    <Columns>
                                                        <asp:BoundColumn DataField="Type" HeaderText="Type"></asp:BoundColumn>
                                                        <asp:BoundColumn DataField="Value" HeaderText="Value"></asp:BoundColumn>
                                                    </Columns>
                                                </asp:DataGrid>
                                            </p>
                                        </div>
                                        <div class="modal-footer">
                                            <button type="button" class="btn btn-primary" data-dismiss="modal">Close</button>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <!-- WindowsCtxModal -->
                            <div class="modal" id="windowsCtxModal" tabindex="-1" role="dialog" aria-labelledby="windowsCtxModalTitle" aria-hidden="true">
                                <div class="modal-dialog modal-dialog-centered modal-lg" role="document">
                                    <div class="modal-content">
                                        <div class="modal-header">
                                            <h5 class="modal-title" id="windowsCtxModalTitle">Windows Context Infomrmation</h5>
                                            <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                                                <span aria-hidden="true">&times;</span>
                                            </button>
                                        </div>
                                        <div class="modal-body">
                                            <asp:Panel ID="winPrincipalTabErrorPanel" CssClass="alert alert-danger" runat="server" Visible="false">
                                                <asp:Label ID="winPrincipalTabError" runat="server"></asp:Label>
                                            </asp:Panel>
                                            <p>
                                                <asp:DataGrid ID="userGroupsGrid" runat="server" AutoGenerateColumns="false" CssClass="table table-striped table-bordered table-condensed table-responsive">
                                                    <Columns>
                                                        <asp:BoundColumn DataField="Name" HeaderText="Name"></asp:BoundColumn>
                                                        <asp:BoundColumn DataField="DisplayName" HeaderText="Display Name"></asp:BoundColumn>
                                                    </Columns>
                                                </asp:DataGrid>
                                            </p>
                                        </div>
                                        <div class="modal-footer">
                                            <button type="button" class="btn btn-primary" data-dismiss="modal">Close</button>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <!-- LoginInfoModal -->
                            <div class="modal" id="LoginInfoModal" tabindex="-1" role="dialog" aria-labelledby="LoginInfoModalTitle" aria-hidden="true">
                                <div class="modal-dialog modal-dialog-centered" role="document">
                                    <div class="modal-content">
                                        <div class="modal-header">
                                            <h5 class="modal-title" id="LoginInfoTitle">Login Information</h5>
                                            <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                                                <span aria-hidden="true">&times;</span>
                                            </button>
                                        </div>
                                        <div class="modal-body">
                                            <asp:Panel ID="loginInfoErrorPanel" CssClass="alert alert-danger" runat="server" Visible="false">
                                                <asp:Label ID="loginInfoError" runat="server"></asp:Label>
                                            </asp:Panel>
                                            <div>
                                                <h3>Hello <b>
                                                    <asp:Label ID="loginName" CssClass="ms-uppercase" runat="server"></asp:Label></b></h3>
                                                <p>
                                                    You authenticated on <b><%= Server.MachineName %></b><br />
                                                    using AuthenticationType: <b><%= Request.LogonUserIdentity.AuthenticationType %></b><br />
                                                    with impersonation level: <b> <%= Request.LogonUserIdentity.ImpersonationLevel %></b><br />
                                                    App Pool running as:<b> <%= System.Security.Principal.WindowsIdentity.GetCurrent().Name %></b><br />
                                                    From: <b>
                                                        <asp:Label ID="UserSourceLocation" CssClass="ms-uppercase" runat="server" /></b>
                                                </p>
                                            </div>
                                        </div>
                                        <div class="modal-footer">
                                            <button type="button" class="btn btn-primary" data-dismiss="modal">Close</button>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </form>
</asp:Content>
