<%@ Page Language="C#" AutoEventWireup="true" Inherits="IISSite.Pages.Secure.IWA.Default" MasterPageFile="IWALayout.Master" %>

<%@ Import Namespace="System" %>
<%@ Import Namespace="System.Web" %>
<%@ Import Namespace="System.Security.Principal" %>
<%@ Import Namespace="System.Security.Claims" %>
<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="System.Data.SqlClient" %>
<%@ Import Namespace="System.Configuration" %>
<%@ Import Namespace="System.DirectoryServices" %>
<%@ Import Namespace="System.Collections" %>
<%@ Import Namespace="System.Text" %>
<%@ Import Namespace="System.Configuration" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="System.Linq" %>
<%@ Import Namespace="System.Web.UI" %>
<%@ Import Namespace="System.Web.UI.WebControls" %>
<%@ Import Namespace="System.Web.UI.HtmlControls" %>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="System.Globalization" %>
<%@ Import Namespace="System.IdentityModel.Tokens" %>

<asp:Content ContentPlaceHolderID="MainBody" runat="server">
    <form runat="server">
        <asp:ScriptManager runat="server"></asp:ScriptManager>
        <div class="row">
            <div class="col-lg-12">
                <div class="page-header mb-4">
                    <div class="media">
                        <div class="media-body ml-3">
                            <h1 class="mb-0">Windows Container Tester</h1>
                            <p class="lead">Use this page to test the GMSA Account and Delegation</p>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div class="row">
            <!--Col1-->
            <div class="col-md-3">
                <div class="accordion" id="dataCommands">
                    <div class="card">
                        <div class="card-header" id="sqlCommandSection">
                            <h5 class="mb-0">
                                <button class="btn btn-link" type="button" data-toggle="collapse" data-target="#sqlCommands" aria-expanded="false" aria-controls="sqlCommands">
                                    Connect to SQL
                                </button>
                            </h5>
                        </div>
                        <div id="sqlCommands" class="collapse" aria-labelledby="sqlCommandSection" data-parent="#dataCommands">
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
                    <div class="card">
                        <div class="card-header" id="ldapCommandSection">
                            <h5 class="mb-0">
                                <button class="btn btn-link collapsed" type="button" data-toggle="collapse" data-target="#ldapCommands" aria-expanded="false" aria-controls="ldapCommands">
                                    LDAP
                                </button>
                            </h5>
                        </div>
                        <div id="ldapCommands" class="collapse" aria-labelledby="ldapCommandSection" data-parent="#dataCommands">
                            <div class="card-body">
                                <div class="form-group">
                                    <label for="ldapDomainname">Domain/Forest Name</label>
                                    <asp:TextBox runat="server" CssClass="form-control" ID="ldapDomainname" placeholder="Enter Domain Name or forest FQDN" />
                                </div>
                                <div class="form-group">
                                    <label for="dbDatabaseName">Users or groups</label>
                                    <asp:RadioButtonList ID="ldapUsersOrGroups" runat="server" CssClass="custom-radio">
                                        <asp:ListItem Text="Users (sAMAccountName or UPN)" Value="users" Selected="True"></asp:ListItem>
                                        <asp:ListItem Text="Groups (Name or DisplayName)" Value="groups"></asp:ListItem>
                                    </asp:RadioButtonList>
                                </div>
                                <div class="form-group">
                                    <label for="ldapSearchString">Search string</label>
                                    <asp:TextBox runat="server" CssClass="form-control" ID="ldapSearchString" placeholder="Enter a search string" />
                                </div>
                                <asp:Button OnClick="GetLdapData_Click" ID="ldapBind" Text="Get LDAP data" runat="server" CssClass="btn btn-primary" />
                            </div>
                        </div>
                    </div>
                    <div class="card">
                        <div class="card-header" id="msmqCommandSection">
                            <h5 class="mb-0">
                                <button class="btn btn-link collapsed" type="button" data-toggle="collapse" data-target="#msmqCommands" aria-expanded="false" aria-controls="msmqCommands">
                                    MSMQ
                                </button>
                            </h5>
                        </div>
                        <div id="msmqCommands" class="collapse" aria-labelledby="msmqCommandSection" data-parent="#dataCommands">
                            <div class="card-body">
                                <h4>Messages in the Queue [<asp:Label ID="msmqQueueMsgCount" runat="server"></asp:Label>]</h4>
                                <div class="form-group">
                                    <label for="msmqMachineName">MSMQ Machine name</label>
                                    <asp:TextBox ID="msmqMachineName" CssClass="form-control ms-uppercase" runat="server"></asp:TextBox>
                                </div>
                                <div class="form-group">
                                    <label for="msmqQueueName">MSMQ Queue name (only works for private queues)</label>
                                    <asp:TextBox ID="msmqQueueName" CssClass="form-control ms-uppercase" runat="server"></asp:TextBox>
                                </div>
                                <div class="form-group">
                                    <label for="msmqQueueMsg">Message</label>
                                    <asp:TextBox ID="msmqQueueMsg" CssClass="form-control ms-uppercase" runat="server"></asp:TextBox>
                                </div>
                                <asp:Button ID="msmqQueueGetMessages" OnClick="MsmqQueueGetMessages_Click" Text="Get Messages" runat="server" CssClass="btn btn-primary" />
                                <asp:Button ID="msmqQueueSendMessage" OnClick="MsmqQueueSendMessage_Click" Text="Send 1" runat="server" CssClass="btn btn-primary" />
                                <asp:Button ID="msmqQueueReadMessage" OnClick="MsmqQueueReadMessage_Click" Text="Pop 1" runat="server" CssClass="btn btn-primary" />
                            </div>
                        </div>
                    </div>
                    <div class="card">
                        <div class="card-header" id="fileShareCommandSection">
                            <h5 class="mb-0">
                                <button class="btn btn-link collapsed" type="button" data-toggle="collapse" data-target="#fileShareCommands" aria-expanded="false" aria-controls="fileShareCommands">
                                    File Share
                                </button>
                            </h5>
                        </div>
                        <div id="fileShareCommands" class="collapse" aria-labelledby="fileShareCommandSection" data-parent="#dataCommands">
                            <div class="card-body">
                                <div class="form-group">
                                    <label for="fileShareServerName">Fileshare Server Name</label>
                                    <asp:TextBox ID="fileShareServerName" CssClass="form-control ms-uppercase" runat="server"></asp:TextBox>
                                </div>
                                <div class="form-group">
                                    <label for="fileShareName">File share name</label>
                                    <asp:TextBox ID="fileShareName" CssClass="form-control ms-uppercase" runat="server"></asp:TextBox>
                                </div>
                                <asp:Button OnClick="GetFileShareData_Click" ID="fileShareBind" Text="Get File Share data" runat="server" CssClass="btn btn-primary" />
                            </div>
                        </div>
                    </div>
                    <div class="card">
                        <div class="card-header" id="gmsaCommandSection">
                            <h5 class="mb-0">
                                <button class="btn btn-link collapsed" type="button" data-toggle="collapse" data-target="#gmsaCommands" aria-expanded="false" aria-controls="gmsaCommands">
                                    GMSA Information
                                </button>
                            </h5>
                        </div>
                        <div id="gmsaCommands" class="collapse" aria-labelledby="gmsaCommandSection" data-parent="#dataCommands">
                            <div class="card-body">
                                <div class="form-group">
                                    <label for="gmsaName">GMSA Name</label>
                                    <asp:TextBox ID="gmsaName" CssClass="form-control ms-uppercase" runat="server"></asp:TextBox>
                                </div>
                                <asp:Button OnClick="GetGmsaData_Click" ID="gmsaBind" Text="Get GMSA info" runat="server" CssClass="btn btn-primary" />
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <!--Col2-->
            <div class="col-md-7">
                <h3>Specify how to make calls to the backend services:</h3>
                <div class="input-group border">
                    <div class="input-group-prepend m-3">
                        <asp:RadioButtonList ID="backendCallType" runat="server" AutoPostBack="false" RepeatDirection="Vertical" CssClass="custom-radio">
                        </asp:RadioButtonList>
                    </div>
                </div>
                <div class="card">
                    <asp:UpdatePanel runat="server" RenderMode="Inline" UpdateMode="Conditional" ValidateRequestMode="Disabled">
                        <ContentTemplate>
                            <div class="card-header">
                                <asp:Panel ID="resultsMessagePanel" runat="server" Visible="false">
                                    <asp:Repeater ID="resultsMessages" runat="server">
                                        <HeaderTemplate>
                                            <div class="accordion" id="errorMessages">
                                                <div class="card">
                                                    <div class="card-header" id="messages">
                                                        <h5 class="mb-0">
                                                            <button class="btn btn-info" type="button" data-toggle="collapse" data-target="#messageGrid" aria-expanded="true" aria-controls="messageGrid">
                                                                Show log messages
                                                            </button>
                                                        </h5>
                                                    </div>
                                                    <div id="messageGrid" class="collapse" aria-labelledby="messages" data-parent="#errorMessages">
                                                        <div class="card-body">
                                                            <ul class="list-group">
                                        </HeaderTemplate>
                                        <ItemTemplate>
                                            <li class="list-group-item"><%# Container.DataItem %></li>
                                        </ItemTemplate>
                                        <AlternatingItemTemplate>
                                            <li class="list-group-item list-group-item-secondary"><%# Container.DataItem %></li>
                                        </AlternatingItemTemplate>
                                        <FooterTemplate>
                                            </ul>
                                                </div>
                                              </div>
                                                </div>
                                                </div>
                                        </FooterTemplate>
                                    </asp:Repeater>
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
                                <asp:Panel ID="fileResultsPanel" runat="server" Visible="false">
                                    <span class="ms-commandLink">
                                        <asp:DataList ID="breadcrumbLinks" runat="server" RepeatDirection="Horizontal" Visible="false">
                                            <ItemTemplate>
                                                <a href='<%# DataBinder.Eval(Container.DataItem, "NavigateUrl") %>'><%# DataBinder.Eval(Container.DataItem, "Text") %></a>
                                            </ItemTemplate>
                                            <SeparatorTemplate>
                                                &gt;
                                            </SeparatorTemplate>
                                        </asp:DataList>
                                    </span>
                                    <asp:Repeater ID="fileShareList" runat="server" OnItemDataBound="FileShareList_ItemDataBound">
                                        <HeaderTemplate>
                                            <h3 class="ms-webpart-titleText">Chose a file share to browse</h3>
                                            <div class="list-group">
                                        </HeaderTemplate>
                                        <ItemTemplate>
                                            <asp:HyperLink ID="fileShareUrl" CssClass="list-group-item" runat="server"></asp:HyperLink>
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            </div>
                                        </FooterTemplate>
                                    </asp:Repeater>
                                    <table class="table table-striped table-bordered">
                                        <asp:Repeater ID="directoryList" runat="server" OnItemDataBound="DirectoryList_ItemDataBound" OnItemCommand="DirectoryList_ItemCommand">
                                            <HeaderTemplate>
                                                <thead>
                                                    <tr>
                                                        <th>Folder Name</th>
                                                        <th>Sub Directories/Files</th>
                                                        <th>Created</th>
                                                        <th>Last Modified</th>
                                                    </tr>
                                                </thead>
                                                <tbody>
                                            </HeaderTemplate>
                                            <ItemTemplate>
                                                <tr>
                                                    <td>
                                                        <asp:LinkButton CssClass="ms-listLink" runat="server" ID="folderName"></asp:LinkButton>
                                                    </td>
                                                    <td>
                                                        <asp:Label runat="server" ID="fileCount"></asp:Label></td>
                                                    <td>
                                                        <asp:Label runat="server" ID="folderCreated"></asp:Label></td>
                                                    <td>
                                                        <asp:Label runat="server" ID="folderLastModified"></asp:Label></td>
                                                </tr>
                                            </ItemTemplate>
                                            <FooterTemplate>
                                                </tbody>
                                            </FooterTemplate>
                                        </asp:Repeater>
                                    </table>
                                    <h3 class="ms-webpart-titleText">Files</h3>
                                    <table class="table table-striped table-bordered table-hover">
                                        <asp:Repeater ID="fileList" runat="server" OnItemDataBound="FileList_ItemDataBound">
                                            <HeaderTemplate>
                                                <tr>
                                                    <th>Name</th>
                                                    <th>Size</th>
                                                    <th>Ext</th>
                                                    <th>Attributes</th>
                                                    <th>Created</th>
                                                    <th>Last Modified</th>
                                                    <th>Actions</th>
                                                </tr>
                                            </HeaderTemplate>
                                            <ItemTemplate>
                                                <tr>
                                                    <td>
                                                        <asp:HyperLink CssClass="ms-listLink" runat="server" ID="filename"></asp:HyperLink></td>
                                                    <td>
                                                        <asp:Label runat="server" ID="fileSize"></asp:Label></td>
                                                    <td>
                                                        <asp:Label runat="server" ID="fileExt"></asp:Label></td>
                                                    <td>
                                                        <asp:Label runat="server" ID="fileAttributes"></asp:Label></td>
                                                    <td>
                                                        <asp:Label runat="server" ID="fileCreated"></asp:Label></td>
                                                    <td>
                                                        <asp:Label runat="server" ID="fileLastModified"></asp:Label></td>
                                                    <td>
                                                        <button id="actionsLink" type="button" runat="server">Actions</button>
                                                    </td>
                                                </tr>
                                            </ItemTemplate>
                                            <FooterTemplate>
                                            </FooterTemplate>
                                        </asp:Repeater>
                                    </table>
                                </asp:Panel>
                                <asp:Panel ID="gmsaResultsPanel" runat="server" Visible="false">
                                    <asp:Panel ID="GMSAInfoErrorPanel" CssClass="alert alert-danger" runat="server" Visible="false">
                                        <asp:Label ID="GMSAInfoError" runat="server"></asp:Label>
                                    </asp:Panel>
                                    <ul class="list-group">
                                        <li class="list-group-item">
                                            <div class="row">
                                                <div class="col-6">
                                                    <h5>sAMAccountName:</h5>
                                                    <small class="text-muted">i.e. DOMAIN\GMSA_NAME$</small>
                                                </div>
                                                <div class="col">
                                                    <p class="mb-1">
                                                        <asp:Label ID="gmsaSamAccountName" runat="server"></asp:Label>
                                                    </p>
                                                </div>
                                            </div>
                                        </li>
                                        <li class="list-group-item">
                                            <div class="row">
                                                <div class="col-6">
                                                    <h5>SID:</h5>
                                                </div>
                                                <div class="col">
                                                    <p class="mb-1">
                                                        <asp:Label ID="gmsaSid" runat="server"></asp:Label>
                                                    </p>
                                                </div>
                                            </div>
                                        </li>
                                        <li class="list-group-item">
                                            <div class="row">
                                                <div class="col-6">
                                                    <h5>SPNs:</h5>
                                                    <small class="text-muted">i.e. HTTP/GMSAName & HTTP/GMSANAME.FQDN</small>
                                                </div>
                                                <div class="col">
                                                    <p class="mb-1">
                                                        <asp:Label ID="gmsaSPNs" runat="server"></asp:Label>
                                                    </p>

                                                </div>
                                            </div>
                                        </li>
                                        <li class="list-group-item">
                                            <div class="row">
                                                <div class="col-6">
                                                    <h5>UserAccount Control:</h5>
                                                    <ul>
                                                        <li><small class="text-muted">NORMAL_ACCOUNT = 512</small></li>
                                                        <li><small class="text-muted">INTERDOMAIN_TRUST_ACCOUNT = 2048</small></li>
                                                        <li><small class="text-muted">WORKSTATION_TRUST_ACCOUNT = 4096</small></li>
                                                        <li><small class="text-muted">TRUSTED_FOR_DELEGATION = 524288</small></li>
                                                        <li><small class="text-muted">TRUSTED_TO_AUTH_FOR_DELEGATION = 16777216</small></li>
                                                    </ul>
                                                </div>
                                                <div class="col">
                                                    <p class="mb-1">
                                                        <asp:Label ID="gmsaAccountControl" runat="server"></asp:Label>
                                                    </p>
                                                </div>
                                            </div>
                                        </li>
                                        <li class="list-group-item">
                                            <div class="row">
                                                <div class="col-6">
                                                    <h5>ms-AllowedToDelegateTo:</h5>
                                                    <small class="text-muted">i.e. LDAP/DCNAME.FQDN, MSSQLSvc/SQL.FQDN:1433</small>
                                                </div>
                                                <div class="col">
                                                    <p class="mb-1">
                                                        <asp:Label ID="gmsaDelegationInfo" runat="server"></asp:Label>
                                                    </p>
                                                </div>
                                            </div>
                                        </li>
                                    </ul>
                                </asp:Panel>
                            </div>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>
            </div>
            <!--Col3-->
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
                                            with impersonation level: <b><%= Request.LogonUserIdentity.ImpersonationLevel %></b><br />
                                            App Pool running as: <b><%= System.Security.Principal.WindowsIdentity.GetCurrent().Name %></b><br />
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
    </form>
</asp:Content>
