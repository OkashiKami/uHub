namespace uHub.Databse
{
    using uHub.Databse.Entities;
    using uHub.Databse.Items;

    public static class ItemDatabase
    {
        public static Item HealthPotion1 = new Item (name: "Health Potion", description: "This is a level 1 health potion", level: 1, isStackable: true);
        public static Item HealthPotion2 = new Item (name: "Health Potion", description: "This is a level 2 health potion", level: 2, isStackable: true);
        public static Item HealthPotion3 = new Item (name: "Health Potion", description: "This is a level 3 health potion", level: 3, isStackable: true);
    }
    public static class EntityDatabase
    {
        public static Entity player = new Entity(name: "Player", description: "Default player used for first creation");
    }   
}
