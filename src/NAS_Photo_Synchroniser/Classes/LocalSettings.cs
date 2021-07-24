using System.Collections.Generic;

namespace NAS_Photo_Synchroniser.Classes
{
    public static class LocalSettings
    {
        public static string SourceFolder { get; set; }
        public static string DestinationFolder { get; set; }
        public static List<string> ExludedFolders { get; set; }
        public static List<string> AcceptedFiles { get; set; }
        public static bool ShouldDeleteFolders { get; set; }
    }
}
