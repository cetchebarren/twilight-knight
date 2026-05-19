using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class CreateSkinnedMeshClothing : MonoBehaviour
{
    [SerializeField] SkinnedMeshRenderer sourceSkin;
    [SerializeField] SkinnedMeshRenderer targetSkin;

    public void CreateNewSkinnedMesh()
    {
        sourceSkin.bones = targetSkin.bones;
        Debug.Log("Done");
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(CreateSkinnedMeshClothing))]
    public class CreateSkinnedMeshClothingEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector(); // Draw the default inspector

            CreateSkinnedMeshClothing myScript = (CreateSkinnedMeshClothing)target;
            if (GUILayout.Button("Create New Skinned Mesh"))
            {
                myScript.CreateNewSkinnedMesh();
            }
        }
    }
#endif
}
