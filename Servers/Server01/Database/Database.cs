namespace uHub.Databse
{
    using uHub.Databse.Entities;
    using uHub.Databse.Items;

    public static class ItemDatabase
    {
        public static Item HealthPotion1 = new Item (name: "Health v1", description: "This is a level 1 health potion", isStackable: true);
        public static Item HealthPotion2 = new Item (name: "Health v2", description: "This is a level 2 health potion", isStackable: true);
        public static Item HealthPotion3 = new Item (name: "Health v3", description: "This is a level 3 health potion", isStackable: true);
    }
    public static class EntityDatabase
    {
        public static Entity player = new Entity(name: "Player", description: "Default player used for first creation");
    }   
}
