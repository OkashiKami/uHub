using System;
using System.Collections.Generic;
using uHub.Databse.Entities;
using uHub.Databse.Items;

namespace uHub.Databse
{
    public class Inventory
    {
        public  Entity player;
        public List<Item> items = new List<Item>();
        public Inventory()
        {
            // Load default player
            this.player = EntityDatabase.player;

            // Give Items
            Add(ItemDatabase.HealthPotion1, 64);
        }

        private void Add(Item newItem, int amount = 1)
        {
            for (int i = 0; i < amount; i++)
            {
                if (items.Count <= 0) { items.Add(newItem); }
                else
                {
                    for (int x = 0; x < items.Count; x++)
                    {
                        Item sourceItem = items[x];
                        if(newItem.id == sourceItem.id)
                        {
                            if(sourceItem.isStackable)
                            {
                                if(sourceItem.curStack < sourceItem.maxStack)
                                {
                                    sourceItem.curStack += 1;
                                    break;
                                }
                                else
                                {
                                    items.Add(newItem);
                                    break;
                                }
                            }
                            else
                            {
                                items.Add(newItem);
                                break; ;
                            }
                        }
                    }
                }
            }
            Program.Log("Adding {0} {1} Lv{2}", amount, newItem.name, newItem.level);
        }
    }
}