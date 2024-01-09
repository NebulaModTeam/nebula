namespace NebulaModel.DataStructures.Chat;

public enum ChatCommandGiftType
{
    Soil = 0, // TODO: Since Soil Pile (item id 1099 ) is also an item, we can probably remove this all together and just use Item
    Item = 1,
    Energy = 2
}
