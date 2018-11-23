using System;

namespace IISSite.Common.Helpers
{
    public class FileShareHelper
    {
        public static string DisplayFileSizeFromBytes(long sizeInBytes)
        {
            string retVal = "";
            // KB (kbit) 2^10 -------------------------------------

            if (sizeInBytes > Math.Pow(2, 10) && sizeInBytes < Math.Pow(2, 20))
            {
                retVal = sizeInBytes / 1024 + "KB";
            }

            // MB (mebibit) 2^20 -----------------------------------
            if (sizeInBytes > Math.Pow(2, 20) && sizeInBytes < Math.Pow(2, 30))
            {
                retVal = sizeInBytes / 1048576 + "MB";
            }

            // GB (gibibit) 2^30 -----------------------------------
            if (sizeInBytes > Math.Pow(2, 30) && sizeInBytes < Math.Pow(2, 40))
            {
                retVal = sizeInBytes / 1073741824 + "GB";
            }
            // TB (tebibit) 2^40 -----------------------------------

            if (sizeInBytes > Math.Pow(2, 40) && sizeInBytes < Math.Pow(2, 50))
            {
                retVal = sizeInBytes / 1099511627776 + "TB";
            }

            // If bigger than 60MB change color to red --------------
            if (sizeInBytes > 600000)
            {
                retVal = "<font style='color: red;'>" + retVal + "</font>";
            }

            return retVal;
        }
       
    }
}