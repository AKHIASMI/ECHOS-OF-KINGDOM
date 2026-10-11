using UnityEngine;

// Small helper: sets a renderer's colour and emission (glow). Works with URP Lit.
public static class MaterialGlow
{
    public static void Apply(Renderer r, Color color, float emissionIntensity)
    {
        if (r == null) return;
        Material m = r.material; // makes a private copy for this object
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
        if (m.HasProperty("_Color")) m.SetColor("_Color", color);
        if (m.HasProperty("_EmissionColor"))
        {
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", color * emissionIntensity);
        }
    }
}
