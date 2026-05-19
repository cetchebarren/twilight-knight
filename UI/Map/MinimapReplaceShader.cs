using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapReplaceShader : MonoBehaviour
{
	public Shader unlitShader;

	void Start()
	{
		//unlitShader = Shader.Find("Unlit/Texture");
		GetComponent<Camera>().SetReplacementShader(unlitShader, "");
	}

}
