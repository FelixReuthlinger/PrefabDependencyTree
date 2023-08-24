# Prefab Dependency Tree

Mod for Modders that have too many recipes and items flowing around -
generated [.dot graph file](https://graphviz.org/doc/info/lang.html) that can be used
with [yED](https://www.yworks.com/yed-live/).

## Features

Read all kind of in-game objects and puts them into relation.

The mod will read from game types:

* Character (any kind of creature)
* Piece (anything you can build)
* TreeBase (tree)
* TreeLog (tree)
* Pickable
* PickableItem
* DropOnDestroy (anything destructible)
* Container (chests)
* LootSpawner (chests)
* MineRock (ore rocks)
* MineRock5 (ore rocks)

For example:

1. Greydwarf -(drops)-> GreydwarfEye -(used for building)-> Portal

![img.png](img.png)

2. Wood -(part of recipe)-> Recipe_Hammer -(crafts)-> Hammer

But in the end the amount of items and recipes, etc. even in the vanilla game is really large, it will create a huge
graph. I can recommend to use yED Live to load the file, but then download it from there as .graphml file and use yED
Desktop to visualize it (more performant). From there you can also create images, like .png or .svg.

# Miscellaneous

<details>
  <summary>Attributions</summary>

* * [Graph by Abd Majd](https://thenounproject.com/browse/icons/term/graph/) Noun Project (CC BY 3.0)

</details>

<details>
  <summary>Contact</summary>

* https://github.com/FelixReuthlinger/PrefabDependencyTree
* Discord: `fluuxxx` (short Flux) (you can find me around some of the Valheim modding discords, too)

</details>


