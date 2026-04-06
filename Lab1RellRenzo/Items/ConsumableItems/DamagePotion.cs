using Lab1.Characters;

namespace Lab1.Items
{
    public class DamagePotion : IConsumable
    {
        public string Name
        {
            get { return "Damage Potion"; }
        }

        public int Weight
        {
            get { return 2; }
        }

        public void Use(Character target)
        {
            target.ApplyDamageBuff(5, 2);
        }
    }
}
