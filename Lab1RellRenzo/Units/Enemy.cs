using System;

namespace Lab1.Characters
{
    public class Enemy
    {
        public string Name { get; }
        public int MaxHp { get; }
        public int CurrentHp { get; private set; }
        public int Damage { get; }
        public bool IsAlive
        {
            get { return CurrentHp > 0; }
        }

        private int stunTurnsLeft;

        public Enemy(string name, int hp, int damage)
        {
            Name = name;
            MaxHp = hp;
            CurrentHp = hp;
            Damage = damage;
        }

        public void TakeDamage(int amount)
        {
            CurrentHp = Math.Max(0, CurrentHp - amount);
            Console.WriteLine($"{Name} HP: {CurrentHp}/{MaxHp}");
        }

        public void ApplyStun(int turns)
        {
            stunTurnsLeft = Math.Max(stunTurnsLeft, turns);
        }

        public bool IsStunned()
        {
            return stunTurnsLeft > 0;
        }

        public void EndTurn()
        {
            if (stunTurnsLeft > 0)
            {
                stunTurnsLeft--;
            }
        }
    }
}
