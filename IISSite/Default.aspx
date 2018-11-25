<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="IISSite.Default" MasterPageFile="~/Layout.Master" %>

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
                <h1 class="display-3">Hello <b>
                    <asp:Label ID="loginName" CssClass="ms-uppercase" runat="server"></asp:Label></b></h1>
                <p class="lead">
                    User <b><%= Request.LogonUserIdentity.Name %></b> authenticated on <b><%= Server.MachineName %></b><br />
                    using AuthenticationType:<b><%= Request.LogonUserIdentity.AuthenticationType %></b><br />
                    with impersonation level:<b> <%= Request.LogonUserIdentity.ImpersonationLevel %></b><br />
                    App Pool running as:<b> <%= System.Security.Principal.WindowsIdentity.GetCurrent().Name %></b><br />
                    From: <b>
                        <asp:Label ID="UserSourceLocation" CssClass="ms-uppercase" runat="server" /></b>
                </p>
            </div>
        </section>

        <div class="container-fluid">
             <div class="card-deck">
                <div class="card border-primary">
                    <h4 class="card-header bg-primary text-white"><i class="fas fa-ghost"></i> Anonymous</h4>
                    <div class="card-body">
                        <ul class="list-group list-group-flush">
                            <li class="list-group-item"><b>Page:</b> This page</li>
                            <li class="list-group-item"><b>Requires login:</b> NO</li>
                            <li class="list-group-item"><b>App Pool Account:</b> <br /><%= System.Security.Principal.WindowsIdentity.GetCurrent().Name %></li>
                            <li class="list-group-item"><b>Accessses back end resources:</b> None</li>
                        </ul>
                    </div>
                    <div class="card-footer">
                        <a class="btn btn-primary" href="/">GO</a>
                    </div>
                </div>

                <div class="card border-success">
                    <h4 class="card-header bg-success text-white">Forms</h4>
                    <div class="card-body">
                        <ul class="list-group list-group-flush">
                            <li class="list-group-item"><b>Page:</b> Forms</li>
                            <li class="list-group-item"><b>Requires login:</b> YES (forms user)</li>
                            <li class="list-group-item"><b>App Pool Account:</b> <br /><%= System.Security.Principal.WindowsIdentity.GetCurrent().Name %></li>
                            <li class="list-group-item"><b>Accessses back end resources:</b> YES</li>
                        </ul>
                    </div>
                    <div class="card-footer">
                        <a class="btn btn-primary" href="/secure/forms">GO</a>
                    </div>
                </div>

            
                <div class="card border-info">
                    <h4 class="card-header bg-info text-white">IWA</h4>
                    <div class="card-body">
                        <ul class="list-group list-group-flush">
                            <li class="list-group-item"><b>Page:</b> Windows</li>
                            <li class="list-group-item"><b>Requires login:</b> YES (Windows domain Account)</li>
                            <li class="list-group-item"><b>App Pool Account:</b> gMSA</li>
                            <li class="list-group-item"><b>Accessses back end resources:</b> YES as gMSA using IWA</li>
                        </ul>
                    </div>
                    <div class="card-footer">
                        <a class="btn btn-primary" href="http://dceu-windev-01.corp.winid.net/secure/iwa">GO</a>
                    </div>
                </div>

      
                <div class="card border-danger">
                    <h4 class="card-header bg-danger text-white">IWA+Delgation</h4>
                    <div class="card-body">
                        <ul class="list-group list-group-flush">
                            <li class="list-group-item"><b>Page:</b> Windows</li>
                            <li class="list-group-item"><b>Requires login:</b> YES (Windows domain Account)</li>
                            <li class="list-group-item"><b>App Pool Account:</b> gMSA</li>
                            <li class="list-group-item"><b>Accessses back end resources:</b> YES using Kerberos Delegaton as the User</li>
                        </ul>
                    </div>
                    <div class="card-footer">
                        <a class="btn btn-primary" href="http://dceu-windev-01.corp.winid.net/secure/iwa">GO</a>
                    </div>
                </div>
            </div>

        </div>
    </form>
</asp:Content>
