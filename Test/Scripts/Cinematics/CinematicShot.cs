using UnityEngine;
using Cinemachine;

namespace Skyfall.Cinematics
{
    public class CinematicShot : MonoBehaviour
    {
        [Header("Shot Settings")]
        [Tooltip("Name of this cinematic shot")]
        public string shotName = "New Shot";
        
        [Tooltip("Duration of this shot in seconds")]
        public float duration = 5f;
        
        [Tooltip("Priority when blending between shots")]
        [Range(0, 100)]
        public int priority = 10;
        
        [Header("Camera Settings")]
        [Tooltip("Field of view for this shot")]
        [Range(1f, 179f)]
        public float fieldOfView = 60f;
        
        [Tooltip("Optional target to look at")]
        public Transform lookAtTarget;
        
        [Tooltip("Optional target to follow")]
        public Transform followTarget;
        
        [Header("Movement")]
        [Tooltip("Enable camera movement during shot")]
        public bool enableMovement = false;
        
        [Tooltip("Movement speed")]
        public float movementSpeed = 1f;
        
        [Tooltip("Movement path")]
        public Transform[] movementPath;
        
        [Header("Post Processing")]
        [Tooltip("Enable depth of field")]
        public bool enableDepthOfField = false;
        
        [Tooltip("Focus distance for depth of field")]
        public float focusDistance = 10f;
        
        private CinemachineVirtualCamera virtualCamera;
        
        void Awake()
        {
            SetupVirtualCamera();
        }
        
        void SetupVirtualCamera()
        {
            virtualCamera = GetComponent<CinemachineVirtualCamera>();
            
            if (virtualCamera == null)
            {
                virtualCamera = gameObject.AddComponent<CinemachineVirtualCamera>();
            }
            
            virtualCamera.Priority = priority;
            virtualCamera.m_Lens.FieldOfView = fieldOfView;
            
            if (followTarget != null)
            {
                virtualCamera.Follow = followTarget;
            }
            
            if (lookAtTarget != null)
            {
                virtualCamera.LookAt = lookAtTarget;
            }
        }
        
        public void ActivateShot()
        {
            if (virtualCamera != null)
            {
                virtualCamera.Priority = 100;
            }
        }
        
        public void DeactivateShot()
        {
            if (virtualCamera != null)
            {
                virtualCamera.Priority = priority;
            }
        }
        
        public CinemachineVirtualCamera GetVirtualCamera()
        {
            if (virtualCamera == null)
            {
                SetupVirtualCamera();
            }
            return virtualCamera;
        }
        
        void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * 3f);
            
            if (lookAtTarget != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, lookAtTarget.position);
            }
            
            if (movementPath != null && movementPath.Length > 1)
            {
                Gizmos.color = Color.green;
                for (int i = 0; i < movementPath.Length - 1; i++)
                {
                    if (movementPath[i] != null && movementPath[i + 1] != null)
                    {
                        Gizmos.DrawLine(movementPath[i].position, movementPath[i + 1].position);
                    }
                }
            }
        }
    }
}
