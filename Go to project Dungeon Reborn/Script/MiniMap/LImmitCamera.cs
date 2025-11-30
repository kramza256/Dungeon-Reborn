using UnityEngine;

public class LImmitCamera : MonoBehaviour
{
    public GameObject Player;

    private void LateUpdate()
    {
        transform.position = new Vector3(Player.transform.position.x, 30 , Player.transform.position.z);
    }
}
