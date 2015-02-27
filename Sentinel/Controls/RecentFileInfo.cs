using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Sentinel.Controls
{
    [DataContract]
    public class RecentFileInfo
    {        
        [DataMember]
        public IEnumerable<string> RecentFilePaths { get; set; }
    }
}
