using System;
using UnityEngine;

namespace Dorofiy.Textures
{
  public class TextureCombinerConfiguration
  {
    private readonly TextureInfo[] _map = new TextureInfo[4];
    private int _width;
    private int _height;
    private Color _defaultColor = Color.black;

    public TextureCombinerConfiguration(int width, int height)
    {
      Width = width;
      Height = height;
    }

    public Color DefaultColor
    {
      get { return _defaultColor; }
      set { _defaultColor = value; }
    }

    public int Width
    {
      get { return _width; }
      set
      {
        if (value <= 0) throw new ArgumentException("Width <= 0");
        _width = value;
      }
    }

    public int Height
    {
      get { return _height; }
      set
      {
        if (value <= 0) throw new ArgumentException("Height <= 0");
        _height = value;
      }
    }

    public TextureInfo R
    {
      get { return _map[0]; }
      set { _map[0] = value; }
    }
    public TextureInfo G
    {
      get { return _map[1]; }
      set { _map[1] = value; }
    }
    public TextureInfo B
    {
      get { return _map[2]; }
      set { _map[2] = value; }
    }
    public TextureInfo A
    {
      get { return _map[3]; }
      set { _map[3] = value; }
    }

    public TextureCombinerConfiguration Set(Texture2D texture, TextureChannel textureChannel,
      TextureChannel outputChannel, float invert)
    {
      var index = (int)outputChannel;
      _map[index] = new TextureInfo
      {
        Channel = textureChannel,
        Texture = texture,
        Invert = invert
      };
      return this;
    }
  }
}
