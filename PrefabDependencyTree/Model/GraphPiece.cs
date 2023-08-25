using System.Collections.Generic;
using System.Linq;

namespace PrefabDependencyTree.Model;

public class GraphPiece
{
    public string PieceName;
    public string RequiredCraftingStation;
    public Dictionary<GraphItem, int> BuildRequirements;

    public static GraphPiece FromPiece(Piece fromGame)
    {
        return new GraphPiece
        {
            PieceName = fromGame.name,
            RequiredCraftingStation = fromGame.m_craftingStation == null
                ? "no_station_required"
                : fromGame.m_craftingStation.name,
            BuildRequirements = fromGame.m_resources
                .Where(resource => resource.m_resItem != null)
                .ToDictionary(
                    resource => GraphItem.GetOrCreate(resource.m_resItem.name,
                        resource.m_resItem.m_itemData.m_shared.m_itemType.ToString()),
                    resource => resource.m_amount
                )
        };
    }

    public override string ToString()
    {
        if (BuildRequirements.Count <= 0)
        {
            return $"[piece name '{PieceName}', requires station '{RequiredCraftingStation}']";
        }
        string requirements = string.Join("\n    ", BuildRequirements
            .Select(requirement => $"[{requirement.Key} x{requirement.Value}]"));
        return $"[piece name '{PieceName}', requires station '{RequiredCraftingStation}', requirements:\n" +
               $"    {requirements}\n" +
               $"]";
    }
}