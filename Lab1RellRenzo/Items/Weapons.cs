using Lab1.Items;

namespace Lab1.Weapons
{
    public class Weapon : IItem
    {
        public string Name { get; }
        public int Weight { get; }
        public int DamageBonus { get; }

        public Weapon(string name, int damageBonus, int weight)
        {
            Name = name;
            DamageBonus = damageBonus;
            Weight = weight;
        }
    }
}