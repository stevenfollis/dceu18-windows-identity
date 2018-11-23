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
        <section class="jumbotron jumbotron-fluid">
            <div class="container">
                <h1 class="display-4">Hello <b>
                    <asp:Label ID="loginName" CssClass="ms-uppercase" runat="server"></asp:Label></b></h1>
                <p>
                    You authenticated on <b><%= Server.MachineName %></b><br />
                    using AuthenticationType:<b><%= Request.LogonUserIdentity.AuthenticationType %></b><br />
                    with impersonation level:<b> <%= Request.LogonUserIdentity.ImpersonationLevel %></b><br />
                    App Pool running as:<b> <%= System.Security.Principal.WindowsIdentity.GetCurrent().Name %></b><br />
                    From: <b>
                        <asp:Label ID="UserSourceLocation" CssClass="ms-uppercase" runat="server" /></b>
                </p>
                <p class="lead">Specify how to make calls to the backend services:</p>
                <div class="input-group">
                    <div class="input-group-prepend">
                        <div class="input-group-text">
                            <asp:RadioButtonList ID="backendCallType" runat="server" AutoPostBack="false" RepeatDirection="Vertical">
                            </asp:RadioButtonList>
                        </div>
                    </div>
                </div>
        </section>
        <div class="row">
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
                                    <asp:DataGrid ID="claimsGrid" runat="server" CssClass="table table-striped table-bordered table-condensed">
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
                                <asp:DataGrid ID="userGroupsGrid" runat="server" AutoGenerateColumns="false" CssClass="table table-striped table-bordered table-condensed">
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
        </div>
        <div class="row">
            <div class="col-lg-12">
                <div>
                    <ul class="nav nav-pills mb-3 nav-tabs" id="pills-tab" role="tablist">
                        <li class="nav-item">
                            <a class="nav-link" id="pills-data-tab" data-toggle="pill" href="#pills-data" role="tab" aria-controls="pills-data" aria-selected="false">Test Database</a>
                        </li>
                        <li class="nav-item">
                            <a class="nav-link" id="pills-ldap-tab" data-toggle="pill" href="#pills-ldap" role="tab" aria-controls="pills-ldap" aria-selected="false">Test ldap</a>
                        </li>
                        <li class="nav-item">
                            <a class="nav-link" id="pills-msmq-tab" data-toggle="pill" href="#pills-msmq" role="tab" aria-controls="pills-msmq" aria-selected="false">Test msmq</a>
                        </li>
                        <li class="nav-item">
                            <a class="nav-link" id="pills-fileshare-tab" data-toggle="pill" href="#pills-fileshare" role="tab" aria-controls="pills-fileshare" aria-selected="false">Test fileshare</a>
                        </li>
                    </ul>
                    <div class="tab-content" id="pills-tabContent">
                        <!-- Show Database -->
                        <div class="tab-pane fade" id="pills-data" role="tabpanel" aria-labelledby="pills-data-tab">
                            <asp:UpdatePanel runat="server" RenderMode="Inline" UpdateMode="Conditional" ValidateRequestMode="Disabled">
                                <ContentTemplate>
                                    <div class="row">
                                        <div class="col-md-12">
                                            <asp:ListBox ID="dbMessages" runat="server" Visible="true"></asp:ListBox>
                                        </div>
                                        <asp:Panel ID="databaseTabErrorPanel" CssClass="alert alert-danger" runat="server">
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
                                        <asp:TextBox runat="server" CssClass="form-control" ID="dbQuery" placeholder="SQL Query" Columns="10" Rows="10" TextMode="MultiLine" Text="SELECT TOP 100 * FROM Categories" />
                                    </div>
                                    <asp:Button OnClick="GetSQLData_Click" ID="GetData" Text="Get SQL Data" runat="server" />
                                    <div class="card">
                                        <div class="card-body">
                                            <asp:Panel ID="sqlResults" runat="server" Visible="false">
                                                <asp:DataGrid ID="sqlDataGrid" AutoGenerateColumns="true" runat="server" CssClass="table table-striped table-bordered table-condensed table-responsive">
                                                </asp:DataGrid>
                                            </asp:Panel>
                                        </div>
                                    </div>
                                </ContentTemplate>
                            </asp:UpdatePanel>
                        </div>
                        <div class="tab-pane fade" id="pills-ldap" role="tabpanel" aria-labelledby="pills-ldap-tab">
                            <div class="card">
                                <div class="card-body">
                                    <asp:UpdatePanel runat="server" RenderMode="Inline" UpdateMode="Conditional" ValidateRequestMode="Disabled">
                                        <ContentTemplate>
                                            <div class="row">
                                                <div class="col-md-12">
                                                    <asp:ListBox ID="ldapMessages" runat="server" Visible="true"></asp:ListBox>
                                                </div>
                                                <div class="alert alert-danger">
                                                    <asp:Label ID="ldapTabError" runat="server" Visible="false"></asp:Label>
                                                </div>
                                            </div>
                                            <div class="form-group">
                                                <label for="ldapDomainname">Domain/Forest Name</label>
                                                <asp:TextBox runat="server" CssClass="form-control" ID="ldapDomainname" placeholder="Enter Domain Name or forest FQDN" />
                                            </div>
                                            <div class="form-group">
                                                <label for="dbDatabaseName">Users or groups</label>
                                                <asp:RadioButtonList ID="ldapUsersOrGroups" runat="server">
                                                    <asp:ListItem Text="Users" Value="users" Selected="True"></asp:ListItem>
                                                    <asp:ListItem Text="Groups" Value="groups"></asp:ListItem>
                                                </asp:RadioButtonList>
                                            </div>
                                            <asp:Button OnClick="LdapBind_Click" ID="ldapBind" Text="Get data" runat="server" />
                                            <div class="card">
                                                <div class="card-body">
                                                    <asp:DataGrid ID="ldapDataGrid" AutoGenerateColumns="true" runat="server" CssClass="table table-striped table-bordered table-condensed table-responsive">
                                                    </asp:DataGrid>
                                                </div>
                                            </div>
                                        </ContentTemplate>
                                    </asp:UpdatePanel>
                                </div>
                            </div>
                        </div>
                        <div class="tab-pane fade" id="pills-msmq" role="tabpanel" aria-labelledby="pills-msmq-tab">
                            <div class="card">
                                <div class="card-body">
                                    <asp:UpdatePanel runat="server" RenderMode="Inline" UpdateMode="Conditional" ValidateRequestMode="Disabled">
                                        <ContentTemplate>
                                            <div class="row">
                                                <div class="col-md-12">
                                                    <div class="alert alert-danger">
                                                        <asp:Label ID="msmqTabError" runat="server" Visible="false"></asp:Label>
                                                    </div>
                                                </div>
                                            </div>
                                            <div>
                                                <div class="form-group">
                                                    <label for="msmqQueueName">MSMQ Queue name</label>
                                                    <asp:TextBox ID="msmqQueueName" CssClass="form-control ms-uppercase" runat="server"></asp:TextBox>
                                                </div>
                                                <div class="form-group">
                                                    <label for="msmqQueueMsg">Message</label>
                                                    <asp:TextBox ID="msmqQueueMsg" CssClass="form-control ms-uppercase" runat="server"></asp:TextBox>
                                                </div>
                                                <asp:Button ID="msmqQueueSendMessage" OnClick="MsmqQueueSendMessage_Click" Text="Send Message" runat="server" />
                                            </div>
                                            <div class="form-group">
                                                <label for="msmqQueueMsgCount">Message Count</label>
                                                <asp:Label ID="msmqQueueMsgCount" CssClass="form-control ms-uppercase" runat="server"></asp:Label>
                                            </div>
                                            <asp:Button ID="msmqQueueReadMessageCount" OnClick="MsmqQueueReadMessageCount_Click" Text="Read message count" runat="server" />
                                            <asp:Button ID="msmqQueueReadMessage" OnClick="MsmqQueueReadMessage_Click" Text="Read Message" runat="server" />
                                            <div class="form-group">
                                                <label for="msmqQueueReadMsg">Read Message</label>
                                                <asp:TextBox ID="msmqQueueReadMsg" CssClass="form-control ms-uppercase" runat="server"></asp:TextBox>
                                            </div>
                                        </ContentTemplate>
                                    </asp:UpdatePanel>
                                </div>
                            </div>
                        </div>
                        <div class="tab-pane fade" id="pills-fileshare" role="tabpanel" aria-labelledby="pills-fileshare-tab">
                            <div class="card">
                                <div class="card-body">
                                    <asp:UpdatePanel runat="server" RenderMode="Inline" UpdateMode="Conditional" ValidateRequestMode="Disabled">
                                        <ContentTemplate>
                                            <div class="row">
                                                <div class="col-md-12">
                                                    <asp:ListBox ID="fileShareMsgs" runat="server" Visible="true"></asp:ListBox>
                                                </div>
                                                <div class="alert alert-danger">
                                                    <asp:Label ID="fileshareTabError" CssClass="bg-danger" runat="server" Visible="false"></asp:Label>
                                                </div>
                                            </div>
                                            <div>
                                                <div class="form-group">
                                                    <label for="fileShareServerName">Fileshare Server Name</label>
                                                    <asp:TextBox ID="fileShareServerName" CssClass="form-control ms-uppercase" runat="server"></asp:TextBox>
                                                </div>
                                                <div class="form-group">
                                                    <label for="fileShareName">File share name</label>
                                                    <asp:TextBox ID="fileShareName" CssClass="form-control ms-uppercase" runat="server"></asp:TextBox>
                                                </div>
                                                <asp:Button OnClick="FileShareBind_Click" ID="fileShareBind" Text="Get data" runat="server" />
                                            </div>
                                            <div class="card">
                                                <div class="card-body">
                                                    <div class="row">
                                                        <div class="col-md-8">
                                                            <div class="well well-sm">
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
                                                            </div>
                                                        </div>
                                                        <%--                                                        <div class="col-md-4">
                                                            <asp:Button CssClass="btn btn-primary btn-lg btn-block" ID="createRandomFile" Visible="true" runat="server" Text="Create Random File (Test)" OnClick="CreateRandomFile_Click" />
                                                        </div>--%>
                                                    </div>
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
                                                    <div class="row">
                                                        <div class="col-md-12">
                                                            <h3 class="ms-webpart-titleText">Folders</h3>
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
                                                        </div>
                                                    </div>
                                                    <div class="row">
                                                        <div class="col-md-12">
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
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                        </ContentTemplate>
                                    </asp:UpdatePanel>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </form>
</asp:Content>
