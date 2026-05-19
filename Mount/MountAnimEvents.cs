using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MalbersAnimations;

namespace etchebarren
{
    public class MountAnimEvents : MonoBehaviour
    {
        public StepsManager stepsManager;
        public MountManager mountManager;
        public DamageTriggerCollider attackHitbox;
        public Rigidbody rigidbody;
        private Animator playerAnim;
        public AudioSource audioSource;
        public AudioClip[] neigh;
        public float neighVolume = 0.5f;
        public AudioClip[] jump;
        public float jumpVolume = 0.5f;
        public AudioClip[] land;
        public float landVolume = 0.5f;

        public bool forceKinematic = false;

        void Start()
        {
            if(rigidbody != null && forceKinematic) rigidbody.isKinematic = false;
            playerAnim = AnimatorHandler.instance.anim;
        }

        void OnEnable()
        {
            if (rigidbody != null && forceKinematic) rigidbody.isKinematic = false;
        }

        public void SetAttackHitboxActive(int active)
        {
            if (active == 1) //true
            {
                attackHitbox.enabled = true;
            }
            else
            {
                attackHitbox.enabled = false;
            }
        }

        public void RecoveryAnimComplete()
        {
            mountManager.RecoveryComplete();
        }

        public void Jump()
        {
            Debug.Log("jump AE called");
            playerAnim.SetTrigger("mountJump");
            int randIndex = Random.Range(0, jump.Length);
            audioSource.volume = jumpVolume;
            audioSource.PlayOneShot(jump[randIndex], jumpVolume);
        }

        public void FallLand()
        {
            Debug.Log("Fall Land AE called");
            playerAnim.SetTrigger("mountLand");
            int randIndex = Random.Range(0, land.Length);
            audioSource.volume = landVolume;
            audioSource.PlayOneShot(land[randIndex], landVolume);
        }

        public void Neigh()
        {
            Debug.Log("Neigh AE called");
            playerAnim.SetTrigger("Neigh");
            int randIndex = Random.Range(0, neigh.Length);
            audioSource.volume = neighVolume;
            audioSource.PlayOneShot(neigh[randIndex], neighVolume);
        }

        public void PauseFootsteps()
        {
            stepsManager.pauseFootsteps = true;
        }

        public void ResumeFootsteps()
        {
            stepsManager.pauseFootsteps = false;
        }

        public void FinishEating()
        {
            mountManager.horseAI.enabled = true;
        }
    }

}
