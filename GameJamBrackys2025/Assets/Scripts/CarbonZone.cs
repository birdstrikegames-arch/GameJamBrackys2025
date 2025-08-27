using UnityEngine;

public class CarbonZone : MonoBehaviour
{
    public float maxStayTime;
    public float killSpeed;
    private float currentStayTime;

    private bool inZone = false;
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inZone = true;
            currentStayTime += Time.deltaTime * killSpeed;

            if (currentStayTime >= maxStayTime)
            {
                GameManager._instance.hasLost = true;
            }
        }
    }


    private void Update()
    {
        if (!inZone && currentStayTime >= 0)
        {
            currentStayTime -= Time.deltaTime * killSpeed;
        }
        if (currentStayTime < 0)
            currentStayTime = 0;
    }
}
