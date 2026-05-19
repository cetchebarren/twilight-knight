using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace etchebarren
{
	public class PlayerFootstepSoundManager : MonoBehaviour
	{
		public CameraHandler cameraHandler;
		public InputHandler inputHandler;
		private GameObject terrainFinder;   //Stores the Terrain GameObject out of the Scene for use later.
		private Terrain terrain;            //Your Terrain (if one's in your scene)
		private TerrainData terrainData;    //Lets us get to the Terrain's splatmap.
		private Vector3 terrainPos;         //Where are we on the Splatmap?
		public static int surfaceIndex = 0; //The order in which your textures were added to your Terrain.
		private string whatTexture;         //Holds the FILENAMEs of the Textures in your Terrain.
		public PlayerLocomotion playerLocomotion;

		public AudioSource footstepAudioSourceL;        //The AudioSource component
		public AudioSource footstepAudioSourceR;        //The AudioSource component

		public LayerMask layerMask;
		public Animator anim;              //Lets us pull floats out of our Animator's curves.
		public static GameObject floor;     //what are we standing on?
		private string currentFoot;         //Each foot does it's own Raycast

		public bool leftFootstepActive = false;
		public bool rightFootstepActive = false;
		public float footstepInterval = 0.1f;

		[Space(5.0f)] public float currentVolume;

		[Header("Default Volume")]
		public float defaultVolume = 0.9f;
		public float defaultRunningVolume = 1.0f;

		[Header("Default Volume")]
		public float crouchingVolumeMultiplier = 0.6f;

		[Header("Dirt Volumes")]
		public float dirtVolume = 1.0f;
		public float dirtRunningVolume = 1.1f;

		[Header("Mud Volumes")]
		public float mudVolume = 1.0f;
		public float mudRunningVolume = 1.1f;

		[Header("Wood Volumes")]
		public float woodVolume = 1.0f;
		public float woodRunningVolume = 1.1f;

		[Header("Leaves Volumes")]
		public float leavesVolume = 0.7f;
		public float leavesRunningVolume = 0.85f;

		[Header("Tile Volumes")]
		public float tileVolume = 1.0f;
		public float tileRunningVolume = 1.0f;

		[Header("Gravel Volumes")]
		public float gravelVolume = 0.8f;
		public float gravelRunningVolume = 0.95f;

		[Header("Snow Volumes")]
		public float snowVolume = 1.0f;
		public float snowRunningVolume = 1.1f;

		[Header("Sand Volumes")]
		public float sandVolume = 1.0f;
		public float sandRunningVolume = 1.1f;

		[Header("Grass Volumes")]
		public float grassVolume = 1.0f;
		public float grassRunningVolume = 1.1f;

		[Header("Stone Volumes")]
		public float stoneVolume = 1.0f;
		public float stoneRunningVolume = 1.1f;

		[Header("Metal Volumes")]
		public float metalVolume = 1.0f;
		public float metalRunningVolume = 1.1f;

		[Header("Shallow Water Volumes")]
		public float shallowWaterVolume = 0.9f;
		public float shallowWaterRunningVolume = 1.1f;

		[Header("Deep Water Volumes")]
		public float deepWaterVolume = 0.4f;
		public float deepWaterRunningVolume = 0.5f;

		[Header("Volume Multipliers")]
		public float strafingVolumeMultiplier = 0.75f;

		[Range(0.0f, 0.2f)]
		public float volumeVariance = 0.02f;    //Variance in volume levels per footstep; set this between 0.0 and 0.2 in the inspector. Default is 0.04f.
		private float pitch;
		[Range(0.0f, 0.2f)]
		public float pitchVariance = 0.08f;     //Variance in pitch levels per footstep; set this between 0.0 and 0.2 in the inspector. Default is 0.08f.
		[Space(5.0f)]
		public GameObject leftFoot; // For spawning effects on left foot, currently unused
		public GameObject rightFoot; // For spawning effects on right foot, currently unused
		[Space(5.0f)]
		[Header("Walking")]
		public AudioClip[] defaults = new AudioClip[0];
		public AudioClip[] dirt = new AudioClip[0];
		public AudioClip[] grass = new AudioClip[0];
		public AudioClip[] gravel = new AudioClip[0];
		public AudioClip[] leaves = new AudioClip[0];
		public AudioClip[] metal = new AudioClip[0];
		public AudioClip[] mud = new AudioClip[0];
		public AudioClip[] stone = new AudioClip[0];
		public AudioClip[] sand = new AudioClip[0];
		public AudioClip[] snow = new AudioClip[0];
		public AudioClip[] tile = new AudioClip[0];
		public AudioClip[] waterShallow = new AudioClip[0];
		public AudioClip[] waterDeep = new AudioClip[0];
		public AudioClip[] wood = new AudioClip[0];
		[Space(5.0f)]
		[Header("Running")]
		public AudioClip[] defaults_run = new AudioClip[0];
		public AudioClip[] dirt_run = new AudioClip[0];
		public AudioClip[] grass_run = new AudioClip[0];
		public AudioClip[] gravel_run = new AudioClip[0];
		public AudioClip[] leaves_run = new AudioClip[0];
		public AudioClip[] metal_run = new AudioClip[0];
		public AudioClip[] mud_run = new AudioClip[0];
		public AudioClip[] stone_run = new AudioClip[0];
		public AudioClip[] sand_run = new AudioClip[0];
		public AudioClip[] snow_run = new AudioClip[0];
		public AudioClip[] tile_run = new AudioClip[0];
		public AudioClip[] waterShallow_run = new AudioClip[0];
		public AudioClip[] waterDeep_run = new AudioClip[0];
		public AudioClip[] wood_run = new AudioClip[0];

		[Space(5.0f)]
		[Tooltip("Choose ONE")]
		public bool instantiatedFX;     //Check this checkbox in the inspector if you want your FX to be instantiated.
		[Tooltip("Choose ONE")]
		public bool toggledFX;          //Check this checkbox in the inspector if you want your FX to be enabled.

		[Space(5.0f)]
		public GameObject dirtFX;
		public GameObject snowFX;
		public GameObject waterFX;
		private Quaternion dirtRotation;
		private Quaternion snowRotation;
		private Quaternion waterRotation;
		private Vector3 dirtPos;
		private Vector3 snowPos;
		private Vector3 waterPos;

		//Start
		void Start()
		{
			FindTerrain();
		}

		public void FindTerrain()
		{
			terrainFinder = GameObject.FindGameObjectWithTag("Terrain");

			if (terrainFinder != null)
			{   //IS THERE A TERRAIN IN THE SCENE?
				terrain = Terrain.activeTerrain;
				terrainData = terrain.terrainData;
				terrainPos = terrain.transform.position;
			}

			leftFootstepActive = false;
			rightFootstepActive = false;
		}

		public void FootstepL()
		{
			if (leftFootstepActive || !inputHandler.isGrounded) return;
            leftFootstepActive = true;
            StartCoroutine(PlayFootstepSound(true, playerLocomotion.isSprinting));
        }

		public void FootstepR()
		{
			if (rightFootstepActive || !inputHandler.isGrounded) return;
            rightFootstepActive = true;
            StartCoroutine(PlayFootstepSound(false, playerLocomotion.isSprinting));
        }

		private IEnumerator PlayFootstepSound(bool left, bool running)
		{
			SetGroundType(left);
			if (left)
			{
				CheckTextureAndPlay(left, running);
				yield return new WaitForSeconds(footstepInterval);
				leftFootstepActive = false;
			}
			else
			{
				CheckTextureAndPlay(left, running);
				yield return new WaitForSeconds(footstepInterval);
				rightFootstepActive = false;
			}
		}

		private void SetGroundType(bool left)
		{
			 //Determine Terrain
			 if (terrainFinder != null)
			 {   //IS THERE A TERRAIN IN THE SCENE?
				 surfaceIndex = GetMainTexture(transform.position);
				 //Not that it matters, but here we determine what position the Terrain Textures are in.
				 //For example, If you added a grass texture, then a dirt, then a rock, you'd have grass=0, dirt=1, rock=2.
				 whatTexture = terrainData.splatPrototypes[surfaceIndex].texture.name;
				 //Debug.Log(whatTexture);
				 //Instead of messing around with numbers, we'll just check the texture's filename.
			 }

			// Determine floor surface
			RaycastHit surfaceHitLeft;
			if (left)
			{
				Ray aboveLeftFoot = new Ray(leftFoot.transform.position + new Vector3(0, 1.5f, 0), Vector3.down);
				if (Physics.Raycast(aboveLeftFoot, out surfaceHitLeft, 2f, layerMask))
				{
					floor = surfaceHitLeft.transform.gameObject;
				}
			}
			else
			{
				Ray aboveRightFoot = new Ray(rightFoot.transform.position + new Vector3(0, 1.5f, 0), Vector3.down);
				if (Physics.Raycast(aboveRightFoot, out surfaceHitLeft, 2f, layerMask))
				{
					floor = surfaceHitLeft.transform.gameObject;
				}
			}

		}

		private float[] GetTextureMix(Vector3 WorldPos)
		{
			if (terrainFinder != null)
			{
				// calculate which splat map cell the worldPos falls within
				int mapX = (int)(((WorldPos.x - terrainPos.x) / terrainData.size.x) * terrainData.alphamapWidth);
				int mapZ = (int)(((WorldPos.z - terrainPos.z) / terrainData.size.z) * terrainData.alphamapHeight);

				// Clamp the coordinates to ensure they are within valid range
				mapX = Mathf.Clamp(mapX, 0, terrainData.alphamapWidth - 1);
				mapZ = Mathf.Clamp(mapZ, 0, terrainData.alphamapHeight - 1);

				// get the splat data for this cell as a 1x1xN 3d array (where N = number of textures)
				float[,,] splatmapData = terrainData.GetAlphamaps(mapX, mapZ, 1, 1);
				float[] cellMix = new float[splatmapData.GetUpperBound(2) + 1]; //turn splatmap data into float array

				for (int n = 0; n < cellMix.Length; n++)
				{
					cellMix[n] = splatmapData[0, 0, n];
				}

				return cellMix;
			}
			else
			{
				return null;   //THERE'S NO TERRAIN IN THE SCENE! DON'T DO THE ABOVE STUFF.
			}
		}

		//Takes the "GetTextureMix" float array from above and returns the MOST DOMINANT texture at Player's position.
		private int GetMainTexture(Vector3 WorldPos)
		{
			if (terrainFinder != null)
			{   //IS THERE A TERRAIN IN THE SCENE?
				float[] mix = GetTextureMix(WorldPos);
				float maxMix = 0;
				int maxIndex = 0;
				for (int n = 0; n < mix.Length; n++)
				{
					if (mix[n] > maxMix)
					{
						maxIndex = n;
						maxMix = mix[n];
					}
				}
				return maxIndex;
			}
			else return 0;  //THERE'S NO TERRAIN IN THE SCENE! DON'T DO THE ABOVE STUFF.
		}

		//-----------------------------------------------------------------------------------------

		public void CheckTextureAndPlay(bool left, bool running)
		{
			if (playerLocomotion.touchingDeepWater)
			{
				PlayFootstep(left, running, running ? waterDeep_run : waterDeep, "WaterDeep");
				return;
			}
			else if (playerLocomotion.touchingShallowWater)
			{
				PlayFootstep(left, running, running ? waterShallow_run : waterShallow, "WaterShallow");
				return;
			}

			if (floor == null)
			{
				PlayFootstep(left, running, running ? defaults_run : defaults, "Dirt");
				return;
			}

			switch (floor.tag)
			{
				case "Untagged":
					// If Running, dirt_run, else dirt
					PlayFootstep(left, running, running ? dirt_run : dirt, "Dirt");
					break;
				case "Surface_Dirt":
					PlayFootstep(left, running, running ? dirt_run : dirt, "Dirt");
					break;
				case "Surface_Grass":
					PlayFootstep(left, running, running ? grass_run : grass, "Grass");
					break;
				case "Surface_Gravel":
					PlayFootstep(left, running, running ? gravel_run : gravel, "Gravel");
					break;
				case "Surface_Leaves":
					PlayFootstep(left, running, running ? leaves_run : leaves, "Leaves");
					break;
				case "Surface_Metal":
					PlayFootstep(left, running, running ? metal_run : metal, "Metal");
					break;
				case "Surface_Mud":
					PlayFootstep(left, running, running ? mud_run : mud, "Mud");
					break;
				case "Surface_Stone":
				case "Surface_Rock":
					PlayFootstep(left, running, running ? stone_run : stone, "Stone");
					break;
				case "Surface_Sand":
					PlayFootstep(left, running, running ? sand_run : sand, "Sand");
					break;
				case "Surface_Snow":
					PlayFootstep(left, running, running ? snow_run : snow, "Snow");
					break;
				case "Surface_Tile":
					PlayFootstep(left, running, running ? tile_run : tile, "Tile");
					break;
				case "Surface_Water":
					PlayFootstep(left, running, running ? waterShallow_run : waterShallow, "WaterShallow");
					break;
				case "Surface_Wood":
					PlayFootstep(left, running, running ? wood_run : wood, "Wood");
					break;
				case "Terrain":
					string debug;
					if (whatTexture.ToLower().Contains("grass"))
					{ 
						PlayFootstep(left, running, running ? grass_run : grass, "Grass"); 
						debug = "Grass"; 
					}
					else if (whatTexture.ToLower().Contains("leaves"))
					{ 
						PlayFootstep(left, running, running ? leaves_run : leaves, "Leaves"); 
						debug = "Leaves"; 
					}
					else if (whatTexture.ToLower().Contains("dirt"))
					{ 
						PlayFootstep(left, running, running ? dirt_run : dirt, "Dirt");
						debug = "Dirt";
					}
					else if (whatTexture.ToLower().Contains("gravel"))
					{ 
						PlayFootstep(left, running, running ? gravel_run : gravel, "Gravel"); 
						debug = "Gravel"; 
					}
					else if (whatTexture.ToLower().Contains("stone") || whatTexture.Contains("rock") || whatTexture.Contains("catacombs"))
					{ 
						PlayFootstep(left, running, running ? stone_run : stone, "Stone"); 
						debug = "Stone"; 
					}
					else if (whatTexture.ToLower().Contains("mud"))
					{ 
						PlayFootstep(left, running, running ? mud_run : mud, "Mud"); 
						debug = "Mud"; 
					}
					else if (whatTexture.ToLower().Contains("wood"))
					{ 
						PlayFootstep(left, running, running ? wood_run : wood, "Wood"); 
						debug = "Wood";
					}
					else if (whatTexture.ToLower().Contains("metal"))
					{ 
						PlayFootstep(left, running, running ? metal_run : metal, "Metal"); 
						debug = "Metal"; 
					}
					else if (whatTexture.ToLower().Contains("snow"))
					{ 
						PlayFootstep(left, running, running ? snow_run : snow, "Snow"); 
						debug = "Snow";
					}
					else if (whatTexture.ToLower().Contains("tile"))
					{ 
						PlayFootstep(left, running, running ? tile_run : tile, "Tile"); 
						debug = "tile"; 
					}
					else if (whatTexture.ToLower().Contains("sand"))
					{ 
						PlayFootstep(left, running, running ? sand_run : sand, "Sand"); 
						debug = "Sand";
					}
					else if (whatTexture.ToLower().Contains("water")) 
					{ 
						PlayFootstep(left, running, running ? waterShallow_run : waterShallow, "WaterShallow"); 
						debug = "WaterShallow"; 
					}
					else 
					{ 
						PlayFootstep(left, running, running ? dirt_run : dirt, "Dirt"); 
						debug = "Dirt";
					}						
					//Debug.Log("Terrain Type Detected: " + debug + " -> \"" + whatTexture + "\"");
					break;
				default:
					PlayFootstep(left, running, running ? dirt_run : dirt, "Dirt");
					break;
			}
		}

		void PlayFootstep(bool left, bool running, AudioClip[] sounds, string volumeTag)
        {
			// DETERMINE VOLUME
			currentVolume = GetVolume(volumeTag, running);

			// Apply strafing volume multiplier if necessary
			if (!running && cameraHandler.currentLockOnTarget != null) currentVolume *= strafingVolumeMultiplier;
			if (playerLocomotion.isCrouching) currentVolume *= crouchingVolumeMultiplier;

			// Apply volume variance and clamp volume
			currentVolume = Mathf.Clamp(currentVolume + UnityEngine.Random.Range(-volumeVariance, volumeVariance), 0f, 1f);

			//Apply pitch and play sound at corresponding foot	
			pitch = (1.0f + Random.Range(-pitchVariance, pitchVariance));
			if (left)
			{
				footstepAudioSourceL.pitch = pitch;
				if (sounds.Length > 0)
				{
					footstepAudioSourceL.PlayOneShot(sounds[Random.Range(0, sounds.Length)], currentVolume);
				}
				else Debug.LogError("trying to play " + nameof(sounds) + " sounds, but no sounds found in array!");
			}
			else
			{
				footstepAudioSourceR.pitch = pitch;
				if (sounds.Length > 0)
				{
					footstepAudioSourceR.PlayOneShot(sounds[Random.Range(0, sounds.Length)], currentVolume);
				}
				else Debug.LogError("trying to play " + nameof(sounds) + " sounds, but no sounds found in array!");
			}

			// Instantiate Effects, if relevant
			//Debug.Log("Ground type for footstep: " + nameof(sounds));
            #region EFFECTS
            /*if (dirtFX != null)
            {       //Is a prefab in the GameObject box, in the Inspector?
                if (instantiatedFX == true)
                {                       //Code for INSTANTIATING a Particle System at the player's feet.
                    if (currentFoot == ("Left"))
                    {
                        Instantiate(dirtFX, (leftFoot.transform.position + new Vector3(0, 0.05f, 0)), transform.rotation);  //Instantiates the prefab at the left foot + 0.05f (slightly above ground).
                    }
                    else if (currentFoot == ("Right"))
                    {
                        Instantiate(dirtFX, (rightFoot.transform.position + new Vector3(0, 0.05f, 0)), transform.rotation); //Instantiates the prefab at the right foot + 0.05f (slightly above ground).
                    }
                }
                if (toggledFX == true)
                {                           //Code for ENABLING a Particle System at the player's feet.
                    if (currentFoot == ("Left"))
                    {
                        if (dirtFX != null)
                        {
                            dirtFX.SetActive(false);
                            dirtFX.SetActive(true);
                            dirtFX.transform.position = leftFoot.transform.position;
                            dirtRotation = dirtFX.transform.rotation;
                            dirtPos = dirtFX.transform.position;
                        }
                    }
                    else if (currentFoot == ("Right"))
                    {
                        if (dirtFX != null)
                        {
                            dirtFX.SetActive(false);
                            dirtFX.SetActive(true);
                            dirtFX.transform.position = rightFoot.transform.position;
                            dirtRotation = dirtFX.transform.rotation;
                            dirtPos = dirtFX.transform.position;
                        }
                    }
                }
            }*/


            /*if (snowFX != null)
			{       //Is a Prefab in the SnowFX GameObject slot, in the Inspector?
				if (instantiatedFX == true)
				{                   //Code for INSTANTIATING a Particle System at the player's feet.
					if (currentFoot == ("Left"))
					{
						Instantiate(snowFX, (leftFoot.transform.position + new Vector3(0, 0.05f, 0)), transform.rotation);  //Instantiates the prefab at the left foot + 0.05f (slightly above ground).
					}
					else if (currentFoot == ("Right"))
					{
						Instantiate(snowFX, (rightFoot.transform.position + new Vector3(0, 0.05f, 0)), transform.rotation); //Instantiates the prefab at the right foot + 0.05f (slightly above ground).
					}
				}
				if (toggledFX == true)
				{                           //Code for ENABLING a Particle System at the player's feet.
					if (currentFoot == ("Left"))
					{
						if (snowFX != null)
						{
							snowFX.SetActive(false);
							snowFX.SetActive(true);
							snowFX.transform.position = leftFoot.transform.position;
							snowRotation = snowFX.transform.rotation;
							snowPos = snowFX.transform.position;
						}
					}
					else if (currentFoot == ("Right"))
					{
						if (snowFX != null)
						{
							snowFX.SetActive(false);
							snowFX.SetActive(true);
							snowFX.transform.position = rightFoot.transform.position;
							snowRotation = snowFX.transform.rotation;
							snowPos = snowFX.transform.position;
						}
					}
				}
			}*/

            /*if (waterFX != null)
			{       //Water is a bit different than the others: We want the water splash prefab to be placed at the SURFACE of the water, not at the bottom of the feet.
				if (instantiatedFX == true)
				{                       //Code for INSTANTIATING a Particle System at the player's feet.
					if (currentFoot == ("Left"))
					{
						RaycastHit leftWaterHit;
						Ray aboveWaterLeft = new Ray(leftFoot.transform.position + new Vector3(0, 1.5f, 0), Vector3.down);
						LayerMask layerMask = ~(1 << 18) | (1 << 19);   //Here we ignore layer 18 and 19 (Player and NPCs). We want the raycast to hit the ground, not people.
						if (Physics.Raycast(aboveWaterLeft, out leftWaterHit, 2f, layerMask))
						{
							Instantiate(waterFX, (leftWaterHit.point + new Vector3(0, 0.05f, 0)), transform.rotation);
						}
					}
					else if (currentFoot == ("Right"))
					{
						RaycastHit rightWaterHit;
						Ray aboveWaterRight = new Ray(rightFoot.transform.position + new Vector3(0, 1.5f, 0), Vector3.down);
						LayerMask layerMask = ~(1 << 18) | (1 << 19);   //Here we ignore layer 18 and 19 (Player and NPCs). We want the raycast to hit the ground, not people.
						if (Physics.Raycast(aboveWaterRight, out rightWaterHit, 2f, layerMask))
						{
							Instantiate(waterFX, (rightWaterHit.point + new Vector3(0, 0.05f, 0)), transform.rotation);
						}
					}
				}
				if (toggledFX == true)
				{                           //Code for ENABLING a Particle System at the player's feet.
					if (currentFoot == ("Left"))
					{
						RaycastHit leftWaterHit;
						Ray aboveWaterLeft = new Ray(leftFoot.transform.position + new Vector3(0, 1.5f, 0), Vector3.down);
						LayerMask layerMask = ~(1 << 18) | (1 << 19);   //Here we ignore layer 18 and 19 (Player and NPCs). We want the raycast to hit the ground, not people.
						if (Physics.Raycast(aboveWaterLeft, out leftWaterHit, 2f, layerMask))
						{
							waterFX.SetActive(false);
							waterFX.SetActive(true);
							waterFX.transform.position = leftWaterHit.point;
							waterRotation = waterFX.transform.rotation;
							waterPos = waterFX.transform.position;
						}
					}
					else if (currentFoot == ("Right"))
					{
						RaycastHit rightWaterHit;
						Ray aboveWaterRight = new Ray(rightFoot.transform.position + new Vector3(0, 1.5f, 0), Vector3.down);
						LayerMask layerMask = ~(1 << 18) | (1 << 19);   //Here we ignore layer 18 and 19 (Player and NPCs). We want the raycast to hit the ground, not people.
						if (Physics.Raycast(aboveWaterRight, out rightWaterHit, 2f, layerMask))
						{
							waterFX.SetActive(false);
							waterFX.SetActive(true);
							waterFX.transform.position = rightWaterHit.point;
							waterRotation = waterFX.transform.rotation;
							waterPos = waterFX.transform.position;
						}
					}
				}
			}*/
            #endregion
        }

		float GetVolume(string volumeTag, bool running)
		{
			// Determine the volume based on the tag and whether the character is running or not
			return volumeTag switch
			{
				"Mud" => running ? mudRunningVolume : mudVolume,
				"Dirt" => running ? dirtRunningVolume : dirtVolume,
				"Wood" => running ? woodRunningVolume : woodVolume,
				"Leaves" => running ? leavesRunningVolume : leavesVolume,
				"Tile" => running ? tileRunningVolume : tileVolume,
				"Gravel" => running ? gravelRunningVolume : gravelVolume,
				"Snow" => running ? snowRunningVolume : snowVolume,
				"Sand" => running ? sandRunningVolume : sandVolume,
				"Grass" => running ? grassRunningVolume : grassVolume,
				"Stone" => running ? stoneRunningVolume : stoneVolume,
				"Metal" => running ? metalRunningVolume : metalVolume,
				"WaterShallow" => running ? shallowWaterRunningVolume : shallowWaterVolume,
				"WaterDeep" => running ? deepWaterRunningVolume : deepWaterVolume,
				_ => defaultVolume
			};
		}

	}
}