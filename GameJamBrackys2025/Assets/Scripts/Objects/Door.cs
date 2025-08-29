/*using System.Collections;
using UnityEngine;

public class Door : Interactable
{
    public Animator animator;
    public float requierdHoldTime;
    public float doorCloseTime;
    public float doorNoiseLevel;
    [Header("Trap door attributes")]
    [Tooltip("only if the door is tagged with 'TrapDoor'")]
    public float trapDoorNoiseLevel;

    [Header("Audio")]
    [SerializeField]
    private AudioSource audioSource;
    [SerializeField]
    private AudioClip openSFX;
    [SerializeField]
    private AudioClip closeSFX;
    [SerializeField]
    private AudioClip failedOpenSFX;
    [SerializeField]
    private AudioClip trapDoorSFX;
    [SerializeField]
    private AudioClip unlock;


    private float currentHoldTime;
    private bool isHolding;


    void Awake()
    {
        if (!animator) animator = GetComponent<Animator>();
    }

 
    public override void Interact()
    {
        if (gameObject.CompareTag("Door") || gameObject.CompareTag("TrapDoor")) // open door
        {
            currentHoldTime += Time.deltaTime;
            GameManager._instance.playerUI.openMeterSlider.maxValue = requierdHoldTime;
            GameManager._instance.playerUI.UpdateDoorUI(currentHoldTime);

        }

        else if (gameObject.CompareTag("LockedDoor")) //locked dear logic
            if (GameManager._instance.hasKey)
            {
                audioSource.PlayOneShot(unlock);
                gameObject.tag = "Door";
            }
    }

    private void Update()
    {
        if (InteractionSystem.FocusedDoor != this)
            return;
        if (GameManager._instance.player.GetComponent<InputManager>().InputMap.Player.Interact.WasReleasedThisFrame()
            && !animator.GetBool("isOpen")
            || currentHoldTime >= requierdHoldTime)

            HoldtimeCheck();
    }


    private void HoldtimeCheck()
    {
        if (!gameObject.CompareTag("LockedDoor"))
        {
            if (currentHoldTime < requierdHoldTime) // released early
            {
                audioSource.PlayOneShot(failedOpenSFX);
                GameManager._instance.IncreaseLoudness(doorNoiseLevel);
            }


            if (gameObject.CompareTag("TrapDoor")) // IF DOOR IS A TRAP DOOR
            {
                audioSource.PlayOneShot(trapDoorSFX);
                gameObject.tag = "Door";
                GameManager._instance.IncreaseLoudness(trapDoorNoiseLevel);
            }
            audioSource.PlayOneShot(openSFX);
            currentHoldTime = 0;
            GameManager._instance.playerUI.UpdateDoorUI(currentHoldTime);
            animator.SetBool("isOpen", true);
            StartCoroutine(DoorCloseDelay());

        }
        else
        {
            GameManager._instance.IncreaseLoudness(doorNoiseLevel);
        }
    }

    private IEnumerator DoorCloseDelay()
    {
        yield return new WaitForSeconds(doorCloseTime);
        audioSource.PlayOneShot(closeSFX);
        animator.SetBool("isOpen", false);

    }
}
*/

using System.Collections;
using UnityEngine;

public class Door : Interactable
{
    [Header("Anim / Timing")]
    public Animator animator;
    public float requierdHoldTime = 1.2f;
    public float doorCloseTime = 2f;

    [Header("Noise")]
    public float doorNoiseLevel = 1f;
    public float trapDoorNoiseLevel = 3f; // only if tag == "TrapDoor"

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openSFX, closeSFX, failedOpenSFX, trapDoorSFX, unlock;

    // runtime
    private float currentHoldTime;
    private bool isHolding;

    void Awake()
    {
        if (!animator) animator = GetComponent<Animator>();
    }

    // Called by the player on PRESS (nice to have, but not required with the local-press fallback below)
    public override void Interact()
    {
        TryStartHold();
    }

    private void Update()
    {
        // Allow Update if we're focused OR already holding (latched)
        bool isFocused = (InteractionSystem.FocusedDoor == this);
        if (!isFocused && !isHolding) return;

        var action = GameManager._instance.player
            .GetComponent<InputManager>().InputMap.Player.Interact;

        // --- Start (fallback) ---
        // If the player didn't call Interact() on press, start here when focused.
        if (isFocused && action.WasPressedThisFrame())
            TryStartHold();

        // --- Hold ---
        if (isHolding && action.IsPressed())
        {
            currentHoldTime += Time.deltaTime; // <- this is the bit that was "stuck"
            GameManager._instance.playerUI.UpdateDoorUI(currentHoldTime);
        }

        // --- Finish on release OR if we reached the required time ---
        if (isHolding && (action.WasReleasedThisFrame() || currentHoldTime >= requierdHoldTime))
        {
            isHolding = false;
            FinishHold(early: currentHoldTime < requierdHoldTime);
        }
    }

    private void TryStartHold()
    {
        // Locked door path
        if (CompareTag("LockedDoor"))
        {
            if (GameManager._instance.hasKey)
            {
                if (unlock) audioSource.PlayOneShot(unlock);
                tag = "Door";
            }
            else
            {
                GameManager._instance.IncreaseLoudness(doorNoiseLevel);
                return;
            }
        }

        if (CompareTag("Door") || CompareTag("TrapDoor"))
        {
            isHolding = true;
            currentHoldTime = 0f;
            GameManager._instance.playerUI.openMeterSlider.maxValue = requierdHoldTime;
            GameManager._instance.playerUI.UpdateDoorUI(currentHoldTime);
        }
    }

    private void FinishHold(bool early)
    {
        if (early)
        {
            if (failedOpenSFX) audioSource.PlayOneShot(failedOpenSFX);
            GameManager._instance.IncreaseLoudness(doorNoiseLevel);
        }

        if (CompareTag("TrapDoor"))
        {
            if (trapDoorSFX) audioSource.PlayOneShot(trapDoorSFX);
            tag = "Door";
            GameManager._instance.IncreaseLoudness(trapDoorNoiseLevel);
        }

        currentHoldTime = 0f;
        GameManager._instance.playerUI.UpdateDoorUI(currentHoldTime);

        if (!animator.GetBool("isOpen"))
        {
            if (openSFX) audioSource.PlayOneShot(openSFX);
            animator.SetBool("isOpen", true);
            StartCoroutine(DoorCloseDelay());
        }
    }

    private IEnumerator DoorCloseDelay()
    {
        yield return new WaitForSeconds(doorCloseTime);
        if (closeSFX) audioSource.PlayOneShot(closeSFX);
        animator.SetBool("isOpen", false);
    }
}


