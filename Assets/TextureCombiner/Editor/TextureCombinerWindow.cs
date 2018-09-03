using System;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Dorofiy.Textures
{
  [Serializable]
  public class TextureCombinerWindow : EditorWindow
  {
    [MenuItem("Assets/Texture/Combine")]
    public static void Open()
    {
      GetWindow<TextureCombinerWindow>("Combine").Show(true);
    }

    private enum PreviewChannel
    {
      RGBA,
      RGB,
      R,
      G,
      B,
      A
    }

    [Serializable]
    private struct Info
    {
      public Texture2D Texture;
      public TextureChannel Channel;
      public Texture2D Preview;
      public float Invert;
      public bool Enabled;
    }

    [SerializeField]
    private Info[] _infos = new Info[4];

    private ReorderableList _reorderableList;
    private Vector2Int _previwsSize = new Vector2Int(128, 128);
    private Vector2Int _previwSize = new Vector2Int(512, 512);
    private Vector2 _scroll;
    private Color[] _colors = new Color[4] { Color.red, Color.green, Color.blue, Color.black };
    private Texture2D _preview;
    private Vector2Int _resultSize = new Vector2Int(256, 256);
    private SerializedObject _serializedObject;
    private string[] _formats = { "PNG", "JPG", "EXR" };
    private int _previewChannel;
    private int _textureFormat;
    private bool _powOfTwo = true;

    void OnEnable()
    {
      _serializedObject = new SerializedObject(this);
      minSize = new Vector2(870, 600);
      maxSize = minSize;
      for (int i = 0; i < _infos.Length; i++)
      {
        _infos[i].Enabled = true;
      }
      _infos[3] = new Info
      {
        Channel = TextureChannel.R,
        Texture = Texture2D.whiteTexture
      };
      _reorderableList = new ReorderableList(_serializedObject, _serializedObject.FindProperty("_infos"), true, false, false, false);
      _reorderableList.elementHeight = _previwsSize.y;
      _reorderableList.onReorderCallback += list => UpdatePreview();
      _reorderableList.drawElementCallback += (rect, index, active, focused) =>
      {
        var i = index;
        GUI.color = _colors[i];

        GUI.Box(rect, GUIContent.none);

        GUI.color = Color.white;
        EditorGUI.BeginChangeCheck();

        var contentRect = rect;

        GUI.Label(new Rect(rect.x, rect.y, 16f, _previwsSize.y), ((TextureChannel)i).ToString(), new GUIStyle()
        {
          normal = new GUIStyleState
          {
            textColor = Color.white
          },
          alignment = TextAnchor.MiddleCenter
        });
        rect.xMin += 16;

        var info = _infos[i];

        info.Enabled = EditorGUI.ToggleLeft(new Rect(contentRect.x, contentRect.y, 16, 16), "", info.Enabled);

        if (info.Enabled)
        {
          info.Texture = EditorGUI.ObjectField(new Rect(rect.x, rect.y, _previwsSize.x, _previwsSize.y), info.Texture,
            typeof(Texture2D), false) as Texture2D;
          rect.xMin += _previwsSize.x;

          GUI.Label(new Rect(rect.x, rect.y, 50, 16), "Channel");
          info.Channel =
            (TextureChannel)EditorGUI.EnumPopup(new Rect(rect.x, rect.y + 16, 50, 16), (Enum)info.Channel);
          GUI.Label(new Rect(rect.x, rect.y + 60, 50, 16), "Invert");
          info.Invert = EditorGUI.Slider(new Rect(rect.x, rect.y + 76, 50, 16), info.Invert, 0, 1);
          //info.Invert = EditorGUI.Toggle(new Rect(rect.x, rect.y + 76, 50, 16), info.Invert);
          rect.xMin += 55;

          if (EditorGUI.EndChangeCheck())
          {
            UpdateChannelPreview(info, i);
          }

          if (_infos[i].Preview != null)
          {
            EditorGUI.DrawPreviewTexture(new Rect(rect.xMin, rect.y, _previwsSize.x, _previwsSize.y),
              _infos[i].Preview);
          }
        }
        else if (EditorGUI.EndChangeCheck())
        {
          _infos[i] = info;
        }
      };
    }

    void OnGUI()
    {
      EditorGUILayout.BeginHorizontal();

      EditorGUILayout.BeginVertical(GUILayout.Width(350));

      EditorGUI.BeginChangeCheck();

      _serializedObject.Update();
      _reorderableList.DoLayoutList();

      EditorGUILayout.EndVertical();
      EditorGUILayout.BeginVertical();

      _previewChannel = GUILayout.SelectionGrid(_previewChannel, Enum.GetNames(typeof(PreviewChannel)), 6);

      if (EditorGUI.EndChangeCheck())
      {
        UpdatePreview();
      }

      var previewRect = EditorGUILayout.GetControlRect(false, _previwsSize.y, GUILayout.Width(_previwSize.x), GUILayout.Height(_previwSize.y));
      if (_preview != null)
      {
        EditorGUI.DrawTextureTransparent(previewRect, _preview);
      }
      EditorGUILayout.EndVertical();
      EditorGUILayout.EndHorizontal();

      EditorGUILayout.BeginHorizontal("Box");
      _powOfTwo = EditorGUILayout.ToggleLeft("PowOf2", _powOfTwo);
      _resultSize.x = CorrectSize(EditorGUILayout.IntSlider("Width", _resultSize.x, 16, 16384), _powOfTwo);
      _resultSize.y = CorrectSize(EditorGUILayout.IntSlider("Height", _resultSize.y, 16, 16384), _powOfTwo);
      EditorGUILayout.EndHorizontal();

      EditorGUILayout.BeginHorizontal("Box");
      _textureFormat = GUILayout.SelectionGrid(_textureFormat, _formats, 3, EditorStyles.miniButton);
      var lastColor = GUI.color;
      GUI.color = Color.green;
      if (GUILayout.Button("SAVE", EditorStyles.miniButton))
      {
        Save();
      }

      GUI.color = lastColor;
      EditorGUILayout.EndHorizontal();
      _serializedObject.ApplyModifiedProperties();
    }

    void Save()
    {
      string savePath = EditorUtility.SaveFilePanel("Save", Application.dataPath, "texture.png", _formats[_textureFormat].ToLowerInvariant());
      if (savePath != string.Empty)
      {
        var config = new TextureCombinerConfiguration(_resultSize.x, _resultSize.y);
        for (int i = 0; i < _infos.Length; i++)
        {
          var info = _infos[i];
          if (info.Enabled && info.Texture != null)
          {
            config.Set(info.Texture, info.Channel, (TextureChannel)i, info.Invert);
          }
        }

        if (!_infos[(int)TextureChannel.A].Enabled)
        {
          config.Set(Texture2D.whiteTexture, TextureChannel.R, TextureChannel.A, 0);
        }

        Texture2D output = TextureCombiner.Create(config);
        if (_textureFormat == 0)
          File.WriteAllBytes(savePath, output.EncodeToJPG());
        else if (_textureFormat == 1)
          File.WriteAllBytes(savePath, output.EncodeToPNG());
        else
          File.WriteAllBytes(savePath, output.EncodeToEXR());

        AssetDatabase.Refresh();
      }
    }

    void UpdateChannelPreview(Info info, int i)
    {
      _infos[i] = info;
      var texture = info.Texture;
      var channel = info.Channel;
      var isInvert = info.Invert;
      if (texture != null)
      {
        var conf = new TextureCombinerConfiguration(_previwsSize.x, _previwsSize.y);
        conf.Set(texture, channel, TextureChannel.R, isInvert);
        conf.Set(texture, channel, TextureChannel.G, isInvert);
        conf.Set(texture, channel, TextureChannel.B, isInvert);
        conf.Set(texture, channel, TextureChannel.A, isInvert);

        var previewTexture = TextureCombiner.Create(conf);
        info.Preview = previewTexture;
        _infos[i] = info;
      }
    }

    void UpdatePreview()
    {
      if (!(_previewChannel == (int)PreviewChannel.RGB || _previewChannel == (int)PreviewChannel.RGBA))
      {
        var config = new TextureCombinerConfiguration(_previwsSize.x, _previwsSize.y);
        var info = _infos[_previewChannel - 2];
        if (info.Enabled)
        {
          config.Set(info.Texture, info.Channel, info.Channel, info.Invert);
        }
        _preview = TextureCombiner.Internal.Create(config, true);
      }
      else
      {
        var config = new TextureCombinerConfiguration(_previwsSize.x, _previwsSize.y);
        var count = _infos.Length - _previewChannel;
        for (int i = 0; i < count; i++)
        {
          var info = _infos[i];
          if (info.Enabled && info.Texture != null)
          {
            config.Set(info.Texture, info.Channel, (TextureChannel)i, info.Invert);
          }
        }

        if (_previewChannel == (int)PreviewChannel.RGB || !_infos[(int)TextureChannel.A].Enabled)
        {
          config.A = new TextureInfo
          {
            Channel = TextureChannel.R,
            Texture = Texture2D.whiteTexture
          };
        }
        _preview = TextureCombiner.Create(config);
      }
    }

    int CorrectSize(int value, bool powOf2)
    {
      if (powOf2)
      {
        return UpperPowerOfTwo(value);
      }

      return value;
    }

    int UpperPowerOfTwo(int v)
    {
      v--;
      v |= v >> 1;
      v |= v >> 2;
      v |= v >> 4;
      v |= v >> 8;
      v |= v >> 16;
      v++;
      return v;
    }
  }
}

