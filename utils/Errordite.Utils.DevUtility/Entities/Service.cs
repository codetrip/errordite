
using System;

namespace Errordite.Utils.DevUtility.Entities
{
    public class Service
    {
        public string Name { get; set; }
        public int? ProcessId { get; set; }
        public string Repository { get; set; }
        public string Status { get; set; }
        public string Executable { get; set; }
    }
}
