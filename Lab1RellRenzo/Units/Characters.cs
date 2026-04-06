using System;
using System.Collections.Generic;
using Lab1.Items;
using Lab1.Weapons;

namespace Lab1.Characters
{
    public abstract class Character
    {
        protected static readonly Random Random = new Random();

        public string Name { get; }
        public int MaxHp { get; }
        public int CurrentHp { get; private set; }
        public int BaseDamage { get; }
        public int MaxMana { get; }
        public int CurrentMana { get; private set; }
        public Lab1.Inventory.Inventory Inventory { get; }
        public Weapon EquippedWeapon { get; private set; }
        public bool IsAlive
        {
            get { return CurrentHp > 0; }
        }

        private int bonusDamage;
        private int bonusDamageTurnsLeft;

        protected Character(string name, int maxHp, int baseDamage, int maxMana, int inventoryWeightLimit)
        {
            Name = name;
            MaxHp = maxHp;
            CurrentHp = maxHp;
            BaseDamage = baseDamage;
            MaxMana = maxMana;
            CurrentMana = maxMana;
            Inventory = new Lab1.Inventory.Inventory(inventoryWeightLimit);
            EquippedWeapon = new Weapon("No weapon", 0, 0);
        }

        public bool AddItem(IItem item)
        {
            return Inventory.AddItem(item);
        }

        public bool RemoveItem(IItem item)
        {
            return Inventory.RemoveItem(item);
        }

        public void EquipWeapon(Weapon weapon)
        {
            EquippedWeapon = weapon;
            Console.WriteLine($"{Name} equipped {weapon.Name} (+{weapon.DamageBonus} damage bonus).");
        }

        public int GetWeaponBonusDamage()
        {
            return EquippedWeapon.DamageBonus;
        }

        public int BuildDamage(int rolledBaseDamage)
        {
            return Math.Max(1, rolledBaseDamage + GetWeaponBonusDamage() + bonusDamage);
        }

        public void EndTurn()
        {
            if (bonusDamageTurnsLeft <= 0)
            {
                return;
            }

            bonusDamageTurnsLeft--;
            if (bonusDamageTurnsLeft == 0)
            {
                bonusDamage = 0;
                Console.WriteLine($"{Name} bonus damage ended.");
            }
        }

        public void ApplyDamageBuff(int amount, int turns)
        {
            bonusDamage = amount;
            bonusDamageTurnsLeft = turns;
            Console.WriteLine($"{Name} recieves +{amount} damage, on {turns} turn.");
        }

        public void TakeDamage(int amount)
        {
            CurrentHp = Math.Max(0, CurrentHp - amount);
            Console.WriteLine($"{Name} takes {amount} damage. HP: {CurrentHp}/{MaxHp}");
        }

        public void Heal(int amount)
        {
            CurrentHp = Math.Min(MaxHp, CurrentHp + amount);
            Console.WriteLine($"{Name} heals for {amount}. HP: {CurrentHp}/{MaxHp}");
        }

        public void RestoreMana(int amount)
        {
            if (MaxMana == 0)
            {
                Console.WriteLine($"{Name} doesnt use mana.");
                return;
            }

            CurrentMana = Math.Min(MaxMana, CurrentMana + amount);
            Console.WriteLine($"{Name} restores {amount} mana. Mana: {CurrentMana}/{MaxMana}");
        }

        protected bool SpendMana(int amount)
        {
            if (CurrentMana < amount)
            {
                Console.WriteLine($"{Name} not enough mana.");
                return false;
            }

            CurrentMana -= amount;
            return true;
        }
    }

    public class Warrior : Character
    {
        private readonly int specialChargeRequired;
        private int chargedDamage;

        public Warrior(string name = "Warrior")
            : base(name, maxHp: 140, baseDamage: 12, maxMana: 0, inventoryWeightLimit: 10)
        {
            specialChargeRequired = 35;
            chargedDamage = 0;
        }

        public int Slash(Enemy target)
        {
            int rolledDamage = BaseDamage + Random.Next(-2, 3);
            int damage = BuildDamage(rolledDamage);
            Console.WriteLine($"{Name} uses Slash on {target.Name} for {damage} damage.");
            target.TakeDamage(damage);
            chargedDamage += damage;
            return damage;
        }

        public bool IsGroupSlashReady()
        {
            return chargedDamage >= specialChargeRequired;
        }

        public void GroupSlash(List<Enemy> enemies)
        {
            if (!IsGroupSlashReady())
            {
                Console.WriteLine($"{Name}'s special is not ready yet.");
                return;
            }

            Console.WriteLine($"{Name} uses Group Slash!");
            foreach (var enemy in enemies)
            {
                if (!enemy.IsAlive)
                {
                    continue;
                }

                int rolledDamage = 9 + Random.Next(-2, 3);
                int damage = BuildDamage(rolledDamage);
                Console.WriteLine($" - {enemy.Name} takes {damage} damage.");
                enemy.TakeDamage(damage);
            }

            chargedDamage = 0;
        }
    }

    public class Mage : Character
    {
        public Mage(string name = "Mage")
            : base(name, maxHp: 85, baseDamage: 6, maxMana: 60, inventoryWeightLimit: 15)
        {
        }

        public void Zap(Enemy target)
        {
            const int manaCost = 8;
            if (!SpendMana(manaCost))
            {
                return;
            }

            int rolledDamage = BaseDamage + Random.Next(-2, 3);
            int damage = BuildDamage(rolledDamage);
            Console.WriteLine($"{Name} uses Zap on {target.Name} for {damage} damage.");
            target.TakeDamage(damage);

            if (Random.Next(0, 3) == 0)
            {
                target.ApplyStun(1);
                Console.WriteLine($"{target.Name} is stunned.");
            }
        }

        public void HealAlly(Character ally)
        {
            const int manaCost = 12;
            if (!SpendMana(manaCost))
            {
                return;
            }

            Console.WriteLine($"{Name} uses Heal on {ally.Name}.");
            ally.Heal(20);
        }
    }

    public class Ranger : Character
    {
        public Ranger(string name = "Ranger")
            : base(name, maxHp: 100, baseDamage: 9, maxMana: 0, inventoryWeightLimit: 12)
        {
        }

        public void ArrowShot(Enemy target)
        {
            int rolledDamage = BaseDamage + Random.Next(-2, 3);
            int damage = BuildDamage(rolledDamage);
            Console.WriteLine($"{Name} shoots {target.Name} for {damage} damage.");
            target.TakeDamage(damage);
        }

        public void FocusedShot(Enemy target)
        {
            int rolledDamage = BaseDamage + 4 + Random.Next(-2, 3);
            int damage = BuildDamage(rolledDamage);
            Console.WriteLine($"{Name} uses Focused Shot on {target.Name} for {damage} damage.");
            target.TakeDamage(damage);
        }
    }
}