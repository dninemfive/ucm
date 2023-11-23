using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace d9.ucm;
public static class Constants
{
    public static class Folders
    {
        public static string TEMP_Base => "C:/Users/dninemfive/Pictures/misc/ucm";
        public static string TEMP_Data => Path.Join(TEMP_Base, "data");
        public static string TEMP_Competitions => Path.Join(TEMP_Base, "competitions");
        public static string TEMP_Cache => Path.Join(TEMP_Base, "cache");
        public static string TEMP_Rules => @"C:\Users\dninemfive\Documents\workspaces\misc\ucm\common\urlrule";
        public static string TEMP_Sources => Path.Join(TEMP_Data, "sources");
    }
    public static class Files
    {
        public static string TEMP_Log => Path.Join(Folders.TEMP_Base, "general.log");
        public static string TEMP_RejectedHashes => Path.Join(Folders.TEMP_Base, "rejected hashes.txt");
        public static string TEMP_TagsToSkip => Path.Join(Folders.TEMP_Base, "tags_to_skip.txt");
        public static string TEMP_PerceptualHashes => Path.Join(Folders.TEMP_Base, "perceptualHashes.json");
    }
    public static int DefaultItemsPerPage => 36;
}
