using Lab1.Characters;
using System;

namespace Lab1.Items
{
    public class ManaPotion : IConsumable
    {
        public string Name
        {
            get { return "Mana Potion"; }
        }

        public int Weight
        {
            get { return 2; }
        }

        public void Use(Character target)
        {
            if (!(target is Mage))
            {
                Console.WriteLine($"{target.Name} ne moze koristiti mana napitak.");
                return;
            }

            target.RestoreMana(25);
        }
    }
}