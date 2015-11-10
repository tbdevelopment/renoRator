using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RenoRatorLibrary
{
    public class PhotoFunctions
    {
        public static string getUniqueFileName(string filename)
        {
            // create a guid
            Guid guid = Guid.NewGuid();
            // get the file extension
            string fileExt = filename;
            char[] a = fileExt.ToCharArray();
            // reverse the string
            Array.Reverse(a);
            fileExt = new string(a);
            // find the period and substring it
            int periodIndex = fileExt.IndexOf('.');
            fileExt = fileExt.Substring(0, periodIndex + 1);
            // reverse the string again
            a = fileExt.ToCharArray();
            Array.Reverse(a);
            fileExt = new string(a);

            // return the guid with the file extension attached
            return guid.ToString() + fileExt;
        }
    }
}
