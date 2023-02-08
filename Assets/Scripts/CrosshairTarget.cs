using Photon.Pun;
using UnityEngine;

// We have this class because when we shoot a ray in singleshotgun script bullet direction was wrong
// So we have to get a crosshair target to direct the ray into it
public class CrosshairTarget : MonoBehaviour
{
    [SerializeField] Camera _camera;
    Ray ray;
    RaycastHit hit;

    PhotonView pv;

    [SerializeField] Transform playerTransform;

    void Start()
    {
        pv = playerTransform.GetComponent<PhotonView>();
    }

    void Update()
    {
        if(pv.IsMine)
        {
            ray.origin = _camera.transform.position;
            ray.direction = _camera.transform.forward;
            if (Physics.Raycast(ray, out hit)) 
            {
                transform.position = hit.point;
            }
            else
            {
                transform.position = ray.origin + ray.direction * 1000.0f;
            }
        }
    }
}
