using UnityEngine;

public class OutlineController : MonoBehaviour
{
    public Material outlineMaterialTemplate; // The actual shader material, Assign in Inspector
    private Renderer _renderer;
    private MaterialPropertyBlock _propBlock;

    [Tooltip("Index of the material to control the outline effect on, will be inserted in the last slot")]
    [SerializeField] private int materialIndex;

    private void Awake()
    {
        outlineMaterialTemplate = Resources.Load<Material>("Materials/OutlineShader"); //THAT IS THE DUMBEST SHIT I HAVE SEEN ALL WEEK AND I CODE DAFNY FFS WHY IS IT SO SPECIFIC
        _renderer = GetComponent<Renderer>();
        _propBlock = new MaterialPropertyBlock();

        var materials = _renderer.sharedMaterials;

        if (outlineMaterialTemplate == null) Debug.LogError("Outline Shader material not set or incorrect");
        else
        {
            // append the outline material to the renderer (so in the last slot)
            Material newMat = Instantiate(outlineMaterialTemplate);

            var newMaterials = new Material[materials.Length + 1];
            materials.CopyTo(newMaterials, materialIndex);
            newMaterials[materials.Length] = newMat;

            _renderer.sharedMaterials = newMaterials;
            materialIndex = newMaterials.Length - 1;
        }
    }

    public void SetOutline(bool enabled)
    {
        _renderer.GetPropertyBlock(_propBlock, materialIndex);
        _propBlock.SetFloat("_Enabled", enabled ? 1f : 0f);
        _renderer.SetPropertyBlock(_propBlock, materialIndex);
    }
}
