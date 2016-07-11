using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace WowDotNetAPI.Models
{
    [DataContract]
    public enum UnitSide
    {
        Alliance = 0,
        Horde = 1
    }
    
    [DataContract]
    public class Guild
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "realm")]
        public string Realm { get; set; }

        [DataMember(Name = "battlegroup")]
        public string Battlegroup { get; set; }

        [DataMember(Name = "side")]
        private int side { get; set; }

        [DataMember(Name = "level")]
        public int Level { get; set; }

        [DataMember(Name = "achievementPoints")]
        public int AchievementPoints { get; set; }

        [DataMember(Name = "lastModified")]
        public long LastModified { get; set; }

        [DataMember(Name = "emblem")]
        public GuildEmblem Emblem { get; set; }

        [DataMember(Name = "members")]
        public GuildMember[] Members { get; set; }

        [DataMember(Name = "achievements")]
        public Achievements Achievements { get; set; }

        [DataMember(Name = "news")]
        public GuildNews[] News { get; set; }

        public UnitSide Side { get { return (UnitSide)Enum.Parse(typeof(UnitSide), Enum.GetName(typeof(UnitSide), side)); } }

        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ Realm.GetHashCode() ^ Battlegroup.GetHashCode() ^ side.GetHashCode() ^
                   AchievementPoints.GetHashCode();
        }
    }
}
