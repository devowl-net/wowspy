using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.ViewModel;

namespace WowSpy.Serialization
{
    [Serializable]
    public class Guild : BindableBase
    {
        public DateTime? LastUpdateTime { get; set; }
        public string GuildName { get; set; }

        public string ServerName { get; set; }

        public List<Player> Players { get; set; }

        public override string ToString()
        {
            return string.Format("[{0}-{1}] LastUpdate: [{2}]", 
                GuildName,
                ServerName,
                LastUpdateTime != null ? LastUpdateTime.ToString() : "?");
        }
    }
}
