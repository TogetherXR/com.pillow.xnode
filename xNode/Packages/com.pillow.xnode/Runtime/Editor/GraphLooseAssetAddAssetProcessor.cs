using UnityEditor;

namespace XNodeEditor
{
    /// <summary> Automatically re-add loose node assets to the Graph node list
    /// From: https://github.com/Siccity/xNode/issues/342#issuecomment-1219091482
    /// </summary>
    internal sealed class GraphLooseAssetAddAssetProcessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            for (var i = 0; i < importedAssets.Length; i++)
            {
                var assetPath = importedAssets[i];
                var assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);

                if (typeof(XNode.NodeGraph).IsAssignableFrom(assetType))
                {
                    AttachLooseChildNodes(assetPath);
                }
            }

            for (var i = 0; i < movedAssets.Length; i++)
            {
                var assetPath = movedAssets[i];
                var assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);

                if (typeof(XNode.NodeGraph).IsAssignableFrom(assetType))
                {
                    AttachLooseChildNodes(assetPath);
                }
            }
        }

        public static void AttachLooseChildNodes(string assetPath)
        {
            XNode.NodeGraph graph = AssetDatabase.LoadAssetAtPath<XNode.NodeGraph>(assetPath);

            if (graph != null)
            {
                graph.nodes.RemoveAll(x => x == null); //Remove null items
                var assets = AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath);
                // Ensure that all sub node assets are present in the graph node list
                for (var u = 0; u < assets.Length; u++)
                {
                    // Ignore null sub assets
                    if (assets[u] == null)
                    {
                        continue;
                    }

                    if (!graph.nodes.Contains(assets[u] as XNode.Node))
                    {
                        graph.nodes.Add(assets[u] as XNode.Node);
                    }
                }
            }
        }
    }
}
