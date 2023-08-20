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
            RequiredCraftingStation = fromGame.m_craftingStation == null ? "" : fromGame.m_craftingStation.name,
            BuildRequirements = fromGame.m_resources
                .Where(resource => resource.m_resItem != null)
                .ToDictionary(
                    resource => GraphItem.GetOrCreate(resource.m_resItem.name),
                    resource => resource.m_amount
                )
        };
    }
}