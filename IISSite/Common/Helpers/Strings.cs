using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IISSite.Common.Helpers
{
    public static class Strings
    {
        public static string PRIV_QNAME = @".\private$\msmqtesterq";
        public static string PUB_QNAME = @".\msmqtestpubq";
        public static string QLABEL = @"{0}-APP";
    }
}