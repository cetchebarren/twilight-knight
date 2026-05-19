using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace etchebarren
{
    [CreateAssetMenu(menuName = "Note")]

    public class Note : ScriptableObject
    {
        public int noteID;
        public string titleContent;
        [TextAreaAttribute(10,30)]
        public string bodyContent;

        public Sprite sprite;
        public Vector3 imageScale = Vector3.one;
        public float vOffset = 0f;
        public float hOffset = 0f;

    }
}
