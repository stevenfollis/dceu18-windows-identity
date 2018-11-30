using System.DirectoryServices.ActiveDirectory;
using System.Security.Principal;

namespace IISSite.Common.Models
{
    public class ADUser
    {
        public string UserName { get; set; }
        public string UserPrincipalName { get; set; }
        public string ParentDomain { get; set; }
        public SecurityIdentifier Sid { get; set; }
        public string SurName { get; set; }
        public string FirstName { get; set; }
        public string EmailAddress { get; set; }
        public string DisplayName { get; set; }
        public string DistinguishedName { get; set; }
        public string SamAccountName { get; set; }
        public Forest ParentForest { get; set; }
        public string ServicePrincipalNames { get; set; }
        public string AllowedToDelegateTo { get; set; }
        public string UserAccountControl { get; set; }
    }
}