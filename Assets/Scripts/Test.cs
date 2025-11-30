using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class Test : MonoBehaviour
{
    // Simple runtime self-test to verify the project runs after pulling from GitHub.

    // Visuals
    private GameObject _cube;
    private Material _mat;

    // Test state
    private int _frames;
    private bool _coroutineTicked;
    private string _status = "Running self-test...";
    private string _details = "";
    private Color _uiColor = Color.yellow;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Application.targetFrameRate = 60;

        var pipeline = GraphicsSettings.currentRenderPipeline;
        Debug.Log($"[SelfTest] Product='{Application.productName}' Version='{Application.version}' Platform='{Application.platform}' Unity='{Application.unityVersion}' Pipeline='{(pipeline ? pipeline.name : "Built-in")}'");

        StartCoroutine(RunSelfTest());
    }

    // Update is called once per frame
    void Update()
    {
        _frames++;

        if (_cube != null)
        {
            _cube.transform.Rotate(0f, 60f * Time.deltaTime, 0f, Space.World);

            if (_mat != null)
            {
                // Cycle color to confirm shader/material updates
                float h = Mathf.PingPong(Time.time * 0.25f, 1f);
                var c = Color.HSVToRGB(h, 0.8f, 1f);
                SetMaterialColor(_mat, c);
            }
        }
    }

    private IEnumerator RunSelfTest()
    {
        // 1) Create a visible object in front of the user
        _cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _cube.name = "SelfTest_Cube";

        // Position cube in front of the main camera
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            // Spawn 2 meters in front of camera
            _cube.transform.position = mainCam.transform.position + mainCam.transform.forward * 2f;
        }
        else
        {
            // Fallback if no main camera
            Debug.LogWarning("[SelfTest] No main camera found, using default position.");
            _cube.transform.position = new Vector3(0f, 1f, 3f);
        }

        // Pick a shader that exists in both URP and Built-in, with fallbacks
        Shader shader =
            Shader.Find("Universal Render Pipeline/Lit") ??
            Shader.Find("Standard") ??
            Shader.Find("Unlit/Color");

        if (shader == null)
        {
            _status = "FAIL";
            _uiColor = Color.red;
            _details = "No suitable shader found.";
            Debug.LogError("[SelfTest] No suitable shader found.");
            yield break;
        }

        _mat = new Material(shader) { name = "SelfTest_Material" };
        SetMaterialColor(_mat, new Color(0.1f, 0.8f, 1f));
        var renderer = _cube.GetComponent<Renderer>();
        if (renderer != null) renderer.sharedMaterial = _mat;

        // 2) Verify Update is ticking
        int startFrame = _frames;
        yield return new WaitForSeconds(0.25f);
        bool updateOk = _frames > startFrame;

        // 3) Verify coroutines work
        yield return StartCoroutine(TickOnce());
        bool coroutineOk = _coroutineTicked;

        // 4) Aggregate result
        bool visualsOk = _cube != null && _mat != null;
        bool allOk = updateOk && coroutineOk && visualsOk;

        _status = allOk ? "PASS" : "FAIL";
        _uiColor = allOk ? Color.green : Color.red;
        _details =
            $"Update={(updateOk ? "OK" : "FAIL")}, Coroutine={(coroutineOk ? "OK" : "FAIL")}, Visual={(visualsOk ? "OK" : "FAIL")}";

        if (allOk)
            Debug.Log($"[SelfTest] PASS - {_details}");
        else
            Debug.LogError($"[SelfTest] FAIL - {_details}");
    }

    private IEnumerator TickOnce()
    {
        yield return null; // next frame
        _coroutineTicked = true;
    }

    private void SetMaterialColor(Material m, Color c)
    {
        // Support both URP Lit and Standard/unlit color properties
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        if (m.HasProperty("_Color")) m.SetColor("_Color", c);
        if (m.HasProperty("_EmissionColor")) m.SetColor("_EmissionColor", c * 0.25f);
    }

    private void OnGUI()
    {
        var style = new GUIStyle(GUI.skin.box)
        {
            fontSize = 16,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.black }
        };

        Color prev = GUI.color;
        GUI.color = _uiColor;
        GUILayout.BeginArea(new Rect(12, 12, 460, 80));
        GUILayout.Box($"Self-Test: {_status}\n{_details}", style, GUILayout.Height(60));
        GUILayout.EndArea();
        GUI.color = prev;
    }

    private void OnDestroy()
    {
        if (_cube != null) Destroy(_cube);
        if (_mat != null) Destroy(_mat);
    }
}
