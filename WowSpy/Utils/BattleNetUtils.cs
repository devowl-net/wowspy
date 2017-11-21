using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WowDotNetAPI.Models;

namespace WowSpy.Utils
{
    public class BattleNetUtils
    {
        public static bool IsEqualCharactors(Character checkPlayer, Character bannedPlayer)
        {
            if (string.Equals(checkPlayer.Name, bannedPlayer.Name, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var checkPets = checkPlayer.PetSlots;
            var bannedPets = bannedPlayer.PetSlots;
            if (checkPets.Length != bannedPets.Length)
            {
                return false;
            }

            for (int i = 0; i < checkPlayer.PetSlots.Length; i ++)
            {
                var checkPet = checkPlayer.PetSlots[i];
                var banPet = bannedPlayer.PetSlots[i];

                if (!Equals(checkPet, banPet))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
