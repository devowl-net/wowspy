using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace WowDotNetAPI.Models
{
    [DataContract]
    public class CharacterPetSlot
    {
        [DataMember(Name = "abilities")]
        public IEnumerable<int> Abilities { get; set; }

        [DataMember(Name = "battlePetGuid")]
        public string BattlePetGuid { get; set; }

        [DataMember(Name = "isEmpty")]
        public bool IsEmpty { get; set; }

        [DataMember(Name = "isLocked")]
        public bool IsLocked { get; set; }

        [DataMember(Name = "slot")]
        public int Slot { get; set; }

        public override bool Equals(object obj)
        {
            CharacterPetSlot another = (CharacterPetSlot) obj;

            if (another.IsEmpty && IsEmpty)
            {
                return true;
            }

            return
                another.Slot == Slot &&
                another.IsLocked == IsLocked &&
                another.BattlePetGuid == BattlePetGuid &&
                another.Abilities.SequenceEqual(Abilities);
        }
    }
}
