using Lab1.Characters;

namespace Lab1.Items
{
    public class HealingPotion : IConsumable
    {
        public string Name
        {
            get { return "Healing Potion"; }
        }

        public int Weight
        {
            get { return 2; }
        }

        public void Use(Character target)
        {
            target.Heal(25);
        }
    }
}