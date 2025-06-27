using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json.Linq;

namespace McpUnity.Resources
{
    /// <summary>
    /// Resource for retrieving a vastly truncated hierarchy of all game objects in the Unity scenes.
    /// </summary>
    public class GetScenesHierarchySimpleResource : McpResourceBase
    {
        public GetScenesHierarchySimpleResource()
        {
            Name = "get_scenes_hierarchy_simple";
            Description = "Retrieves a vastly truncated hierarchy of all game objects in the Unity loaded scenes.";
            Uri = "unity://scenes_hierarchy_simple";
        }

        /// <summary>
        /// Fetch a vastly truncated hierarchy of all game objects in the Unity loaded scenes.
        /// </summary>
        /// <param name="parameters">Resource parameters as a JObject (not used)</param>
        /// <returns>A JObject containing the truncated hierarchy of game objects</returns>
        public override JObject Fetch(JObject parameters)
        {
            // Get all game objects in the hierarchy
            JArray hierarchyArray = GetSceneHierarchy();

            // Create the response
            return new JObject
            {
                ["success"] = true,
                ["message"] = $"Retrieved truncated hierarchy with {hierarchyArray.Count} root objects",
                ["hierarchy"] = hierarchyArray
            };
        }

        /// <summary>
        /// Get a vastly truncated hierarchy of all game objects in the Unity loaded scenes.
        /// </summary>
        /// <returns>A JArray containing the truncated hierarchy of game objects</returns>
        private JArray GetSceneHierarchy()
        {
            JArray rootObjectsArray = new JArray();

            // Get all loaded scenes
            int sceneCount = SceneManager.loadedSceneCount;
            for (int i = 0; i < sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);

                // Create a scene object
                JObject sceneObject = new JObject
                {
                    ["name"] = scene.name,
                    ["rootObjects"] = new JArray()
                };

                // Get root game objects in the scene
                GameObject[] rootObjects = scene.GetRootGameObjects();
                JArray rootObjectsInScene = (JArray)sceneObject["rootObjects"];

                foreach (GameObject rootObject in rootObjects)
                {
                    // Add the root object and its children to the array
                    rootObjectsInScene.Add(GameObjectToJObject(rootObject));
                }


                // Add the scene to the root objects array
                rootObjectsArray.Add(sceneObject);
            }


            return rootObjectsArray;
        }


        /// <summary>
        /// Convert a GameObject to a vastly truncated JObject with its hierarchy.
        /// </summary>
        /// <param name="gameObject">The GameObject to convert</param>
        /// <returns>A JObject representing the truncated GameObject</returns>
        public static JObject GameObjectToJObject(GameObject gameObject)
        {
            if (gameObject == null) return null;

            // Add children
            JArray childrenArray = new JArray();
            foreach (Transform child in gameObject.transform)
            {
                childrenArray.Add(GameObjectToJObject(child.gameObject));
            }

            // Create a JObject for the game object
            JObject gameObjectJson = new JObject
            {
                ["name"] = gameObject.name,
                ["instanceId"] = gameObject.GetInstanceID(),
                ["children"] = childrenArray
            };

            return gameObjectJson;
        }
    }
}
