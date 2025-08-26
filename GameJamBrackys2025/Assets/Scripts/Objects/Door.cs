using System.Collections;
using UnityEngine;

public class Door : Interactable
{
    public Animator animator;
    public float requierdHoldTime;
    public float doorCloseTime;


    private float currentHoldTime;
    public override void Interact()
    {
        //while (GameManager._instance.player.GetComponent<InputManager>().InputMap.Player.Interact.IsPressed())
        
           currentHoldTime += Time.deltaTime;
        
        if (GameManager._instance.player.GetComponent<InputManager>().InputMap.Player.Interact.WasReleasedThisFrame())
        {
            Debug.Log("WORKED");
            HoldtimeCheck();

        }
        else
            Debug.Log("didnt WORKED");

    }

    private void HoldtimeCheck()
    {
        if (currentHoldTime < requierdHoldTime)
        {
            Debug.Log("released before time");
        }
        else
        {
            Debug.Log("released after time");
        }
        currentHoldTime = 0;
        animator.SetBool("isOpen", true);
        StartCoroutine(DoorCloseDelay());
    }

    private IEnumerator DoorCloseDelay()
    {
        yield return new WaitForSeconds(doorCloseTime);
        animator.SetBool("isOpen", false);

    }
}
