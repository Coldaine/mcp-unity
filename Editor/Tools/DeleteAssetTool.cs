using System;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json.Linq;
using McpUnity.Unity;
using McpUnity.Utils;

namespace McpUnity.Tools
{
    /// <summary>
    /// Tool for deleting assets from the Unity AssetDatabase
    /// </summary>
    public class DeleteAssetTool : McpToolBase
    {
        public DeleteAssetTool()
        {
            Name = "delete_asset";
            Description = "Deletes an asset from the Unity AssetDatabase";
        }
        
        /// <summary>
        /// Execute the DeleteAsset tool with the provided parameters
        /// </summary>
        /// <param name="parameters">Tool parameters as a JObject</param>
        public override JObject Execute(JObject parameters)
        {
            // Extract parameters
            string assetPath = parameters["assetPath"]?.ToObject<string>();
            string guid = parameters["guid"]?.ToObject<string>();
            bool moveToTrash = parameters["moveToTrash"]?.ToObject<bool>() ?? true;
            
            // Validate parameters - require either assetPath or guid
            if (string.IsNullOrEmpty(assetPath) && string.IsNullOrEmpty(guid))
            {
                return McpUnitySocketHandler.CreateErrorResponse(
                    "Required parameter 'assetPath' or 'guid' not provided", 
                    "validation_error"
                );
            }
            
            // If we have a GUID but no path, convert GUID to path
            if (string.IsNullOrEmpty(assetPath) && !string.IsNullOrEmpty(guid))
            {
                assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(assetPath))
                {
                    return McpUnitySocketHandler.CreateErrorResponse(
                        $"Asset with GUID '{guid}' not found", 
                        "not_found_error"
                    );
                }
            }
            
            // Check if the asset exists
            if (!AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath))
            {
                return McpUnitySocketHandler.CreateErrorResponse(
                    $"Asset at path '{assetPath}' not found", 
                    "not_found_error"
                );
            }
            
            // Check if it's a folder
            bool isFolder = AssetDatabase.IsValidFolder(assetPath);
            
            try
            {
                bool success;
                
                if (moveToTrash)
                {
                    // Move to trash (safer option)
                    success = AssetDatabase.MoveAssetToTrash(assetPath);
                }
                else
                {
                    // Permanently delete
                    success = AssetDatabase.DeleteAsset(assetPath);
                }
                
                if (!success)
                {
                    return McpUnitySocketHandler.CreateErrorResponse(
                        $"Failed to delete {(isFolder ? "folder" : "asset")} at path '{assetPath}'", 
                        "deletion_error"
                    );
                }
                
                // Refresh the AssetDatabase to update the project window
                AssetDatabase.Refresh();
                
                // Log the action
                string actionType = moveToTrash ? "moved to trash" : "permanently deleted";
                string assetType = isFolder ? "folder" : "asset";
                McpLogger.LogInfo($"Successfully {actionType} {assetType} at path '{assetPath}'");
                
                // Create the response
                return new JObject
                {
                    ["success"] = true,
                    ["type"] = "text",
                    ["message"] = $"Successfully {actionType} {assetType} '{System.IO.Path.GetFileName(assetPath)}' from path '{assetPath}'",
                    ["assetPath"] = assetPath,
                    ["isFolder"] = isFolder,
                    ["movedToTrash"] = moveToTrash
                };
            }
            catch (Exception ex)
            {
                return McpUnitySocketHandler.CreateErrorResponse(
                    $"Error deleting asset: {ex.Message}", 
                    "deletion_error"
                );
            }
        }
    }
}
