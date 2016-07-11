using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.Mvvm;

namespace WowSpy.Serialization
{
    [Serializable]
    public class PlayerObj : BindableBase
    {
        public string PlayerName { get; set; }

        public string GuildName { get; set; }

        public int TotalPetsCount { get; set; }
        public List<PetObj> Pets { get; set; }

        public string ServerName { get; set; }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            PlayerObj p1 = this, p2 = (PlayerObj)obj;
            if (ReferenceEquals(p1, p2))
            {
                return true;
            }

            if (p1.Pets == null || p2.Pets == null || !p1.Pets.Any() || !p2.Pets.Any())
            {
                return false;
            }

            var dif = p1.Pets.Where(pet1 => p2.Pets.FirstOrDefault(pet2 => pet2.Equals(pet1)) != null);

            return p1.Pets.Count == p2.Pets.Count && dif.Count() == p1.Pets.Count;
        }

    }
}
