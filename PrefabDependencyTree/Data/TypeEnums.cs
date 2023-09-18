namespace PrefabDependencyTree.Data;

public enum NodeType
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
    Exclude,
    Unfiltered
}
