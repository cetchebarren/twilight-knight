using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace etchebarren
{
    public class SceneEntrance : MonoBehaviour
    {
        public Transform spawnPoint;
        public SceneExit sceneExit;
        public GameObject entranceVisualizer;
        [SerializeField] bool hideVisualizerOnStart = true;

        void Awake()
        {
            if(hideVisualizerOnStart) entranceVisualizer.SetActive(false);
        }
    }
}
