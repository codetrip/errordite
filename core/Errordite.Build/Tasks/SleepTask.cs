using System.Threading;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Errordite.Build.Tasks
{
    public class Sleep : Task
    {
        public override bool Execute()
        {
            Thread.Sleep(Timeout);
            return true;
        }

        [Required]
        public int Timeout { get; set; }
    }
}
