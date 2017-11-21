using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace WowDotNetAPI.Models
{
    [DataContract]
    [DebuggerDisplay("{GuildCharacter?.Name}-{GuildCharacter?.Realm}")]
    public class GuildMember
    {
        [DataMember(Name="character")]
        
        public GuildCharacter GuildCharacter { get; set; }

        [DataMember(Name = "rank")]
        public int Rank { get; set; }

        [DataMember]
        public Character FullCharactor { get; set; }

        public override string ToString()
        {
            return $"{GuildCharacter?.Name}-{GuildCharacter?.Realm}";
        }
    }
}
