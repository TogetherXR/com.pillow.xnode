﻿using UnityEditor;
using UnityEngine;
using System.IO;

namespace XNodeEditor
{
    /// <summary> Deals with modified assets </summary>
    internal class NodeEditorAssetModProcessor : AssetModificationProcessor
    {
        /// <summary> Automatically delete Node sub-assets before deleting their script.
        /// This is important to do, because you can't delete null sub assets.
        /// <para/> For another workaround, see: https://gitlab.com/RotaryHeart-UnityShare/subassetmissingscriptdelete </summary> 
        private static AssetDeleteResult OnWillDeleteAsset(string path, RemoveAssetOptions options)
        {
            // Skip processing anything without the .cs extension
            if (Path.GetExtension(path) != ".cs")
            {
                return AssetDeleteResult.DidNotDelete;
            }

            // Get the object that is requested for deletion
            var obj = AssetDatabase.LoadAssetAtPath<Object>(path);

            // If we aren't deleting a script, return
            if (obj is MonoScript script)
            {
                var scriptType = script.GetClass();

                if (scriptType == null ||
                    (scriptType != typeof(XNode.Node) && !scriptType.IsSubclassOf(typeof(XNode.Node))))
                {
                    return AssetDeleteResult.DidNotDelete;
                }

                // Find all ScriptableObjects using this script
                var guids = AssetDatabase.FindAssets($"t:{scriptType}");

                for (var i = 0; i < guids.Length; i++)
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                    var objects = AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath);

                    for (var k = 0; k < objects.Length; k++)
                    {
                        if (objects[k] is XNode.Node node &&
                            node.GetType() == scriptType &&
                            node != null && node.graph != null)
                        {
                            // Delete the node and notify the user
                            Debug.LogWarning($"{node.name} of {node.graph} depended on deleted script and has been removed automatically.", node.graph);
                            node.graph.RemoveNode(node);
                        }
                    }
                }

                // We didn't actually delete the script. Tell the internal system to carry on with normal deletion procedure
                return AssetDeleteResult.DidNotDelete;
            }

            // Check script type. Return if deleting a non-node script
            return AssetDeleteResult.DidNotDelete;
        }
    }
}
