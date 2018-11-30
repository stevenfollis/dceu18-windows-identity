using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using System.Security.Principal;
using IISSite.Common.Models;

namespace IISSite.Common.Helpers
{
    public class LdapHelper
    {
        public static List<ADGroup> GetADGroups(IdentityReferenceCollection Groups)
        {
            List<ADGroup> aDGroups = new List<ADGroup>();
            try
            {
                foreach (IdentityReference g in Groups)
                {
                    string groupName = new System.Security.Principal.SecurityIdentifier(g.Value).Translate(typeof(System.Security.Principal.NTAccount)).ToString();
                    aDGroups.Add(new ADGroup()
                    {
                        Name = groupName,
                        DisplayName = g.Value
                    }
                    );
                }
            }
            catch(Exception ex)
            {
                //TODO: Log
            }

            return aDGroups;
        }

        public static ADUser GetAdUser(string userName)
        {
            ADUser adUser = null;

            try
            {
                // set up domain context
                PrincipalContext ctx = new PrincipalContext(ContextType.Domain);

                // find a user
                UserPrincipal user = UserPrincipal.FindByIdentity(ctx, userName);

                if (user != null)
                {
                    Domain userDomain = Domain.GetCurrentDomain();
                    adUser = new ADUser()
                    {
                        UserName = user.Name,
                        UserPrincipalName = user.UserPrincipalName,
                        Sid = user.Sid,
                        FirstName = user.GivenName,
                        SurName = user.Surname,
                        EmailAddress = user.EmailAddress,
                        DisplayName = user.DisplayName,
                        DistinguishedName = user.DistinguishedName,
                        SamAccountName = user.SamAccountName,
                        ParentDomain = userDomain.Name,
                        ParentForest = userDomain.Forest,
                    };
                }
            }
            catch(Exception ex)
            {
                
            }
            return adUser;
        }
    }
}