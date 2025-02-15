using UnityEngine;

public class MeteorManager : MonoBehaviour
{
    public GameObject meteorPrefab; // Meteor prefab
    public float cooldown = 5f; // Time between meteor launches
    private float lastMeteorTime;

    void Update()
    {
        if (Input.GetMouseButtonDown(1) && Time.time - lastMeteorTime > cooldown) // Right-click to launch meteor
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                LaunchMeteor(hit.point);
                lastMeteorTime = Time.time;
            }
        }
    }

    void LaunchMeteor(Vector3 position)
    {
        Instantiate(meteorPrefab, position + Vector3.up * 10f, Quaternion.identity); // Spawn meteor above the target
    }
}
