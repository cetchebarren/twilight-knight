using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace etchebarren
{
    public class CustomNPC : MonoBehaviour
    {
        public enum Gender
        {
            male,
            female
        }

        public enum HairstyleM
        {
            Short,
            Medium,
            Long,
            Bald
        }

        public enum HairstyleF
        {
            HighPonytail,
            LowPonyTail,
            LongLoose,
            Short,
            Bald
        }

        public enum FacialHairM
        {
            None,
            MustacheCurly,
            MustacheBushy,
            LongBeard,
            Goatee,
            MustacheCurlyAndGoatee,
            MustacheBushyAndGoatee,
        }

        public enum HairColor
        {
            LightBlonde,
            Blonde,
            LightBrown,
            Brown,
            DarkBrown,
            VeryDarkBrown,
            VeryVeryDarkBrown,
            Black,
            Red,
            Gray,
            White
        }

        public enum EyeColor
        {
            Brown,
            Yellow,
            Green,
            Blue,
            Gray,
            Red
        }

        public enum SkinColor
        {
            Light1,
            Light2,
            Light3,
            Medium1,
            Medium2,
            Medium3,
            Dark1,
            Dark2,
            Dark3
        }

        public enum HatF
        {
            None, 
            Bonnet
        }

        public enum Armor
        {
            None,
            Set1,
            Set2
        }

        public enum Shirt
        {
            Black,
            Gray,
            White,
            Brown,
            Blue,
            Green,
            Red,
            None
        }

        public enum Pants
        {
            Black,
            LightBrown,
            DarkBrown,
            Gray,
            White,
            None
        }

        [Header("Settings")]
        public Gender gender;
        public Armor armor;
        public SkinColor skinColor;
        public HairColor hairColor;
        public EyeColor eyeColor;

        // please update to show in inspector ONLY IF gender == Gender.Male:
        public HairstyleM hairstyleM;
        public FacialHairM facialHairM;

        // please update to show in inspector ONLY IF gender == Gender.Female:
        public HairstyleF hairstyleF;
        public HatF hatF;

        //please update toshow in inspector ONLY IF armor == Armor.none:
        public Shirt shirt;
        public Pants pants;

        [Header("References")]
        // Male
        [SerializeField] private GameObject maleNPC;
        [SerializeField] private GameObject[] maleBodyMeshes; // Hands and Upper Body
        [SerializeField] private SkinnedMeshRenderer[] maleBody;
        [SerializeField] private GameObject[] maleHairstyles;
        [SerializeField] private GameObject[] maleFacialHair;
        [SerializeField] private GameObject[] maleArmor1;
        [SerializeField] private GameObject[] maleArmor2;
        [SerializeField] private GameObject[] maleShirts;
        [SerializeField] private GameObject[] malePants;
        // Female
        [SerializeField] private GameObject femaleNPC;
        [SerializeField] private GameObject[] femaleBodyMeshes; // Hands and Upper Body
        [SerializeField] private SkinnedMeshRenderer[] femaleBody;
        [SerializeField] private GameObject[] femaleHairstyles;
        [SerializeField] private GameObject[] femaleHats;
        [SerializeField] private GameObject[] femaleArmor1;
        [SerializeField] private GameObject[] femaleArmor2;
        [SerializeField] private GameObject[] femaleShirts;
        [SerializeField] private GameObject[] femalePants;
        // Colors
        [SerializeField] private Material[] hairColors;
        [SerializeField] private MeshRenderer[] hairMeshRenderers;
        [SerializeField] private SkinnedMeshRenderer[] hairSkinnedMeshRenderers;
        [SerializeField] private Material[] eyeColors;
        [SerializeField] private SkinnedMeshRenderer bodyHeadM; // used for eye color, eyebrows/eyelashes, and skin
        [SerializeField] private SkinnedMeshRenderer bodyHeadF; // used for eye color, eyebrows/eyelashes, and skin
        [SerializeField] private Material[] maleSkinColors;
        [SerializeField] private Material[] femaleSkinColors;

        [SerializeField] bool RandomCanHaveArmor = false;
        [SerializeField] Animator anim;
        [SerializeField] Avatar maleBodyAvatar;
        [SerializeField] Avatar femaleBodyAvatar;

        [Header("Mimic Player")]
        public Transform maleDetailsParent;
        public Transform femaleDetailsParent;

        /* NEXT: Add these to some object on player prefab so there is no need to set it PER custom npc that we clone*/
        /*
        public SkinnedMeshRenderer[] playerFemaleBody;
        public SkinnedMeshRenderer[] playerMaleBody;
        public GameObject[] playerMaleDetails;
        public GameObject[] playerFemaleDetails;
        */

        private bool cloned = false;

        // Function to customize character
        public void UpdateNPC()
        {
            if(gender == Gender.male) // Male
            {
                //Set Gender
                femaleNPC.SetActive(false);
                maleNPC.transform.SetSiblingIndex(0);
                maleNPC.SetActive(true);
                anim.avatar = maleBodyAvatar;

                //Set Hairstyles
                foreach(GameObject hairstyle in maleHairstyles)
                {
                    hairstyle.SetActive(false);
                }
                if(hairstyleM != HairstyleM.Bald)
                {
                    maleHairstyles[(int)hairstyleM].SetActive(true);
                }

                //Set Facial Hair
                foreach (GameObject facialHair in maleFacialHair)
                {
                    facialHair.SetActive(false);
                }
                switch (facialHairM)
                {
                    case FacialHairM.None:
                        break;
                    case FacialHairM.MustacheCurly:
                        maleFacialHair[2].SetActive(true);
                        break;
                    case FacialHairM.MustacheBushy:
                        maleFacialHair[1].SetActive(true);
                        break;
                    case FacialHairM.LongBeard:
                        maleFacialHair[0].SetActive(true);
                        maleFacialHair[1].SetActive(true);
                        break;
                    case FacialHairM.Goatee:
                        maleFacialHair[3].SetActive(true);
                        break;
                    case FacialHairM.MustacheBushyAndGoatee:
                        maleFacialHair[1].SetActive(true);
                        maleFacialHair[3].SetActive(true);
                        break;
                    case FacialHairM.MustacheCurlyAndGoatee:
                        maleFacialHair[2].SetActive(true);
                        maleFacialHair[3].SetActive(true);
                        break;
                    default:
                        break;
                }
            }
            else // Female
            {
                //Set Gender
                maleNPC.SetActive(false);
                femaleNPC.transform.SetSiblingIndex(0);
                femaleNPC.SetActive(true);
                anim.avatar = femaleBodyAvatar;

                //Set Hairstyles
                foreach (GameObject hairstyle in femaleHairstyles)
                {
                    hairstyle.SetActive(false);
                }
                if (hairstyleF != HairstyleF.Bald)
                {
                    femaleHairstyles[(int)hairstyleF].SetActive(true);
                }

                //Set Hat
                foreach (GameObject hat in femaleHats)
                {
                    hat.SetActive(false);
                }
                if (hatF == HatF.Bonnet)
                {
                    femaleHats[0].SetActive(true);
                }

            }

            // Clothing
            if(armor == Armor.Set1)
            {
                // Body Meshes
                foreach(GameObject bodyMesh in maleBodyMeshes)
                {
                    bodyMesh.SetActive(false);
                }
                foreach (GameObject bodyMesh in femaleBodyMeshes)
                {
                    bodyMesh.SetActive(false);
                }
                // Disable Shirts
                foreach (GameObject shirt in femaleShirts)
                {
                    shirt.SetActive(false);
                }
                foreach (GameObject shirt in maleShirts)
                {
                    shirt.SetActive(false);
                }
                // Disable Pants
                foreach (GameObject pants in femalePants)
                {
                    pants.SetActive(false);
                }
                foreach (GameObject pants in malePants)
                {
                    pants.SetActive(false);
                }
                // Enable Armor
                foreach (GameObject armorPiece in femaleArmor2)
                {
                    armorPiece.SetActive(false);
                }
                foreach (GameObject armorPiece in maleArmor2)
                {
                    armorPiece.SetActive(false);
                }
                foreach (GameObject armorPiece in femaleArmor1)
                {
                    armorPiece.SetActive(true);
                }
                foreach (GameObject armorPiece in maleArmor1)
                {
                    armorPiece.SetActive(true);
                }

            }
            else if (armor == Armor.Set2)
            {
                // Body Meshes
                foreach (GameObject bodyMesh in maleBodyMeshes)
                {
                    bodyMesh.SetActive(false);
                }
                foreach (GameObject bodyMesh in femaleBodyMeshes)
                {
                    bodyMesh.SetActive(false);
                }
                // Disable Shirts
                foreach (GameObject shirt in femaleShirts)
                {
                    shirt.SetActive(false);
                }
                foreach (GameObject shirt in maleShirts)
                {
                    shirt.SetActive(false);
                }
                // Disable Pants
                foreach (GameObject pants in femalePants)
                {
                    pants.SetActive(false);
                }
                foreach (GameObject pants in malePants)
                {
                    pants.SetActive(false);
                }
                // Enable Armor
                foreach (GameObject armorPiece in femaleArmor1)
                {
                    armorPiece.SetActive(false);
                }
                foreach (GameObject armorPiece in maleArmor1)
                {
                    armorPiece.SetActive(false);
                }
                foreach (GameObject armorPiece in femaleArmor2)
                {
                    armorPiece.SetActive(true);
                }
                foreach (GameObject armorPiece in maleArmor2)
                {
                    armorPiece.SetActive(true);
                }
            }
            else
            {
                // Body Meshes
                foreach (GameObject bodyMesh in maleBodyMeshes)
                {
                    bodyMesh.SetActive(true);
                }
                foreach (GameObject bodyMesh in femaleBodyMeshes)
                {
                    bodyMesh.SetActive(true);
                }
                // DisableArmor
                foreach (GameObject armorPiece in femaleArmor1)
                {
                    armorPiece.SetActive(false);
                }
                foreach (GameObject armorPiece in maleArmor1)
                {
                    armorPiece.SetActive(false);
                }
                foreach (GameObject armorPiece in femaleArmor2)
                {
                    armorPiece.SetActive(false);
                }
                foreach (GameObject armorPiece in maleArmor2)
                {
                    armorPiece.SetActive(false);
                }
                //Reset all and Pick Shirt
                foreach (GameObject shirt in femaleShirts)
                {
                    shirt.SetActive(false);
                }
                foreach (GameObject shirt in maleShirts)
                {
                    shirt.SetActive(false);
                }
                if(shirt != Shirt.None)
                {
                    maleShirts[(int)shirt].SetActive(true);
                    femaleShirts[(int)shirt].SetActive(true);
                }
                //Reset all and Pick Pants
                foreach (GameObject pants in femalePants)
                {
                    pants.SetActive(false);
                }
                foreach (GameObject pants in malePants)
                {
                    pants.SetActive(false);
                }
                if (pants != Pants.None)
                {
                    femaleBodyMeshes[2].SetActive(false);
                    maleBodyMeshes[2].SetActive(false);
                    malePants[(int)pants].SetActive(true);
                    femalePants[(int)pants].SetActive(true);
                }

                if (PrefabUtility.IsPartOfPrefabInstance(this.gameObject))
                {
                    // Mark the scene as dirty to ensure changes are saved
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                }
            }

            // Hair Colors
            if (!cloned)
            {
                foreach (MeshRenderer hairMeshRenderer in hairMeshRenderers)
                {
                    hairMeshRenderer.material = hairColors[(int)hairColor];

                    // Apply material changes to LODs as well
                    List<GameObject> lods = GetLODs(hairMeshRenderer.gameObject);
                    foreach (GameObject lod in lods)
                    {
                        MeshRenderer meshRenderer = lod.GetComponent<MeshRenderer>();
                        if (meshRenderer != null)
                        {
                            meshRenderer.material = hairMeshRenderer.sharedMaterial;
                        }
                    }
                }
                foreach (SkinnedMeshRenderer hairSkinnedMeshRenderer in hairSkinnedMeshRenderers)
                {
                    Material[] mats = hairSkinnedMeshRenderer.sharedMaterials;
                    mats[0] = hairColors[(int)hairColor];
                    hairSkinnedMeshRenderer.materials = mats;

                    // Apply material changes to LODs as well
                    List<GameObject> lods = GetLODs(hairSkinnedMeshRenderer.gameObject);
                    foreach (GameObject lod in lods)
                    {
                        SkinnedMeshRenderer skinnedMeshRenderer = lod.GetComponent<SkinnedMeshRenderer>();
                        if (skinnedMeshRenderer != null)
                        {
                            skinnedMeshRenderer.materials = hairSkinnedMeshRenderer.sharedMaterials;
                        }
                    }
                }

                // Male model Eye and eyebrow colors
                Material[] mBodyHead = bodyHeadM.sharedMaterials;
                mBodyHead[2] = eyeColors[(int)eyeColor];
                mBodyHead[4] = hairColors[(int)hairColor];
                bodyHeadM.materials = mBodyHead;
                // Apply material changes to LODs as well
                List<GameObject> headLodsM = GetLODs(bodyHeadM.gameObject);
                foreach (GameObject lod in headLodsM)
                {
                    SkinnedMeshRenderer skinnedMeshRenderer = lod.GetComponent<SkinnedMeshRenderer>();
                    if (skinnedMeshRenderer != null)
                    {
                        skinnedMeshRenderer.materials = bodyHeadM.sharedMaterials;
                    }
                }

                // Female model Eye and eyebrow colors
                Material[] fBodyHead = bodyHeadF.sharedMaterials;
                fBodyHead[3] = eyeColors[(int)eyeColor];
                fBodyHead[1] = hairColors[(int)hairColor];
                bodyHeadF.materials = fBodyHead;
                // Apply material changes to LODs as well
                List<GameObject> headLodsF = GetLODs(bodyHeadF.gameObject);
                foreach (GameObject lod in headLodsF)
                {
                    SkinnedMeshRenderer skinnedMeshRenderer = lod.GetComponent<SkinnedMeshRenderer>();
                    if (skinnedMeshRenderer != null)
                    {
                        skinnedMeshRenderer.materials = bodyHeadF.sharedMaterials;
                    }
                }

                //Skin color
                foreach (SkinnedMeshRenderer bodySkinnedMeshRenderer in maleBody)
                {
                    Material[] mats = bodySkinnedMeshRenderer.sharedMaterials;
                    mats[0] = maleSkinColors[(int)skinColor];
                    bodySkinnedMeshRenderer.materials = mats;

                    // Apply material changes to LODs as well
                    List<GameObject> lods = GetLODs(bodySkinnedMeshRenderer.gameObject);
                    foreach (GameObject lod in lods)
                    {
                        SkinnedMeshRenderer skinnedMeshRenderer = lod.GetComponent<SkinnedMeshRenderer>();
                        if (skinnedMeshRenderer != null)
                        {
                            skinnedMeshRenderer.materials = bodySkinnedMeshRenderer.sharedMaterials;
                        }
                    }
                }
                foreach (SkinnedMeshRenderer bodySkinnedMeshRenderer in femaleBody)
                {
                    Material[] mats = bodySkinnedMeshRenderer.sharedMaterials;
                    mats[0] = femaleSkinColors[(int)skinColor];
                    bodySkinnedMeshRenderer.materials = mats;

                    // Apply material changes to LODs as well
                    List<GameObject> lods = GetLODs(bodySkinnedMeshRenderer.gameObject);
                    foreach (GameObject lod in lods)
                    {
                        SkinnedMeshRenderer skinnedMeshRenderer = lod.GetComponent<SkinnedMeshRenderer>();
                        if (skinnedMeshRenderer != null)
                        {
                            skinnedMeshRenderer.materials = bodySkinnedMeshRenderer.sharedMaterials;
                        }
                    }
                }
            }

            UnityEditor.EditorUtility.SetDirty(this);
            Debug.Log("Character customization applied.");
        }

        public void RandomizeNPC()
        {
            // Set gender to a random value
            gender = (Gender)UnityEngine.Random.Range(0, 2);

            // Set hairstyle and facial hair for males if gender is male
            if (gender == Gender.male)
            {
                hairstyleM = (HairstyleM)UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(HairstyleM)).Length);
                facialHairM = (FacialHairM)UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(FacialHairM)).Length);
            }
            // Set hairstyle and hat for females if gender is female
            else if (gender == Gender.female)
            {
                hairstyleF = (HairstyleF)UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(HairstyleF)).Length);
                hatF = (HatF)UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(HatF)).Length);
            }

            // Set armor to a random value
            if(RandomCanHaveArmor)
            {
                armor = (Armor)UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(Armor)).Length);
            }
            else
            {
                armor = Armor.None;
            }

            // Set shirt and pants if armor is none
            if (armor == Armor.None)
            {
                shirt = (Shirt)UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(Shirt)).Length);
                pants = (Pants)UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(Pants)).Length);
            }

            // Set skin color to a random value
            skinColor = (SkinColor)UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(SkinColor)).Length);

            // Set hair color to a random value
            hairColor = (HairColor)UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(HairColor)).Length);

            // Set eye color to a random value
            eyeColor = (EyeColor)UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(EyeColor)).Length);

            // Call UpdateNPC to apply the randomized settings
            UpdateNPC();
        }

        public void MimicPlayer()
        {
            if(ScenePersistentPlayerObject.instance == null)
            {
                Debug.LogError("SCPO was null, error mimicing player model in Custom NPC, returning.");
                return;
            }

            cloned = true;

            if (PlayerStats.instance.gender == "mars")
            {
                gender = Gender.male;
            }
            else gender = Gender.female;

            hairstyleM = HairstyleM.Bald;
            hairstyleF = HairstyleF.Bald;
            facialHairM = FacialHairM.None;
            hatF = HatF.None;
            armor = Armor.Set1;
            shirt = Shirt.Black;
            pants = Pants.DarkBrown;

            // Update colors for skin, eyebrows, eyes
            for(int i = 0; i < femaleBody.Length; i++)
            {
                femaleBody[i].materials = ScenePersistentPlayerObject.instance.saveManager.selectSkinTone.rendererListF[i].materials;
            }

            for (int i = 0; i < maleBody.Length; i++)
            {
                maleBody[i].materials = ScenePersistentPlayerObject.instance.saveManager.selectSkinTone.rendererListM[i].materials;
            }

            // Clear details/hair before adding new ones
            for (int i = maleDetailsParent.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(maleDetailsParent.GetChild(i).gameObject);
            }

            for (int i = femaleDetailsParent.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(femaleDetailsParent.GetChild(i).gameObject);
            }

            // Instantiate male details / hair
            foreach (GameObject detail in ScenePersistentPlayerObject.instance.saveManager.selectSkinTone.playerMaleDetails)
            {
                if (detail != null)
                {
                    GameObject newDetail = Instantiate(detail, maleDetailsParent);
                    newDetail.transform.localPosition = detail.transform.localPosition;
                    newDetail.transform.localRotation = detail.transform.localRotation;
                    newDetail.transform.localScale = detail.transform.localScale;
                }
            }

            // Instantiate female details / hair
            foreach (GameObject detail in ScenePersistentPlayerObject.instance.saveManager.selectSkinTone.playerFemaleDetails)
            {
                if (detail != null)
                {
                    GameObject newDetail = Instantiate(detail, femaleDetailsParent);
                    newDetail.transform.localPosition = detail.transform.localPosition;
                    newDetail.transform.localRotation = detail.transform.localRotation;
                    newDetail.transform.localScale = detail.transform.localScale;
                }
            }
        
            UpdateNPC();
        }

        public static List<GameObject> GetLODs(GameObject parent)
        {
            List<GameObject> lods = new List<GameObject>();
            FindLODs(parent.transform, lods);
            return lods;
        }

        private static void FindLODs(Transform parent, List<GameObject> lods)
        {
            if (parent.childCount == 0)
            {
                lods.Add(parent.gameObject);
            }
            else
            {
                foreach (Transform child in parent)
                {
                    FindLODs(child, lods);
                }
            }
        }
    }
}

#if UNITY_EDITOR
namespace etchebarren
{
    [CustomEditor(typeof(CustomNPC))]
    public class CustomNPCEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // Get the target object
            CustomNPC customizer = (CustomNPC)target;

            // Settings Header
            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);

            // Gender field
            customizer.gender = (CustomNPC.Gender)EditorGUILayout.EnumPopup("Gender", customizer.gender);

            // Hair / Hats
            EditorGUILayout.LabelField("Clothing", EditorStyles.boldLabel);

            // Conditional display based on Gender
            if (customizer.gender == CustomNPC.Gender.male)
            {
                customizer.hairstyleM = (CustomNPC.HairstyleM)EditorGUILayout.EnumPopup("Hairstyle (Male)", customizer.hairstyleM);
                customizer.facialHairM = (CustomNPC.FacialHairM)EditorGUILayout.EnumPopup("Facial Hair (Male)", customizer.facialHairM);
            }
            else if (customizer.gender == CustomNPC.Gender.female)
            {
                customizer.hairstyleF = (CustomNPC.HairstyleF)EditorGUILayout.EnumPopup("Hairstyle (Female)", customizer.hairstyleF);
                customizer.hatF = (CustomNPC.HatF)EditorGUILayout.EnumPopup("Hat (Female)", customizer.hatF);
            }

            // Clothing Header
            EditorGUILayout.LabelField("Clothing", EditorStyles.boldLabel);

            // Armor field
            customizer.armor = (CustomNPC.Armor)EditorGUILayout.EnumPopup("Armor", customizer.armor);

            // Conditional display based on Armor
            if (customizer.armor == CustomNPC.Armor.None)
            {
                customizer.shirt = (CustomNPC.Shirt)EditorGUILayout.EnumPopup("Shirt", customizer.shirt);
                customizer.pants = (CustomNPC.Pants)EditorGUILayout.EnumPopup("Pants", customizer.pants);
            }

            // Color Header
            EditorGUILayout.LabelField("Colors", EditorStyles.boldLabel);

            // Other settings
            customizer.skinColor = (CustomNPC.SkinColor)EditorGUILayout.EnumPopup("Skin Color", customizer.skinColor);
            customizer.hairColor = (CustomNPC.HairColor)EditorGUILayout.EnumPopup("Hair Color", customizer.hairColor);
            customizer.eyeColor = (CustomNPC.EyeColor)EditorGUILayout.EnumPopup("Eye Color", customizer.eyeColor);

            EditorGUILayout.Space(); // Add some space before the buttons

            // Randomize Button
            if (GUILayout.Button("Randomize"))
            {
                // Call a method to randomize NPC settings
                customizer.RandomizeNPC();
            }

            EditorGUILayout.Space(); // Add some space before the buttons

            // Update Button
            if (GUILayout.Button("Update NPC"))
            {
                // Call a method to update NPC based on settings
                customizer.UpdateNPC();
            }

            EditorGUILayout.Space(); // Add some space before the next section

            // Use serializedObject to draw specific fields
            SerializedProperty property = serializedObject.GetIterator();
            property.NextVisible(true); // Skip the script field

            while (property.NextVisible(false))
            {
                if (property.name != "gender" &&
                    property.name != "armor" &&
                    property.name != "skinColor" &&
                    property.name != "hairColor" &&
                    property.name != "eyeColor" &&
                    property.name != "hairstyleM" &&
                    property.name != "facialHairM" &&
                    property.name != "hairstyleF" &&
                    property.name != "hatF" &&
                    property.name != "shirt" &&
                    property.name != "pants")
                {
                    EditorGUILayout.PropertyField(property, true);
                }
            }

            // Apply modified properties
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
