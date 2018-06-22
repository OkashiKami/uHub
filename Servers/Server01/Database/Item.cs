using System;

namespace uHub.Databse.Items
{
    public class Item
    {
        public int id;
        public string name;
        public string description;
        public int level;
        public bool isStackable;
        public Action onUse;
        internal int curStack = 1;
        internal int maxStack = 64;

        public Item(string name, string description = default(string), int level = 1, bool isStackable = default(bool), Action onUse = default(Action))
        {
            this.name = name;
            this.description = description;
            this.level = level;
            this.isStackable = isStackable;
            this.onUse = onUse;
        }
    }
}