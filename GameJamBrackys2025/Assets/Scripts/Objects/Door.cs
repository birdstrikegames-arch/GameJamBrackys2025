using System.Collections;
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


    private float currentHoldTime;
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
                gameObject.tag = "Door";
            }
    }

    private void Update()
    {
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
                GameManager._instance.IncreaseLoudness(doorNoiseLevel);

            if(gameObject.CompareTag("TrapDoor")) // IF DOOR IS A TRAP DOOR
                GameManager._instance.IncreaseLoudness(trapDoorNoiseLevel);

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
        animator.SetBool("isOpen", false);

    }
}
