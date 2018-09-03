using UnityEngine;

namespace Dorofiy.Textures
{
  public class TextureCombiner
  {
    public class Internal
    {
      public static Texture2D Create(TextureCombinerConfiguration configuration, bool preview)
      {
        var unlitColor = Shader.Find("Unlit/Color");
        var unlitMaterial = new Material(unlitColor);
        unlitMaterial.color = configuration.DefaultColor;

        var shader = Shader.Find("Utils/TextureCombiner");
        var material = new Material(shader);

        material.SetInt("_Preview", preview ? 1 : 0);

        var renderTexture = RenderTexture.GetTemporary(configuration.Width, configuration.Height);
        renderTexture.antiAliasing = 16;
        
        Graphics.Blit(Texture2D.whiteTexture, renderTexture, unlitMaterial);

        material.SetVector("_Invert", new Vector4(configuration.R.Invert, configuration.G.Invert, configuration.B.Invert, configuration.A.Invert));

        if (configuration.R.Texture) material.SetTexture("_R", configuration.R.Texture);
        else material.SetTexture("_R", renderTexture);
        material.SetVector("_BlendR", GetVectorByChannel(configuration.R.Channel));

        if (configuration.G.Texture) material.SetTexture("_G", configuration.G.Texture);
        else material.SetTexture("_G", renderTexture);
        material.SetVector("_BlendG", GetVectorByChannel(configuration.G.Channel));

        if (configuration.B.Texture) material.SetTexture("_B", configuration.B.Texture);
        else material.SetTexture("_B", renderTexture);
        material.SetVector("_BlendB", GetVectorByChannel(configuration.B.Channel));

        if (configuration.A.Texture) material.SetTexture("_A", configuration.A.Texture);
        else material.SetTexture("_A", renderTexture);
        material.SetVector("_BlendA", GetVectorByChannel(configuration.A.Channel));

        Graphics.Blit(renderTexture, renderTexture, material);

        RenderTexture.active = renderTexture;

        var texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false)
        {
          filterMode = FilterMode.Bilinear
        };
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture.Apply();

        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(renderTexture);

        return texture;
      }
    }

    public static Texture2D Create(TextureCombinerConfiguration configuration)
    {
      return Internal.Create(configuration, false);
    }

    private static Vector4 GetVectorByChannel(TextureChannel channel)
    {
      switch (channel)
      {
        case TextureChannel.R: return new Vector4(1, 0, 0, 0);
        case TextureChannel.G: return new Vector4(0, 1, 0, 0);
        case TextureChannel.B: return new Vector4(0, 0, 1, 0);
        case TextureChannel.A: return new Vector4(0, 0, 0, 1);
      }
      return Vector4.zero;
    }
  }
}
