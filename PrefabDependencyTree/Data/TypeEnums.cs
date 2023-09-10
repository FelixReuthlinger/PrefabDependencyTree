namespace PrefabDependencyTree.Data;

public enum NodeTypes
{
    Piece,
    CraftingStation,
    Processor,
    Recipe
}

public enum DropType
{
    Character,
    Container,
    Destructible,
    LootSpawner,
    MineRock,
    Pickable,
    Tree
}

public enum FilterType
{
    Include,
    Exclude
}
