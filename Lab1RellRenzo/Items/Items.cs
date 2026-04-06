using Lab1.Characters;

namespace Lab1.Items
{
    public interface IItem
    {
        string Name { get; }
        int Weight { get; }
    }

    public interface IConsumable : IItem
    {
        void Use(Character target);
    }
}