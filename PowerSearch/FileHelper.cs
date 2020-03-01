using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerSearch
{
    public static class FileHelper
    {
        public static string[] getFilesInFolder(string Foldername, bool IncludeSubfolder = false)
        {
            try
            {
                if (IncludeSubfolder)
                    return Directory.GetFiles(Foldername, "*", SearchOption.AllDirectories);
                else
                    return Directory.GetFiles(Foldername);
            }
            catch (Exception ex)
            {
                throw; // ex;
            }
        }

        public static string[] getSubfolder(string Foldername, bool IncludeSubfolder = false)
        {
            try
            {
                if (IncludeSubfolder)
                    return Directory.GetDirectories(Foldername, "*", SearchOption.AllDirectories);
                else
                    return Directory.GetDirectories(Foldername);
            }
            catch (Exception ex)
            {
                throw; // ex;
            }
        }


    }
}
