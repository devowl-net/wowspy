using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WowSpy.Serialization
{
    [Serializable]
    public class Pet
    {
        public string Name { get; set; }

        public int Level { get; set; }


        public override bool Equals(object obj)
        {
            Pet pet1 = this, pet2 = (Pet)obj;
            if (ReferenceEquals(pet1, pet2))
            {
                return true;
            }

            return string.Equals(pet1.Name, pet2.Name, StringComparison.OrdinalIgnoreCase)
                && pet1.Level == pet2.Level;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }
}
