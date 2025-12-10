using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Management;

public class HandTrackingSetup : MonoBehaviour
{
    [Header("Hand Visualizers")]
    [SerializeField] private GameObject leftHandVisualizer;
    [SerializeField] private GameObject rightHandVisualizer;

    private XRHandSubsystem _handSubsystem;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Get the hand tracking subsystem
        var xrManager = XRGeneralSettings.Instance?.Manager;
        if (xrManager != null && xrManager.activeLoader != null)
        {
            _handSubsystem = xrManager.activeLoader.GetLoadedSubsystem<XRHandSubsystem>();
            
            if (_handSubsystem != null)
            {
                Debug.Log("[HandTracking] Hand subsystem initialized successfully");
            }
            else
            {
                Debug.LogWarning("[HandTracking] Hand subsystem not available");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_handSubsystem == null) return;

        // Update hand tracking status
        UpdateHandVisibility(Handedness.Left, leftHandVisualizer);
        UpdateHandVisibility(Handedness.Right, rightHandVisualizer);
    }

    private void UpdateHandVisibility(Handedness handedness, GameObject visualizer)
    {
        if (visualizer == null) return;

        var hand = handedness == Handedness.Left ? 
            _handSubsystem.leftHand : _handSubsystem.rightHand;

        // Show/hide based on tracking state
        bool isTracked = hand.isTracked;
        visualizer.SetActive(isTracked);

        if (isTracked)
        {
            // Update root position (wrist)
            if (hand.GetJoint(XRHandJointID.Wrist).TryGetPose(out var wristPose))
            {
                visualizer.transform.position = wristPose.position;
                visualizer.transform.rotation = wristPose.rotation;
            }
        }
    }

    void OnDestroy()
    {
        _handSubsystem = null;
    }
}
