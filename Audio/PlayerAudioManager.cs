using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace etchebarren
{
    public class PlayerAudioManager : MonoBehaviour
    {
        [System.Serializable]
        public struct ItemAudio
        {
            public AudioClip audioClip;
            public float delay;
            public float volume;
        }

        [Header("AUDIO SOURCES")]
        public AudioSource combat;
        public AudioSource movement;
        public AudioSource weapon;
        public AudioSource vocal;
        public AudioSource swimming;

        [Header("REFERENCES")]
        public PlayerInventory playerInventory;
        public GameObject groundCheckStart;
        public static GameObject floor;     //what are we standing on?
        public LayerMask layerMask;
        private GameObject terrainFinder;   //Stores the Terrain GameObject out of the Scene for use later.
        private Terrain terrain;            //Your Terrain (if one's in your scene)
        private TerrainData terrainData;    //Lets us get to the Terrain's splatmap.
        private Vector3 terrainPos;         //Where are we on the Splatmap?
        public static int surfaceIndex = 0; //The order in which your textures were added to your Terrain.
        private string whatTexture;         //Holds the FILENAMEs of the Textures in your Terrain.

        public bool rollAudioActive = false;
        private float maxRollInterval = 0.5f;

        [Header("JUMP/LAND AUDIO SETTINGS")]
        [Range(0.0f, 1.0f)]
        public float jumpLandVolume = 1.0f;             //Volume slider bar; set this between 0 and 1 in the Inspector.
        [Space(5.0f)]
        public float waterVolumeMultiplier = 0.25f;             //Volume slider bar; set this between 0 and 1 in the Inspector.
        [Space(5.0f)]
        [Range(0.0f, 0.2f)]
        public float jumpLandVolumeVariance = 0.04f;    //Variance in volume levels per footstep; set this between 0.0 and 0.2 in the inspector. Default is 0.04f.
        private float jumpLandPitch;
        private float currentJumpLandVolume;
        [Range(0.0f, 0.2f)]
        public float jumpLandPitchVariance = 0.08f;     //Variance in pitch levels per footstep; set this between 0.0 and 0.2 in the inspector. Default is 0.08f.
        public bool canPlayJumpAudio = true;
        public bool canPlayLandAudio = true;

        [Header("SLIDING AUDIO SETTINGS")]
        [Range(0.0f, 1.0f)]
        public float slidingVolume = 0.5f;             //Volume slider bar; set this between 0 and 1 in the Inspector.
        public AudioClip slidingClip;

        [Header("WEAPON ATTACK AUDIO SETTINGS")]
        [Range(0.0f, 1.0f)]
        public float weaponVolume = 0.5f;             //Volume slider bar; set this between 0 and 1 in the Inspector.
        [Range(0.0f, 0.2f)]
        public float weaponVolumeVariance = 0.04f;    //Variance in volume levels per footstep; set this between 0.0 and 0.2 in the inspector. Default is 0.04f.
        private float weaponPitch;
        private float currentWeaponVolume;
        [Range(0.0f, 0.2f)]
        public float weaponPitchVariance = 0.08f;     //Variance in pitch levels per footstep; set this between 0.0 and 0.2 in the inspector. Default is 0.08f.

        [Header("SHEATHE AUDIO SETTINGS")]
        [Range(0.0f, 1.0f)]
        public float sheatheVolume = 0.5f;             //Volume slider bar; set this between 0 and 1 in the Inspector.
        [Range(0.0f, 1.0f)]
        public float unsheatheVolume = 0.75f;             //Volume slider bar; set this between 0 and 1 in the Inspector.
        [Range(0.0f, 0.2f)]
        public float sheatheVolumeVariance = 0.04f;    //Variance in volume levels per footstep; set this between 0.0 and 0.2 in the inspector. Default is 0.04f.
        private float sheathePitch;
        private float currentSheatheVolume;
        [Range(0.0f, 0.2f)]
        public float sheathePitchVariance = 0.08f;     //Variance in pitch levels per footstep; set this between 0.0 and 0.2 in the inspector. Default is 0.08f.

        [Header("WHISTLE/CLICK AUDIO SETTINGS")]
        [Range(0.0f, 1.0f)]
        public float whistleVolume = 0.5f;             //Volume slider bar; set this between 0 and 1 in the Inspector.
        [Range(0.0f, 1.0f)]
        public float whistlePitch = 1.0f;
        [Range(0.0f, 0.2f)]
        public float whistleVolumeVariance = 0.04f;    //Variance in volume levels per footstep; set this between 0.0 and 0.2 in the inspector. Default is 0.04f.
        [Range(0.0f, 0.2f)]
        public float whistlePitchVariance = 0.08f;     //Variance in pitch levels per footstep; set this between 0.0 and 0.2 in the inspector. Default is 0.08f.
        [Space(5.0f)]
        public float clickVolume = 0.75f;             //Volume slider bar; set this between 0 and 1 in the Inspector.
        public float clickPitch = 1.0f;
        [Range(0.0f, 0.2f)]
        public float clickVolumeVariance = 0.04f;    //Variance in volume levels per footstep; set this between 0.0 and 0.2 in the inspector. Default is 0.04f.
        [Range(0.0f, 0.2f)]
        public float clickPitchVariance = 0.08f;     //Variance in pitch levels per footstep; set this between 0.0 and 0.2 in the inspector. Default is 0.08f.

        [Header("Items")]
        public ItemAudio[] itemAudios;

        [Header("AUDIO CLIPS")]
        public AudioClip[] weaponSwing;
        public AudioClip weaponSlam;
        [Range(0.0f, 1.0f)]
        public float weaponSlamVolume = 1.0f;
        public AudioClip roll;
        [Range(0.0f, 1.0f)]
        public float rollVolume = 1.0f;
        public AudioClip backstep;
        [Range(0.0f, 1.0f)]
        public float backstepVolume = 1.0f;
        public AudioClip[] sheathe;
        public AudioClip[] unsheathe;
        public AudioClip shieldEquip;
        public AudioClip shieldUnequip;
        [Space(5.0f)]
        [Range(0.0f, 1.0f)]
        public float generalSpellVolume = 0.5f;
        public AudioClip generalSpell;
        [Space(5.0f)]
        [Range(0.0f, 1.0f)]
        public float castFrostshardVolume = 0.3f;
        public AudioClip castFrostshard;
        [Space(5.0f)]
        [Range(0.0f, 1.0f)]
        public float castFireballVolume = 0.3f;
        public AudioClip castFireball;
        [Space(5.0f)]
        [Range(0.0f, 1.0f)]
        public float castZapshockVolume = 0.3f;
        public AudioClip castZapshock;
        [Space(5.0f)]
        [Range(0.0f, 1.0f)]
        public float mendStartVolume = 0.8f;
        public AudioClip mendStart;
        [Space(5.0f)]
        [Range(0.0f, 1.0f)]
        public float rejuvenateStartVolume = 1.0f;
        public AudioClip rejuvenateStart;
        [Range(0.0f, 1.0f)]
        public float rejuvenateTickVolume = 1.0f;
        public float itemTickVolume = 1.0f;
        public AudioClip rejuvenateTick;
        [Space(5.0f)]
        [Range(0.0f, 1.0f)]
        public float arcanaLuminaStartVolume = 0.8f;
        public AudioClip arcanaLuminaStart;
        [Space(5.0f)]
        public AudioClip meteorStart;
        [Range(0.0f, 1.0f)]
        public float meteorCastVolume = 1.0f;
        public AudioClip meteorCast;
        [Space(5.0f)]
        public AudioClip glacialBurstStart;
        public AudioClip glacialBurstStart2;
        public AudioClip glacialBurstCast;
        [Space(5.0f)]
        [Range(0.0f, 1.0f)]
        public float stormguardStartVolume = 1.0f;
        public AudioClip stormguardStart;
        public float stormguardVolume = 1.0f;
        public AudioClip stormguard;
        public GameObject shockguardFlash;
        [Space(5.0f)]
        public AudioClip fireInfusion;
        public AudioClip iceInfusion;
        public AudioClip shockInfusion;
        [Space(5.0f)]
        public AudioClip[] hitBlocked;
        [Range(0.0f, 1.0f)]
        public float hitBlockedVolume = 1.0f;
        public AudioClip[] getHitMelee;
        [Range(0.0f, 1.0f)]
        public float physicalHitVolume = 1.0f;
        public AudioClip[] getHitMagic;
        [Range(0.0f, 1.0f)]
        public float magicHitVolume = 1.0f;
        public AudioClip[] marsVoiceHit;
        [Range(0.0f, 1.0f)]
        public float marsVoiceHitVolume = 1.0f;
        public AudioClip[] venusVoiceHit;
        [Range(0.0f, 1.0f)]
        public float venusVoiceHitVolume = 1.0f;
        [Space(5.0f)]
        public AudioClip[] default_jump;
        public AudioClip[] default_land;
        [Space(5.0f)]
        public AudioClip[] dirt_jump;
        public AudioClip[] dirt_land;
        [Space(5.0f)]
        public AudioClip[] grass_jump;
        public AudioClip[] grass_land;
        [Space(5.0f)]
        public AudioClip[] gravel_jump;
        public AudioClip[] gravel_land;
        [Space(5.0f)]
        public AudioClip[] leaves_jump;
        public AudioClip[] leaves_land;
        [Space(5.0f)]
        public AudioClip[] metal_jump;
        public AudioClip[] metal_land;
        [Space(5.0f)]
        public AudioClip[] mud_jump;
        public AudioClip[] mud_land;
        [Space(5.0f)]
        public AudioClip[] stone_jump;
        public AudioClip[] stone_land;
        [Space(5.0f)]
        public AudioClip[] sand_jump;
        public AudioClip[] sand_land;
        [Space(5.0f)]
        public AudioClip[] snow_jump;
        public AudioClip[] snow_land;
        [Space(5.0f)]
        public AudioClip[] tile_jump;
        public AudioClip[] tile_land;
        [Space(5.0f)]
        public AudioClip[] water_jump;
        public AudioClip[] water_land;
        [Space(5.0f)]
        public AudioClip[] wood_jump;
        public AudioClip[] wood_land;
        [Space(5.0f)]
        public AudioClip[] whistleCallHorse;
        public AudioClip[] clickSpeedUp;
        public AudioClip[] clickIgnore;
        public AudioClip whistleStop;
        public bool canPlayWhistleAudio = true;
        public float whistleAudioCooldown = 0.5f;
        [Space(5.0f)]
        public AudioClip swimmingForward;
        [Range(0.0f, 1.0f)]
        public float swimmingForwardVolume = 0.5f;
        public AudioClip swimmingStill;
        [Range(0.0f, 1.0f)]
        public float swimmingStillVolume = 0.5f;
        [Range(0.0f, 1.0f)]
        public float climbingVolume = 0.8f;
        public AudioClip[] climbing;

        [Header("DIGGING")]
        public AudioClip[] shovelinDirt;
        public float shovelinDirtVolume = 1.0f;
        public AudioClip[] tossDirt;
        public float tossDirtVolume = 1.0f;
        public AudioClip[] shovelHitHardSurface;
        public float shovelHitHardSurfaceVolume = 1.0f;

        //Start
        void Start()
        {
            terrainFinder = GameObject.FindGameObjectWithTag("Terrain");

            if (terrainFinder != null)
            {   //IS THERE A TERRAIN IN THE SCENE?
                terrain = Terrain.activeTerrain;
                terrainData = terrain.terrainData;
                terrainPos = terrain.transform.position;
            }
        }

        private void SetGroundType()
        {
            // Determine Terrain
            if (terrainFinder != null)
            {   //IS THERE A TERRAIN IN THE SCENE?
                surfaceIndex = GetMainTexture(transform.position);
                //Not that it matters, but here we determine what position the Terrain Textures are in.
                //For example, If you added a grass texture, then a dirt, then a rock, you'd have grass=0, dirt=1, rock=2.
                whatTexture = terrainData.splatPrototypes[surfaceIndex].texture.name;
                //Instead of messing around with numbers, we'll just check the texture's filename.
            }

            // Determine floor surface
            RaycastHit surfaceHitLeft;

            Ray startingPos = new Ray(groundCheckStart.transform.position + new Vector3(0, 1.5f, 0), Vector3.down);
            if (Physics.Raycast(startingPos, out surfaceHitLeft, 2f, layerMask))
            {
                floor = surfaceHitLeft.transform.gameObject;
            }
          
        }

        #region Spells

        public void PlayGeneralSpell()
        {
            combat.volume = 1.0f;
            combat.PlayOneShot(generalSpell, generalSpellVolume);
        }

        public void PlayFrostshardCast()
        {
            combat.volume = 1.0f;
            combat.PlayOneShot(castFrostshard, castFrostshardVolume);
            combat.PlayOneShot(generalSpell, generalSpellVolume / 2);
        }

        public void PlayFireballCast()
        {
            combat.volume = 1.0f;
            combat.PlayOneShot(castFireball, castFireballVolume);
            combat.PlayOneShot(generalSpell, generalSpellVolume / 2);
        }

        public void PlayZapshockCast()
        {
            combat.volume = 1.0f;
            combat.PlayOneShot(generalSpell, generalSpellVolume);
            combat.PlayOneShot(castZapshock, castZapshockVolume);
        }

        public void PlayMeteorAudio()
        {
            StartCoroutine(MeteorAudio());

            IEnumerator MeteorAudio()
            {
                combat.volume = 1.0f;
                combat.clip = meteorStart;
                combat.Play();
                yield return new WaitForSeconds(4.0f);
                combat.PlayOneShot(meteorCast, meteorCastVolume);
                combat.Stop();
            }
        }

        public void PlayGlacialBurstAudio()
        {
            StartCoroutine(GlacialBurstAudio());

            IEnumerator GlacialBurstAudio()
            {
                combat.volume = 1.0f;
                movement.volume = 0.5f;

                combat.clip = glacialBurstStart;
                combat.Play();

                movement.clip = glacialBurstStart2;
                movement.Play();

                yield return new WaitForSeconds(3.0f);
                combat.PlayOneShot(glacialBurstCast, 1.0f);
                combat.Stop();

                yield return new WaitForSeconds(0.5f);
                movement.Stop();
            }
        }

        public void PlayMendAudio()
        {
            StartCoroutine(MendAudio());

            IEnumerator MendAudio()
            {
                combat.volume = 1.0f;
                yield return new WaitForSeconds(0.5f);
                combat.PlayOneShot(mendStart, mendStartVolume);
            }
        }

        public void PlayArcanaLuminaAudio()
        {
            StartCoroutine(ArcanaLuminaAudio());

            IEnumerator ArcanaLuminaAudio()
            {
                combat.volume = 1.0f;
                yield return new WaitForSeconds(0.5f);
                combat.PlayOneShot(arcanaLuminaStart, arcanaLuminaStartVolume);
            } 
        }

        public void PlayRejuvenateAudio()
        {
            combat.volume = 1.0f;
            combat.PlayOneShot(rejuvenateStart, rejuvenateStartVolume);
        }

        public void PlayRejuvenateTickAudio()
        {
            combat.volume = 1.0f;
            combat.PlayOneShot(rejuvenateTick, rejuvenateTickVolume);
        }

        public void PlayStormguardAudio()
        {
            StartCoroutine(StormguardAudio());
        }

        private IEnumerator StormguardAudio()
        {
            combat.volume = 1.0f;
            combat.PlayOneShot(stormguardStart, stormguardStartVolume);

            yield return new WaitForSeconds(0.7f);

            shockguardFlash.SetActive(true);
            combat.PlayOneShot(stormguard, stormguardVolume);
        }

        public void PlayFireInfusionAudio()
        {
            weapon.volume = 1.0f;
            weapon.PlayOneShot(fireInfusion, 1.0f);
        }

        public void PlayIceInfusionAudio()
        {
            weapon.volume = 1.0f;
            weapon.PlayOneShot(iceInfusion, 1.0f);
        }

        public void PlayShockInfusionAudio()
        {
            weapon.volume = 1.0f;
            weapon.PlayOneShot(shockInfusion, 1.0f);
        }

        #endregion

        #region Combat

        public void PlayCallHorseWhistle()
        {
            if (canPlayWhistleAudio)
            {
                canPlayWhistleAudio = false;
                vocal.volume = Mathf.Clamp(whistleVolume + Random.Range(-whistleVolumeVariance, whistleVolumeVariance), 0f, 1f);
                vocal.pitch = Mathf.Clamp(whistlePitch + Random.Range(-whistlePitchVariance, whistlePitchVariance), 0f, 1f);
                int randomIndex = Random.Range(0, whistleCallHorse.Length);
                vocal.PlayOneShot(whistleCallHorse[randomIndex], 0.5f);
                StartCoroutine(WhistleAudioCooldown());
            }
        }

        public void PlaySpeedUpHorseClick()
        {
            if (canPlayWhistleAudio)
            {
                canPlayWhistleAudio = false;
                vocal.volume = Mathf.Clamp(clickVolume + Random.Range(-clickVolumeVariance, clickVolumeVariance), 0f, 1f);
                vocal.pitch = Mathf.Clamp(clickPitch + Random.Range(-clickPitchVariance, clickPitchVariance), 0f, 1f);
                int randomIndex = Random.Range(0, clickSpeedUp.Length);
                vocal.PlayOneShot(clickSpeedUp[randomIndex], 0.5f);
                StartCoroutine(WhistleAudioCooldown());
            }
        }

        public void PlayIgnoreHorseClick()
        {
            if (canPlayWhistleAudio)
            {
                canPlayWhistleAudio = false;
                vocal.volume = Mathf.Clamp(clickVolume + Random.Range(-clickVolumeVariance, clickVolumeVariance), 0f, 1f);
                vocal.pitch = Mathf.Clamp(clickPitch + Random.Range(-clickPitchVariance, clickPitchVariance), 0f, 1f);
                int randomIndex = Random.Range(0, clickIgnore.Length);
                vocal.PlayOneShot(clickIgnore[randomIndex], 0.5f);
                StartCoroutine(WhistleAudioCooldown());
            }

        }

        public void PlayStopHorseWhistle()
        {
            if (canPlayWhistleAudio)
            {
                canPlayWhistleAudio = false;
                vocal.volume = Mathf.Clamp(whistleVolume + Random.Range(-whistleVolumeVariance, whistleVolumeVariance), 0f, 1f);
                vocal.pitch = Mathf.Clamp(whistlePitch + Random.Range(-whistlePitchVariance, whistlePitchVariance), 0f, 1f);
                vocal.PlayOneShot(whistleStop, 0.5f);
                StartCoroutine(WhistleAudioCooldown());
            }
        }

        private IEnumerator WhistleAudioCooldown()
        {
            yield return new WaitForSeconds(whistleAudioCooldown);
            canPlayWhistleAudio = true;
        }

        public void PlayWeaponSwingAudio()
        {
            weapon.volume = 1.0f;

            currentWeaponVolume = Mathf.Clamp(weaponVolume + UnityEngine.Random.Range(-weaponVolumeVariance, weaponVolumeVariance), 0f, 1f);

            weaponPitch = (1.0f + Random.Range(-weaponPitchVariance, weaponPitchVariance));

            weapon.pitch = weaponPitch;

            if (weaponSwing.Length > 0)
            {
                weapon.PlayOneShot(weaponSwing[Random.Range(0, weaponSwing.Length)], currentWeaponVolume);
            }
            else Debug.LogError("trying to play " + nameof(weaponSwing) + " sounds, but no sounds found in array!");
        }

        public void PlaySheatheAudio()
        {
            if (playerInventory.equippedWeapon == playerInventory.emptyWeapon) return;

            weapon.volume = 1.0f;

            currentSheatheVolume = Mathf.Clamp(sheatheVolume + UnityEngine.Random.Range(-sheatheVolumeVariance, sheatheVolumeVariance), 0f, 1f);

            sheathePitch = (1.0f + Random.Range(-sheathePitchVariance, sheathePitchVariance));

            weapon.pitch = sheathePitch;

            if (sheathe.Length > 0)
            {
                weapon.PlayOneShot(sheathe[Random.Range(0, sheathe.Length)], currentSheatheVolume);
            }
            else Debug.LogError("trying to play " + nameof(sheathe) + " sounds, but no sounds found in array!");
        }

        public void PlayUnsheatheAudio()
        {
            if (playerInventory.equippedWeapon == playerInventory.emptyWeapon) return;

            weapon.volume = 1.0f;

            currentSheatheVolume = Mathf.Clamp(unsheatheVolume + UnityEngine.Random.Range(-sheatheVolumeVariance/2, sheatheVolumeVariance), 0f, 1f);

            sheathePitch = (1.0f + Random.Range(-sheathePitchVariance, sheathePitchVariance));

            weapon.pitch = sheathePitch;

            if (unsheathe.Length > 0)
            {
                weapon.PlayOneShot(unsheathe[Random.Range(0, unsheathe.Length)], currentSheatheVolume);
            }
            else Debug.LogError("trying to play " + nameof(unsheathe) + " sounds, but no sounds found in array!");
        }

        public void PlayShieldEquipAudio()
        {
            if (playerInventory.equippedShield == playerInventory.emptyShield) return;

            weapon.volume = 1.0f;

            float currentShieldVolume = Mathf.Clamp(0.5f + UnityEngine.Random.Range(-sheatheVolumeVariance, sheatheVolumeVariance), 0f, 1f);

            float shieldPitch = (1.0f + Random.Range(-sheathePitchVariance, sheathePitchVariance));

            weapon.pitch = shieldPitch;

            weapon.PlayOneShot(shieldEquip, currentShieldVolume);
        }

        public void PlayShieldUnequipAudio()
        {
            if (playerInventory.equippedShield == playerInventory.emptyShield) return;

            weapon.volume = 1.0f;

            float currentShieldVolume = Mathf.Clamp(0.5f + UnityEngine.Random.Range(-sheatheVolumeVariance, sheatheVolumeVariance), 0f, 1f);

            float shieldPitch = (1.0f + Random.Range(-sheathePitchVariance, sheathePitchVariance));

            weapon.pitch = shieldPitch;

            weapon.PlayOneShot(shieldUnequip, currentShieldVolume);
        }


        public void PlayGetHitAudio(bool melee=true)
        {
            int randomIndex = 0;
            combat.volume = 1.0f;

            if (melee)
            {
                randomIndex = Random.Range(0, getHitMelee.Length);
                combat.PlayOneShot(getHitMelee[randomIndex], physicalHitVolume);
            }
            else
            {
                randomIndex = Random.Range(0, getHitMagic.Length);
                combat.PlayOneShot(getHitMagic[randomIndex], magicHitVolume);
            }

        }

        public void PlayBlockedAudio()
        {
            combat.volume = 1.0f;
            int randomIndex = Random.Range(0, hitBlocked.Length);
            combat.PlayOneShot(hitBlocked[randomIndex], hitBlockedVolume);
        }

        public void PlayGetHitVoiceAudio(bool mars)
        {
            StartCoroutine(GetHitVoiceAudio());

            IEnumerator GetHitVoiceAudio()
            {
                yield return new WaitForSeconds(0.15f);
                int randomIndex = 0;
                combat.volume = 1.0f;

                if (mars)
                {
                    randomIndex = Random.Range(0, marsVoiceHit.Length);
                    combat.PlayOneShot(marsVoiceHit[randomIndex], marsVoiceHitVolume);
                }
                else
                {
                    randomIndex = Random.Range(0, venusVoiceHit.Length);
                    combat.PlayOneShot(venusVoiceHit[randomIndex], venusVoiceHitVolume);
                }
            }
        }

        public void PlayPlungeAttackAudio()
        {
            weapon.volume = 1.0f;
            weapon.PlayOneShot(weaponSlam, weaponSlamVolume); 
        }

        public void PlayRollAudio()
        {
            if (rollAudioActive) return;
            rollAudioActive = true;
            StartCoroutine(RollAudio());
        }

        public void PlaySlideAudio()
        {
            movement.volume = 1.0f;
            movement.PlayOneShot(slidingClip, slidingVolume);
        }

        private IEnumerator RollAudio()
        {
            movement.volume = 1.0f;
            movement.pitch = 1.0f + Random.Range(-0.08f, 0.08f);
            movement.PlayOneShot(roll, rollVolume);
            yield return new WaitForSeconds(maxRollInterval);
            rollAudioActive = false;
        }

        public void PlayBackstepAudio()
        {
            movement.volume = 1.0f;
            movement.pitch = 1.0f + Random.Range(-0.08f, 0.08f);
            movement.PlayOneShot(backstep, backstepVolume);
        }


        #endregion

        #region Movement

        private float[] GetTextureMix(Vector3 worldPos)
        {
            if (terrainFinder == null)
                return null;

            Vector3 terrainSize = terrainData.size;

            float relativeX = worldPos.x - terrainPos.x;
            float relativeZ = worldPos.z - terrainPos.z;

            // Convert to normalized coordinates (can go <0 or >1)
            float normX = relativeX / terrainSize.x;
            float normZ = relativeZ / terrainSize.z;

            // Convert to alphamap coords
            int mapX = (int)(normX * terrainData.alphamapWidth);
            int mapZ = (int)(normZ * terrainData.alphamapHeight);

            // Clamp to valid range
            mapX = Mathf.Clamp(mapX, 0, terrainData.alphamapWidth - 1);
            mapZ = Mathf.Clamp(mapZ, 0, terrainData.alphamapHeight - 1);

            float[,,] splatmapData = terrainData.GetAlphamaps(mapX, mapZ, 1, 1);

            float[] cellMix = new float[splatmapData.GetUpperBound(2) + 1];
            for (int n = 0; n < cellMix.Length; n++)
            {
                cellMix[n] = splatmapData[0, 0, n];
            }

            return cellMix;
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

        public void PlayJumpAudio()
        {
            if (!canPlayJumpAudio) return;

            StartCoroutine(ResetFlag(ActionType.Jump));

            SetGroundType();

            if (floor == null)
            {
                PlayJumpLandAudio(default_jump);
                return;
            }

            switch (floor.tag)
            {
                case "Untagged":
                    PlayJumpLandAudio(default_jump);
                    break;
                case "Surface_Dirt":
                    PlayJumpLandAudio(dirt_jump);
                    break;
                case "Surface_Grass":
                    PlayJumpLandAudio(grass_jump);
                    break;
                case "Surface_Gravel":
                    PlayJumpLandAudio(gravel_jump);
                    break;
                case "Surface_Leaves":
                    PlayJumpLandAudio(leaves_jump);
                    break;
                case "Surface_Metal":
                    PlayJumpLandAudio(metal_jump);
                    break;
                case "Surface_Mud":
                    PlayJumpLandAudio(mud_jump);
                    break;
                case "Surface_Stone":
                case "Surface_Rock":
                    PlayJumpLandAudio(stone_jump);
                    break;
                case "Surface_Sand":
                    PlayJumpLandAudio(sand_jump);
                    break;
                case "Surface_Snow":
                    PlayJumpLandAudio(snow_jump);
                    break;
                case "Surface_Tile":
                    PlayJumpLandAudio(tile_jump);
                    break;
                case "Surface_Water":
                    PlayJumpLandAudio(water_jump);
                    break;
                case "Surface_Wood":
                    PlayJumpLandAudio(wood_jump);
                    break;
                case "Terrain":
                    if (whatTexture.Contains("grass") == true || whatTexture.Contains("Grass") == true || whatTexture.Contains("GRASS") == true)
                        PlayJumpLandAudio(grass_jump);
                    else if (whatTexture.Contains("leaves") == true || whatTexture.Contains("Leaves") == true || whatTexture.Contains("LEAVES") == true)
                        PlayJumpLandAudio(leaves_jump);
                    else if (whatTexture.Contains("dirt") == true || whatTexture.Contains("Dirt") == true || whatTexture.Contains("DIRT") == true)
                        PlayJumpLandAudio(dirt_jump);
                    else if (whatTexture.Contains("gravel") == true || whatTexture.Contains("Gravel") == true || whatTexture.Contains("GRAVEL") == true)
                        PlayJumpLandAudio(gravel_jump);
                    else if (whatTexture.Contains("stone") == true || whatTexture.Contains("Stone") == true || whatTexture.Contains("STONE") == true
                          || whatTexture.Contains("rock") == true || whatTexture.Contains("Rpck") == true || whatTexture.Contains("ROCK") == true)
                        PlayJumpLandAudio(stone_jump);
                    else if (whatTexture.Contains("mud") == true || whatTexture.Contains("Mud") == true || whatTexture.Contains("MUD") == true)
                        PlayJumpLandAudio(mud_jump);
                    else if (whatTexture.Contains("wood") == true || whatTexture.Contains("Wood") == true || whatTexture.Contains("WOOD") == true)
                        PlayJumpLandAudio(wood_jump);
                    else if (whatTexture.Contains("metal") == true || whatTexture.Contains("Metal") == true || whatTexture.Contains("METAL") == true)
                        PlayJumpLandAudio(metal_jump);
                    else if (whatTexture.Contains("snow") == true || whatTexture.Contains("Snow") == true || whatTexture.Contains("SNOW") == true)
                        PlayJumpLandAudio(snow_jump);
                    else if (whatTexture.Contains("tile") == true || whatTexture.Contains("Tile") == true || whatTexture.Contains("TILE") == true)
                        PlayJumpLandAudio(tile_jump);
                    else if (whatTexture.Contains("sand") == true || whatTexture.Contains("Sand") == true || whatTexture.Contains("SAND") == true)
                        PlayJumpLandAudio(sand_jump);
                    else if (whatTexture.Contains("water") == true || whatTexture.Contains("Water") == true || whatTexture.Contains("WATER") == true)
                        PlayJumpLandAudio(water_jump, true);
                    else PlayJumpLandAudio(default_jump);
                    break;
                default:
                    PlayJumpLandAudio(default_jump);
                    break;
            }
        }

        public void PlayLandAudio()
        {
            if (!canPlayLandAudio) return;

            StartCoroutine(ResetFlag(ActionType.Land));

            SetGroundType();

            if (floor == null)
            {
                PlayJumpLandAudio(default_land);
                return;
            }

            switch (floor.tag)
            {
                case "Untagged":
                    PlayJumpLandAudio(default_land);
                    break;
                case "Surface_Dirt":
                    PlayJumpLandAudio(dirt_land);
                    break;
                case "Surface_Grass":
                    PlayJumpLandAudio(grass_land);
                    break;
                case "Surface_Gravel":
                    PlayJumpLandAudio(gravel_land);
                    break;
                case "Surface_Leaves":
                    PlayJumpLandAudio(leaves_land);
                    break;
                case "Surface_Metal":
                    PlayJumpLandAudio(metal_land);
                    break;
                case "Surface_Mud":
                    PlayJumpLandAudio(mud_land);
                    break;
                case "Surface_Stone":
                case "Surface_Rock":
                    PlayJumpLandAudio(stone_land);
                    break;
                case "Surface_Sand":
                    PlayJumpLandAudio(sand_land);
                    break;
                case "Surface_Snow":
                    PlayJumpLandAudio(snow_land);
                    break;
                case "Surface_Tile":
                    PlayJumpLandAudio(tile_land);
                    break;
                case "Surface_Water":
                    PlayJumpLandAudio(water_land);
                    break;
                case "Surface_Wood":
                    PlayJumpLandAudio(wood_land);
                    break;
                case "Terrain":
                    if (whatTexture.Contains("grass") == true || whatTexture.Contains("Grass") == true || whatTexture.Contains("GRASS") == true)
                        PlayJumpLandAudio(grass_land);
                    else if (whatTexture.Contains("leaves") == true || whatTexture.Contains("Leaves") == true || whatTexture.Contains("LEAVES") == true)
                        PlayJumpLandAudio(leaves_land);
                    else if (whatTexture.Contains("dirt") == true || whatTexture.Contains("Dirt") == true || whatTexture.Contains("DIRT") == true)
                        PlayJumpLandAudio(dirt_land);
                    else if (whatTexture.Contains("gravel") == true || whatTexture.Contains("Gravel") == true || whatTexture.Contains("GRAVEL") == true)
                        PlayJumpLandAudio(gravel_land);
                    else if (whatTexture.Contains("stone") == true || whatTexture.Contains("Stone") == true || whatTexture.Contains("STONE") == true
                          || whatTexture.Contains("rock") == true || whatTexture.Contains("Rpck") == true || whatTexture.Contains("ROCK") == true)
                        PlayJumpLandAudio(stone_land);
                    else if (whatTexture.Contains("mud") == true || whatTexture.Contains("Mud") == true || whatTexture.Contains("MUD") == true)
                        PlayJumpLandAudio(mud_land);
                    else if (whatTexture.Contains("wood") == true || whatTexture.Contains("Wood") == true || whatTexture.Contains("WOOD") == true)
                        PlayJumpLandAudio(wood_land);
                    else if (whatTexture.Contains("metal") == true || whatTexture.Contains("Metal") == true || whatTexture.Contains("METAL") == true)
                        PlayJumpLandAudio(metal_land);
                    else if (whatTexture.Contains("snow") == true || whatTexture.Contains("Snow") == true || whatTexture.Contains("SNOW") == true)
                        PlayJumpLandAudio(snow_land);
                    else if (whatTexture.Contains("tile") == true || whatTexture.Contains("Tile") == true || whatTexture.Contains("TILE") == true)
                        PlayJumpLandAudio(tile_land);
                    else if (whatTexture.Contains("sand") == true || whatTexture.Contains("Sand") == true || whatTexture.Contains("SAND") == true)
                        PlayJumpLandAudio(sand_land);
                    else if (whatTexture.Contains("water") == true || whatTexture.Contains("Water") == true || whatTexture.Contains("WATER") == true)
                        PlayJumpLandAudio(water_land, true);
                    else PlayJumpLandAudio(default_land);
                    break;
                default:
                    PlayJumpLandAudio(default_land);
                    break;
            }
        }

        public void PlayJumpLandAudio( AudioClip[] sounds, bool water=false)
        {
            movement.volume = 1.0f;

            currentJumpLandVolume = Mathf.Clamp(jumpLandVolume + UnityEngine.Random.Range(-jumpLandVolumeVariance, jumpLandVolumeVariance), 0f, 1f);

            jumpLandPitch = (1.0f + Random.Range(-jumpLandPitchVariance, jumpLandPitchVariance));

            movement.pitch = jumpLandPitch;

            if (water) currentJumpLandVolume *= waterVolumeMultiplier;

            if (sounds.Length > 0)
            {
                movement.PlayOneShot(sounds[Random.Range(0, sounds.Length)], currentJumpLandVolume);
            }
            else Debug.LogError("trying to play " + nameof(sounds) + " sounds, but no sounds found in array!");
        }

        public enum ActionType
        {
            Jump,
            Land
        }

        private IEnumerator ResetFlag(ActionType actionType)
        {
            switch (actionType)
            {
                case ActionType.Jump:
                    canPlayJumpAudio = false;
                    yield return new WaitForSeconds(0.5f);
                    canPlayJumpAudio = true;
                    break;
                case ActionType.Land:
                    canPlayLandAudio = false;
                    yield return new WaitForSeconds(0.5f);
                    canPlayLandAudio = true;
                    break;
                default:
                    Debug.LogError("Invalid Flag for ResetFlag");
                    break;
            }
        }

        public void PlayClimbAudio()
        {
            movement.volume = 1.0f;

            currentJumpLandVolume = Mathf.Clamp(climbingVolume + UnityEngine.Random.Range(-jumpLandVolumeVariance, jumpLandVolumeVariance), 0f, 1f);

            jumpLandPitch = (1.0f + Random.Range(-jumpLandPitchVariance, jumpLandPitchVariance));

            movement.pitch = jumpLandPitch;

            if (climbing.Length > 0)
            {
                movement.PlayOneShot(climbing[Random.Range(0, climbing.Length)], currentJumpLandVolume);
            }
            else Debug.LogError("trying to play " + nameof(climbing) + " sounds, but no sounds found in array!");
        }

        #endregion

        #region Swimming

        public void PlaySwimmingAudio(bool status)
        {
            if (status)
            {
                swimming.Play();
            }
            else
            {
                swimming.Stop();
            }
        }

        public void SetSwimmingClip(bool moving)
        {
            if (moving)
            {
                if(swimming.clip != swimmingForward || !swimming.isPlaying)
                {
                    swimming.clip = swimmingForward;
                    swimming.volume = swimmingForwardVolume;
                    swimming.Play();
                }
            }
            else
            {
                if(swimming.clip != swimmingStill || !swimming.isPlaying)
                {
                    swimming.clip = swimmingStill;
                    swimming.volume = swimmingStillVolume;
                    swimming.Play();
                }
            }
        }

        #endregion

        #region Digging
        public void PlayShovelInDirt()
        {
            combat.volume = 1f;
            if (shovelinDirt == null || shovelinDirt.Length == 0)
                return;

            combat.PlayOneShot(shovelinDirt[Random.Range(0, shovelinDirt.Length)], shovelinDirtVolume);
        }

        public void PlayTossDirt()
        {
            combat.volume = 1f;
            if (tossDirt == null || tossDirt.Length == 0)
                return;

            combat.PlayOneShot(tossDirt[Random.Range(0, tossDirt.Length)], tossDirtVolume);
        }

        public void PlayShovelHitHardSurface()
        {
            combat.volume = 1f;
            if (shovelHitHardSurface == null || shovelHitHardSurface.Length == 0)
                return;

            combat.PlayOneShot(shovelHitHardSurface[Random.Range(0, shovelHitHardSurface.Length)], shovelHitHardSurfaceVolume);
        }

        #endregion

        #region Misc
        public void PlayItemAudio(int itemAudioIndex, bool playSecondClip=true)
        {
            StartCoroutine(ItemAudioDelay(itemAudioIndex, playSecondClip));
        }
        
        private IEnumerator ItemAudioDelay(int itemAudioIndex, bool playSecondClip)
        {
            yield return new WaitForSeconds(itemAudios[itemAudioIndex].delay);
            combat.volume = 1.0f;
            Debug.Log("Item audio volume: " + itemAudios[itemAudioIndex].volume + " Item audio index: " + itemAudioIndex);
            combat.PlayOneShot(itemAudios[itemAudioIndex].audioClip, itemAudios[itemAudioIndex].volume);

            if (playSecondClip)
            {
                yield return new WaitForSeconds(0.5f);
                combat.PlayOneShot(rejuvenateTick, itemTickVolume);
            }

        }
        #endregion

    }
}

