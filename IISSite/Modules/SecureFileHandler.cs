using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Security.Principal;
using System.Text;
using System.Configuration;
using System.Web.Configuration;
using System.Security.Claims;
using System.Globalization;
using System.IdentityModel.Tokens;
using IISSite.Models;
using IISSite.Helpers;

namespace IISSite.Modules
{
    /// <summary>
    /// 
    /// </summary>
    public class SecureFileHandler : IHttpHandler
    {
        string fileShareRootPath = string.Empty;
        string fileShareVirtualDirectory = string.Empty;
        string decodedVal = string.Empty;
        string fileShareConfigId = string.Empty;
        string fullFileSharePath = string.Empty;
        //FileShareConfig curFileShareConfig = null;
        //SecureFileShareConfigSection fileShareConfigs = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="SecureFileHandler"/> class.
        /// </summary>
        public SecureFileHandler()
        {
 
        }


        protected void OnInit()
        {
            //if its a user, then ensure its a windows user, else, kick them out
            if (HttpContext.Current.User.Identity.IsAuthenticated)
            {
                HttpContext.Current.Response.AddHeader("Pragma", "No-Cache");
                HttpContext.Current.Response.CacheControl = "Private";
                HttpContext.Current.Response.AppendHeader("Access-Control-Allow-Origin", "*");
                HttpContext.Current.Response.AppendHeader("Access-Control-Allow-Methods", "GET,PUT,POST,DELETE");
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 401;
                HttpContext.Current.Response.Write("Access denied. No token");
            }
        }

        #region IHttpHandler Members

        /// <summary>
        /// Gets a value indicating whether another request can 
        /// use the <see cref="T:System.Web.IHttpHandler"></see> instance.
        /// </summary>
        /// <value></value>
        /// <returns>true if the <see cref="T:System.Web.IHttpHandler"></see>
        /// instance is reusable; otherwise, false.</returns>
        public bool IsReusable
        {
            get { return false; }
        }

        /// <summary>
        /// Enables processing of HTTP Web requests by a custom HttpHandler
        /// that implements the <see cref="T:System.Web.IHttpHandler"></see> interface.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpContext"></see>
        /// object that provides references to the intrinsic server objects 
        /// (for example, Request, Response, Session, and Server) 
        /// used to service HTTP requests.</param>
        public void ProcessRequest(HttpContext context)
        {
            try
            {
                //get the url requested by the user
                string urlRequested =  HttpUtility.UrlDecode(context.Request.Url.AbsolutePath.ToUpper());
                //Intercept anything that starts with FS for FileShare after the domain.tld
                string fileShareHandlerPath = HttpContext.Current.Request.ApplicationPath + "/FS";
                if (urlRequested.StartsWith(fileShareHandlerPath.ToUpper(), StringComparison.OrdinalIgnoreCase))
                {
                    string parsedUrl = urlRequested.ToUpper().Replace(fileShareHandlerPath.ToUpper(), "");
                    string fsPath = parsedUrl.TrimStart(new char[]{'\\'}).Replace("/", @"\");
                    if (fsPath != null)
                    {
                        SendFile(context, $@"\\{fsPath}");
                    }
                }
            }
            catch(HttpException)
            {
                //Something happened during the transmission of the file or
                //Something happened trying to fetch the file
                context.Response.Clear();
                context.Response.ClearContent();
                context.Response.ClearHeaders();
                context.Response.StatusCode = 500;
                context.Response.Write("An error occured trying to serve the file.");
            }
            catch (Exception ex)
            {
                context.Response.Clear();
                context.Response.ClearContent();
                context.Response.ClearHeaders();
                context.Response.StatusCode = 500;
                context.Response.Write("An error occured trying to serve the file." + ex.ToString());
            }
        }

        private void SendFile(HttpContext context, string filePath)
        {
            try
            {
                ADUser currentUser = LdapHelper.GetAdUser(context.User.Identity.Name);
                if (currentUser != null)
                {
                    ////Start impersonating
                    WindowsIdentity wi = new WindowsIdentity(currentUser.UserPrincipalName);
                    using (WindowsImpersonationContext wCtx = wi.Impersonate())
                    {
                        try
                        {
                            string fileUrl = makeFullFileUrl(filePath);
                            string fileName = Path.GetFileName(fileUrl);
                            FileInfo fileInfo = new FileInfo(fileUrl);
                            context.Response.Clear();
                            context.Response.ClearContent();
                            context.Response.ClearHeaders();
                            context.Response.AddHeader("Content-Disposition", "attachment; filename=" + fileName);
                            // bypass the Open/Save/Cancel dialog
                            //Response.AddHeader("Content-Disposition", "inline; filename=" + doc.FileName);
                            context.Response.AddHeader("Content-Length", fileInfo.Length.ToString());
                            // Set the ContentType
                            context.Response.ContentType = MimeMapping.GetMimeMapping(fileName);
                            context.Response.BufferOutput = true;
                            //var file = File.ReadAllBytes(fileUrl);
                            context.Response.WriteFile(fileUrl, true);
                            context.Response.Flush();
                            context.ApplicationInstance.CompleteRequest();
                        }
                        catch (System.UnauthorizedAccessException)
                        {
                            //The current user does not have rights, give them access denied
                            context.Response.Clear();
                            context.Response.ClearContent();
                            context.Response.ClearHeaders();
                            context.Response.StatusCode = 401;
                            context.Response.Write("Access denied. You do not have access to this file.");
                        }
                        catch (Exception ex)
                        {
                            //Something happened trying to fetch the file
                            context.Response.Clear();
                            context.Response.ClearContent();
                            context.Response.ClearHeaders();
                            context.Response.StatusCode = 500;
                            context.Response.Write($"An error occured trying to serve the file. {ex.ToString()}");
                        }
                        finally
                        {
                            if (wCtx != null)
                                wCtx.Undo();
                        }
                    }
                }
            }
            catch (System.Security.SecurityException)
            {
                //The current user is not a synced account, the UPN is invalid
                context.Response.Clear();
                context.Response.ClearContent();
                context.Response.ClearHeaders();
                context.Response.StatusCode = 401;
                context.Response.Write("Access denied. Not a valid user.");
            }
        }

        private string makeFullFileUrl(string filePath)
        {
            string retUrl = filePath;
            return retUrl.Replace('/', '\\').ToString();
        }


 

        #endregion
    }
}