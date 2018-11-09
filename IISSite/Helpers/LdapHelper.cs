using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using IISSite.Models;

namespace IISSite.Helpers
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
    }
}