using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexConnect.Shared.MasterList
{
    [Serializable]
    public class ServerInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Realmlist { get; set; }
    }
}
