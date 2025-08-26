using System.Collections;
using UnityEngine;

public class Door : Interactable
{
    public Animator animator;
    public float requierdHoldTime;
    public float doorCloseTime;
    public float doorNoiseLevel;


    private float currentHoldTime;
    public override void Interact()
    {
        if (gameObject.CompareTag("Door"))
        {
            currentHoldTime += Time.deltaTime;
            GameManager._instance.playerUI.openMeterSlider.maxValue = requierdHoldTime;
            GameManager._instance.playerUI.UpdateDoorUI(currentHoldTime);
        }

        else if (gameObject.CompareTag("LockedDoor"))
            if (GameManager._instance.hasKey)
            {
                gameObject.tag = "Door";
            }
    }

    private void Update()
    {
        if (GameManager._instance.player.GetComponent<InputManager>().InputMap.Player.Interact.WasReleasedThisFrame())
            HoldtimeCheck();

    }


    private void HoldtimeCheck()
    {
        if (currentHoldTime < requierdHoldTime)
        {
            Debug.Log("released before time");
            GameManager._instance.IncreaseLoudness(doorNoiseLevel);
        }
        else
        {
            Debug.Log("released after time");
        }
        currentHoldTime = 0;
        GameManager._instance.playerUI.UpdateDoorUI(currentHoldTime);
        animator.SetBool("isOpen", true);
        StartCoroutine(DoorCloseDelay());
    }

    private IEnumerator DoorCloseDelay()
    {
        yield return new WaitForSeconds(doorCloseTime);
        animator.SetBool("isOpen", false);

    }
}
