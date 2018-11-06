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
using FileShareBrowserWeb;

namespace FileShareBrowserWeb.Modules
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
        FileShareConfig curFileShareConfig = null;
        SecureFileShareConfigSection fileShareConfigs = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="SecureFileHandler"/> class.
        /// </summary>
        public SecureFileHandler()
        {
 
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
                if (urlRequested.StartsWith("/FS"))
                {
                    string[] parsedUrl = urlRequested.Trim('/').Split('/');
                    string fsId = parsedUrl[1]; //Grab the file share ID to map it
                    string fsPath = urlRequested.Replace("/FS/", "").Replace(fsId, "");
                    if (fsPath != null)
                    {
                        setFileShareConfigContext(context, fsId);
                        sendFile(context, fsPath);
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

        private void setFileShareConfigContext(HttpContext context, string fsId)
        {
            //Populate the file shares configured here
            fileShareConfigs = (SecureFileShareConfigSection)WebConfigurationManager.GetSection("SecureFileShares");

            //Get the correct config from the web.config based on the ID
            curFileShareConfig = fileShareConfigs.SecureFileShareConfigs[fsId] as FileShareConfig;
            if (curFileShareConfig == null)
            {
                throw new ConfigurationErrorsException(string.Format("The file share id {} was not found.", fsId));
            }
        }

        private void sendFile(HttpContext context, string filePath)
        {
            ClaimsPrincipal currentClaims = ClaimsPrincipal.Current;
            Claim userUpn = currentClaims.Claims.FirstOrDefault(c => c.Type.ToString(CultureInfo.InvariantCulture) == ClaimTypes.Upn);
            if (userUpn != null)
            {
                try
                {
                    WindowsIdentity wi = new WindowsIdentity(userUpn.Value);
                    //Start impersonating
                    using (WindowsImpersonationContext wCtx = wi.Impersonate())
                    {
                        string fileUrl = makeFullFileUrl(filePath);
                        try
                        {
                            string fileName = System.IO.Path.GetFileName(fileUrl);
                            FileInfo fileInfo = new FileInfo(fileUrl);
                            context.Response.Clear();
                            context.Response.ClearContent();
                            context.Response.ClearHeaders();
                            context.Response.BufferOutput = true;
                            context.Response.AddHeader("Content-Disposition", "attachment; filename=" + fileName);
                            // bypass the Open/Save/Cancel dialog
                            //Response.AddHeader("Content-Disposition", "inline; filename=" + doc.FileName);

                            context.Response.AddHeader("Content-Length", fileInfo.Length.ToString());
                            // Set the ContentType
                            context.Response.ContentType = System.Web.MimeMapping.GetMimeMapping(fileName);
                            context.Response.TransmitFile(fileUrl);
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
                        catch
                        {
                            //Something happened trying to fetch the file
                            context.Response.Clear();
                            context.Response.ClearContent();
                            context.Response.ClearHeaders();
                            context.Response.StatusCode = 500;
                            context.Response.Write("An error occured trying to serve the file.");
                        }
                        finally
                        {
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
        }

        private string makeFullFileUrl(string filePath)
        {
            string retUrl = curFileShareConfig.ActualPath + filePath;
            return retUrl.Replace('/', '\\').ToString();
        }


 

        #endregion
    }
}