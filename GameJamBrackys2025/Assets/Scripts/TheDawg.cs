using UnityEngine;
using UnityEngine.AI;

public class TheDawg : MonoBehaviour
{
    [SerializeField]
    private NavMeshAgent agent;

    public float dogSpeed;

    private GameObject player;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }
    private void Update()
    {
        if (agent != null)
        {
            agent.speed = dogSpeed;
            agent.SetDestination(player.transform.position);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            Debug.Log("walking on dawg");
        }
    }
}
