using UnityEngine;
using UnityEngine.XR.Hands;

public class SimpleHandVisualizer : MonoBehaviour
{
    [SerializeField] private Handedness handedness = Handedness.Left;
    [SerializeField] private GameObject jointPrefab;
    
    private XRHandSubsystem _handSubsystem;
    private GameObject[] _jointObjects;
    private static readonly XRHandJointID[] k_JointIds = new XRHandJointID[]
    {
        XRHandJointID.Wrist,
        XRHandJointID.Palm,
        XRHandJointID.ThumbMetacarpal,
        XRHandJointID.ThumbProximal,
        XRHandJointID.ThumbDistal,
        XRHandJointID.ThumbTip,
        XRHandJointID.IndexMetacarpal,
        XRHandJointID.IndexProximal,
        XRHandJointID.IndexIntermediate,
        XRHandJointID.IndexDistal,
        XRHandJointID.IndexTip,
        XRHandJointID.MiddleMetacarpal,
        XRHandJointID.MiddleProximal,
        XRHandJointID.MiddleIntermediate,
        XRHandJointID.MiddleDistal,
        XRHandJointID.MiddleTip,
        XRHandJointID.RingMetacarpal,
        XRHandJointID.RingProximal,
        XRHandJointID.RingIntermediate,
        XRHandJointID.RingDistal,
        XRHandJointID.RingTip,
        XRHandJointID.LittleMetacarpal,
        XRHandJointID.LittleProximal,
        XRHandJointID.LittleIntermediate,
        XRHandJointID.LittleDistal,
        XRHandJointID.LittleTip
    };

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var xrManager = UnityEngine.XR.Management.XRGeneralSettings.Instance?.Manager;
        if (xrManager != null && xrManager.activeLoader != null)
        {
            _handSubsystem = xrManager.activeLoader.GetLoadedSubsystem<XRHandSubsystem>();
        }

        // Create sphere for each joint
        if (jointPrefab == null)
        {
            jointPrefab = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            jointPrefab.transform.localScale = Vector3.one * 0.01f; // 1cm spheres
            
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            material.color = handedness == Handedness.Left ? new Color(0.3f, 0.8f, 1f) : new Color(1f, 0.5f, 0.3f);
            jointPrefab.GetComponent<Renderer>().material = material;
            
            Destroy(jointPrefab.GetComponent<Collider>()); // Remove collider
        }

        _jointObjects = new GameObject[k_JointIds.Length];

        for (int i = 0; i < _jointObjects.Length; i++)
        {
            _jointObjects[i] = Instantiate(jointPrefab, transform);
            _jointObjects[i].name = $"Joint_{k_JointIds[i]}";
            _jointObjects[i].SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_handSubsystem == null) return;

        var hand = handedness == Handedness.Left ? 
            _handSubsystem.leftHand : _handSubsystem.rightHand;

        if (!hand.isTracked)
        {
            foreach (var joint in _jointObjects)
            {
                if (joint != null) joint.SetActive(false);
            }
            return;
        }

        // Update each joint position safely using explicit joint IDs
        for (int i = 0; i < k_JointIds.Length; i++)
        {
            var jointGO = _jointObjects[i];
            if (jointGO == null) continue;

            var id = k_JointIds[i];
            var joint = hand.GetJoint(id);

            if (joint.TryGetPose(out var pose))
            {
                jointGO.SetActive(true);
                jointGO.transform.position = pose.position;
                jointGO.transform.rotation = pose.rotation;
            }
            else
            {
                jointGO.SetActive(false);
            }
        }
    }
}
