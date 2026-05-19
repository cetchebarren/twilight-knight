using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

namespace etchebarren
{
    public class SceneTransitionWeb
    {
        private Dictionary<string, List<string>> transitions;

        public SceneTransitionWeb()
        {
            transitions = new Dictionary<string, List<string>>();
        }

        public void AddTransition(string fromScene, string toScene)
        {
            if (!transitions.ContainsKey(fromScene))
            {
                transitions[fromScene] = new List<string>();
            }
            transitions[fromScene].Add(toScene);

            // Since it's bidirectional, add transition in both directions
            if (!transitions.ContainsKey(toScene))
            {
                transitions[toScene] = new List<string>();
            }
            transitions[toScene].Add(fromScene);
        }

        // We are determining the next scene that we must go to in order to progress towards the goal
        public string GetNextScene(string intendedScene)
        {
            Queue<string> queue = new Queue<string>();
            HashSet<string> visited = new HashSet<string>();
            Dictionary<string, string> previousScene = new Dictionary<string, string>();

            string currentScene = SceneManager.GetActiveScene().name;
            // Start BFS from the current scene
            queue.Enqueue(currentScene);
            visited.Add(currentScene);
            previousScene[currentScene] = null; // Root node has no previous scene

            while (queue.Count > 0)
            {
                string scene = queue.Dequeue();

                // Check if the current scene is the intended scene
                if (scene == intendedScene)
                {
                    // Backtrack to find the next scene from the current scene
                    string backtrackScene = scene;

                    // If the intended scene is the current scene, there's no next scene to return
                    if (backtrackScene == currentScene)
                    {
                        Debug.Log("The intended scene is the current scene, no scene transition needed.");
                        return null;
                    }

                    // Backtrack until we find the scene directly after the current scene
                    while (previousScene[backtrackScene] != currentScene)
                    {
                        // This condition ensures we don't encounter a null value and marks an error
                        if (previousScene[backtrackScene] == null)
                        {
                            Debug.LogError("Encountered an unexpected null in previousScene. Backtrack failed.");
                            return null;
                        }

                        backtrackScene = previousScene[backtrackScene];
                    }
                    return backtrackScene;
                }

                // Check if the current scene has transitions defined
                if (!transitions.ContainsKey(scene))
                {
                    Debug.LogWarning("The scene " + scene + " is not currently included or found in the scene transition dictionary, used for cross-scene quest markers");
                    return null;
                }

                // Enqueue neighboring scenes (transitions)
                foreach (string neighbor in transitions[scene])
                {
                    if (!visited.Contains(neighbor))
                    {
                        queue.Enqueue(neighbor);
                        visited.Add(neighbor);
                        previousScene[neighbor] = scene; // Track the previous scene
                    }
                }
            }

            // Intended scene not found, return null
            return null;
        }


    }
}
