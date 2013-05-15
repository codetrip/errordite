
using System;

namespace Errordite.Utils.DevUtility.Entities
{
    public class Repository
    {
        public string LocalLocation { get; set; }
        public string ParentRepo { get; set; }
        public string Name { get; set; }
        public bool HasDbBackup { get; set; }
        public bool CurrentForIis { get; set; }
        public bool ContainsThisApp { get; set; }
    }
}
