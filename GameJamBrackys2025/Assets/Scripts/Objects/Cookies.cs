using UnityEngine;

public class Cookies : Interactable
{
    public override void Interact()
    {
        GameManager._instance.hasCollectedCookies = true;
    }
}
