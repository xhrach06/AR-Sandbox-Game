using UnityEngine;

public class MousePosition : MonoBehaviour
{
    public Camera mainCamera; 

    void Start()
    {
       
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    void Update()
    {
        // Check if the left mouse button was clicked
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Mouse Click Detected");

      
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

           
            if (Physics.Raycast(ray, out hit))
            {
                
                Vector3 hitPosition = hit.point;
                Debug.Log("Mouse Position in World Space: " + hitPosition);
       
            }
            else
            {
                Debug.Log("Raycast did not hit anything");
            }
        }
    }
}
