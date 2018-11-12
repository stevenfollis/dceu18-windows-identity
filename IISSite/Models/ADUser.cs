using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace IISSite.Models
{
    public class ADUser
    {
        public string UserName { get; set; }
        public string UserPrincipalName { get; set; }
        public string ParentDomain { get; set; }
        public SecurityIdentifier Sid { get; internal set; }
        public string SurName { get; internal set; }
        public string FirstName { get; internal set; }
        public string EmailAddress { get; internal set; }
        public string DisplayName { get; internal set; }
        public string DistinguishedName { get; internal set; }
        public string SamAccountName { get; internal set; }
        public Forest ParentForest { get; internal set; }
    }
}