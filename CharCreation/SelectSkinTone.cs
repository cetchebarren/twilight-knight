using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectSkinTone : MonoBehaviour
{
    public Texture lightA_F, lightB_F, lightC_F;
    public Texture mediumA_F, mediumB_F, mediumC_F;
    public Texture darkA_F, darkB_F, darkC_F;

    public Texture lightA_M, lightB_M, lightC_M;
    public Texture mediumA_M, mediumB_M, mediumC_M;
    public Texture darkA_M, darkB_M, darkC_M;

    private Texture skinToSetF, skinToSetM;
    public List<SkinnedMeshRenderer> rendererListF = new List<SkinnedMeshRenderer>();
    public List<SkinnedMeshRenderer> rendererListM = new List<SkinnedMeshRenderer>();

    public string currentSkinTone = "lightB";

    [Header("References for Custom NPC Player Clone")]
    public GameObject[] playerMaleDetails;
    public GameObject[] playerFemaleDetails;

    public void SetSkinTone(string skintype)
    {
        currentSkinTone = skintype;

        if (skintype == "lightA")
        {
            skinToSetF = lightA_F;
            skinToSetM = lightA_M;
        }
        else if (skintype == "mediumA")
        {
            skinToSetF = mediumA_F;
            skinToSetM = mediumA_M;
        }
        else if (skintype == "darkA")
        {
            skinToSetF = darkA_F;
            skinToSetM = darkA_M;
        }
        else if (skintype == "lightB")
        {
            skinToSetF = lightB_F;
            skinToSetM = lightB_M;
        }
        else if (skintype == "mediumB")
        {
            skinToSetF = mediumB_F;
            skinToSetM = mediumB_M;
        }
        else if (skintype == "darkB")
        {
            skinToSetF = darkB_F;
            skinToSetM = darkB_M;
        }
        else if (skintype == "lightC")
        {
            skinToSetF = lightC_F;
            skinToSetM = lightC_M;
        }
        else if (skintype == "mediumC")
        {
            skinToSetF = mediumC_F;
            skinToSetM = mediumC_M;
        }
        else if (skintype == "darkC")
        {
            skinToSetF = darkC_F;
            skinToSetM = darkC_M;
        }

        for (int i = 0; i < rendererListF.Count; i++)
        {
            rendererListF[i].material.SetTexture("_MainTex", skinToSetF);
        }

        for (int i = 0; i < rendererListM.Count; i++)
        {
            rendererListM[i].material.SetTexture("_MainTex", skinToSetM);
        }
    }

}
