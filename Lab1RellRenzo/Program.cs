using Lab1.Characters;
using Lab1.Items;
using Lab1.Weapons;

Random random = new Random();

// Likovi
Warrior warrior = new Warrior("Warrior");
Mage mage = new Mage("Mage");
Ranger ranger = new Ranger("Archer");

List<Character> heroes = new List<Character>();
heroes.Add(warrior);
heroes.Add(mage);
heroes.Add(ranger);

// Weapons
Weapon silverSword = new Weapon("Silver Sword", 5, 6);
Weapon oakStaff = new Weapon("Oak Staff", 2, 4);
Weapon hunterBow = new Weapon("Hunter Bow", 3, 5);

warrior.AddItem(silverSword);
mage.AddItem(oakStaff);
ranger.AddItem(hunterBow);

warrior.EquipWeapon(silverSword);
mage.EquipWeapon(oakStaff);
ranger.EquipWeapon(hunterBow);

// Poitions
warrior.AddItem(new DamagePotion());
warrior.AddItem(new HealingPotion());
mage.AddItem(new ManaPotion());
ranger.AddItem(new HealingPotion());

// Enemies sa random generacijom kolicine
List<Enemy> enemyTemplates = new List<Enemy>();
enemyTemplates.Add(new Enemy("Skeleton", 60, 10));
enemyTemplates.Add(new Enemy("Orc", 100, 20));
enemyTemplates.Add(new Enemy("Goblin", 40, 6));

int enemyCount = random.Next(1, 4);
List<Enemy> enemies = new List<Enemy>();
for (int i = 0; i < enemyCount; i++)
{
    Enemy template = enemyTemplates[random.Next(enemyTemplates.Count)];
    enemies.Add(new Enemy(template.Name, template.MaxHp, template.Damage));
}

Console.Clear();
Console.WriteLine("Battle start");
Console.WriteLine("1 = Attack, 2 = Special, 3 = Items");
Console.WriteLine("Enter to start");
Console.ReadLine();
Console.Clear();

int round = 0;
while (AnyHeroAlive(heroes) && AnyEnemyAlive(enemies))
{
    round++;

    // Potez igraca: svaki zivi heroj igra.
    for (int i = 0; i < heroes.Count; i++)
    {
        Character hero = heroes[i];
        if (!hero.IsAlive)
        {
            continue;
        }

        DrawBattleHeader(round, heroes, enemies);
        Console.WriteLine(hero.Name + "'s turn");
        Console.WriteLine("1 - Attack");
        Console.WriteLine("2 - Special");
        Console.WriteLine("3 - Items");

        int choice = ReadMenuChoice(1, 3);

        if (hero is Warrior)
        {
            Warrior w = (Warrior)hero;
            if (choice == 1)
            {
                Enemy? target = PickAliveEnemy(enemies);
                if (target != null)
                {
                    w.Slash(target);
                }
            }
            else if (choice == 2)
            {
                w.GroupSlash(enemies);
            }
            else
            {
                UsePotionFromInventory(hero);
            }
        }
        else if (hero is Mage)
        {
            Mage m = (Mage)hero;
            if (choice == 1)
            {
                Enemy? target = PickAliveEnemy(enemies);
                if (target != null)
                {
                    m.Zap(target);
                }
            }
            else if (choice == 2)
            {
                Character? ally = PickAliveAlly(heroes);
                if (ally != null)
                {
                    m.HealAlly(ally);
                }
            }
            else
            {
                UsePotionFromInventory(hero);
            }
        }
        else if (hero is Ranger)
        {
            Ranger r = (Ranger)hero;
            if (choice == 1)
            {
                Enemy? target = PickAliveEnemy(enemies);
                if (target != null)
                {
                    r.ArrowShot(target);
                }
            }
            else if (choice == 2)
            {
                Enemy? target = PickAliveEnemy(enemies);
                if (target != null)
                {
                    r.FocusedShot(target);
                }
            }
            else
            {
                UsePotionFromInventory(hero);
            }
        }

        if (!AnyEnemyAlive(enemies))
        {
            PauseAndClear("Victory");
            break;
        }

        PauseAndClear("Turn ended");
    }

    if (!AnyEnemyAlive(enemies))
    {
        break;
    }

    // Enemy potez
    DrawBattleHeader(round, heroes, enemies);
    Console.WriteLine("Enemy turn");

    for (int i = 0; i < enemies.Count; i++)
    {
        Enemy enemy = enemies[i];
        if (!enemy.IsAlive)
        {
            continue;
        }

        if (enemy.IsStunned())
        {
            Console.WriteLine(enemy.Name + " was stunned");
            enemy.EndTurn();
            continue;
        }

        List<Character> aliveHeroes = GetAliveHeroes(heroes);
        if (aliveHeroes.Count == 0)
        {
            break;
        }

        Character target = aliveHeroes[random.Next(aliveHeroes.Count)];
        Console.WriteLine(enemy.Name + " attacks " + target.Name + " for " + enemy.Damage + " damage.");
        target.TakeDamage(enemy.Damage);
        enemy.EndTurn();
    }

    for (int i = 0; i < heroes.Count; i++)
    {
        heroes[i].EndTurn();
    }

    if (!AnyHeroAlive(heroes))
    {
        PauseAndClear("Defeat");
        break;
    }

    PauseAndClear("End of turn" + round);
}

Console.Clear();
Console.WriteLine("End of battle");
for (int i = 0; i < heroes.Count; i++)
{
    Character hero = heroes[i];
    string status = "defeated";
    if (hero.IsAlive)
    {
        status = "alive";
    }

    Console.WriteLine(hero.Name + " (" + status + ") HP " + hero.CurrentHp + "/" + hero.MaxHp + ", Mana " + hero.CurrentMana + "/" + hero.MaxMana);
}

static bool AnyHeroAlive(List<Character> heroes)
{
    for (int i = 0; i < heroes.Count; i++)
    {
        if (heroes[i].IsAlive)
        {
            return true;
        }
    }
    return false;
}

static bool AnyEnemyAlive(List<Enemy> enemies)
{
    for (int i = 0; i < enemies.Count; i++)
    {
        if (enemies[i].IsAlive)
        {
            return true;
        }
    }
    return false;
}

static List<Character> GetAliveHeroes(List<Character> heroes)
{
    List<Character> result = new List<Character>();
    for (int i = 0; i < heroes.Count; i++)
    {
        if (heroes[i].IsAlive)
        {
            result.Add(heroes[i]);
        }
    }
    return result;
}

static void DrawBattleHeader(int round, List<Character> heroes, List<Enemy> enemies)
{
    Console.WriteLine("Round " + round);
    Console.WriteLine("Team:");
    for (int i = 0; i < heroes.Count; i++)
    {
        Character h = heroes[i];
        if (h.IsAlive)
        {
            Console.WriteLine("- " + h.Name + " HP " + h.CurrentHp + "/" + h.MaxHp + " Mana " + h.CurrentMana + "/" + h.MaxMana);
        }
        else
        {
            Console.WriteLine("- " + h.Name + " (defeated)");
        }
    }

    Console.WriteLine("Enemies:");
    List<Enemy> aliveEnemies = new List<Enemy>();
    for (int i = 0; i < enemies.Count; i++)
    {
        if (enemies[i].IsAlive)
        {
            aliveEnemies.Add(enemies[i]);
        }
    }

    if (aliveEnemies.Count == 0)
    {
        Console.WriteLine("- done");
    }
    else
    {
        for (int i = 0; i < aliveEnemies.Count; i++)
        {
            Enemy e = aliveEnemies[i];
            string stun = "";
            if (e.IsStunned())
            {
                stun = " [OMAMLJEN]";
            }
            Console.WriteLine((i + 1) + ". " + e.Name + " HP " + e.CurrentHp + "/" + e.MaxHp + stun);
        }
    }

    Console.WriteLine();
}

static int ReadMenuChoice(int min, int max)
{
    while (true)
    {
        Console.Write("Type 1 to 3 for selection (" + min + "-" + max + "): ");
        string? input = Console.ReadLine();
        int number;
        bool ok = int.TryParse(input, out number);
        if (ok && number >= min && number <= max)
        {
            return number;
        }
        Console.WriteLine("Invalid input.");
    }
}

static Enemy? PickAliveEnemy(List<Enemy> enemies)
{
    List<Enemy> alive = new List<Enemy>();
    for (int i = 0; i < enemies.Count; i++)
    {
        if (enemies[i].IsAlive)
        {
            alive.Add(enemies[i]);
        }
    }

    if (alive.Count == 0)
    {
        return null;
    }

    if (alive.Count == 1)
    {
        return alive[0];
    }

    Console.WriteLine("Pick enemies:");
    for (int i = 0; i < alive.Count; i++)
    {
        Console.WriteLine((i + 1) + ". " + alive[i].Name + " HP " + alive[i].CurrentHp + "/" + alive[i].MaxHp);
    }
    int idx = ReadMenuChoice(1, alive.Count);
    return alive[idx - 1];
}

static Character? PickAliveAlly(List<Character> heroes)
{
    List<Character> alive = new List<Character>();
    for (int i = 0; i < heroes.Count; i++)
    {
        if (heroes[i].IsAlive)
        {
            alive.Add(heroes[i]);
        }
    }

    if (alive.Count == 0)
    {
        return null;
    }

    if (alive.Count == 1)
    {
        return alive[0];
    }

    Console.WriteLine("Pick heros:");
    for (int i = 0; i < alive.Count; i++)
    {
        Console.WriteLine((i + 1) + ". " + alive[i].Name + " HP " + alive[i].CurrentHp + "/" + alive[i].MaxHp);
    }
    int idx = ReadMenuChoice(1, alive.Count);
    return alive[idx - 1];
}

static List<IConsumable> GetConsumables(Character hero)
{
    List<IConsumable> result = new List<IConsumable>();
    List<IItem> items = hero.Inventory.GetItems();
    for (int i = 0; i < items.Count; i++)
    {
        IItem item = items[i];
        if (item is IConsumable)
        {
            IConsumable potion = (IConsumable)item;
            result.Add(potion);
        }
    }
    return result;
}

static void UsePotionFromInventory(Character hero)
{
    List<IConsumable> potions = GetConsumables(hero);
    if (potions.Count == 0)
    {
        Console.WriteLine(hero.Name + " no items in inventory");
        return;
    }

    Console.WriteLine("Pick items:");
    for (int i = 0; i < potions.Count; i++)
    {
        Console.WriteLine((i + 1) + ". " + potions[i].Name);
    }
    int idx = ReadMenuChoice(1, potions.Count);
    IConsumable potionToUse = potions[idx - 1];
    potionToUse.Use(hero);
    hero.RemoveItem(potionToUse);
}

static void PauseAndClear(string message)
{
    Console.WriteLine();
    Console.WriteLine(message);
    Console.WriteLine("Press enter to continue");
    Console.ReadLine();
    Console.Clear();
}
