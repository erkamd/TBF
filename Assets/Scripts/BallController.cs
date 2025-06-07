using UnityEngine;

public class BallController : MonoBehaviour
{
    public Vector3 velocity;
    public AgentController possessor;

    public bool IsPossessed => possessor != null;

    public void MoveBall()
    {
        if (IsPossessed)
        {
            transform.position = possessor.transform.position;
        }
        else
        {
            transform.position += velocity;
        }
    }
}
