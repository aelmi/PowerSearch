using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerSearch
{
    public class AppFilter
    {
        public List<string> FileExtension = new List<string>();
        public List<string> ExcludeExtension = new List<string>();
        public List<string> Folder = new List<string>();
        public bool IncludeSubfolder = false;
        public List<string> ExcludeSubfolder = new List<string>();

    }
}
