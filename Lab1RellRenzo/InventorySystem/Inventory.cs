using System;
using System.Collections.Generic;
using Lab1.Items;
using System.Linq;

namespace Lab1.Inventory
{
    public class Inventory
    {
        private readonly List<IItem> items = new List<IItem>();
        private int currentWeight = 0;
        public int MaxWeight { get; }
        public int CurrentWeight
        {
            get { return currentWeight; }
        }

        public Inventory(int maxWeight)
        {
            MaxWeight = maxWeight;
        }

        public bool AddItem(IItem item)
        {
            if (currentWeight + item.Weight > MaxWeight)
            {
                Console.WriteLine($"Cannot add item {item.Name} - weight limit reached {currentWeight}/{MaxWeight}");
                return false;
            }
            items.Add(item);
            currentWeight += item.Weight;
            Console.WriteLine($"Added item: {item.Name} (weight: {item.Weight})");
            return true;
        }

        public bool RemoveItem(IItem item)
        {
            if (items.Remove(item))
            {
                currentWeight -= item.Weight;
                Console.WriteLine($"Removed item: {item.Name} from inventory.");
                return true;
            }
            return false;
        }

        public List<IItem> GetItems()
        {
            return items.ToList();
        }
    }
}