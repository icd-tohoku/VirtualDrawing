using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SimpleDrawing : RayInputSystem
{
    private Texture2D _instanceTexture;
    private Color[] _buffer;
    private Color[] _clearBuffer;
    private int _textureSize;
    private Vector2 _lastTouchVec = new Vector2();
    private bool _isInteractionCalled = false;
    private float _timer = 0;

    private void Awake()
    {
        //描画用surfaceの準備
        Texture2D mainTexture = (Texture2D)gameObject.GetComponent<Renderer>().material.mainTexture;
        _textureSize = mainTexture.width;
        Color[] pixels = mainTexture.GetPixels();

        _buffer = new Color[pixels.Length];
        pixels.CopyTo(_buffer, 0);

        _instanceTexture = new Texture2D(mainTexture.width, mainTexture.height, TextureFormat.RGBA32, false);
        _instanceTexture.filterMode = FilterMode.Point;

        var data = _instanceTexture.GetRawTextureData<Color32>();
        for (int i = 0; i < _textureSize * _textureSize; i++)
        {
            data[i] = Color.white;
        }

        //Clear用のバッファ
        _clearBuffer = new Color[pixels.Length];
        pixels.CopyTo(_clearBuffer, 0);
    }

    private void Update()
    {
        if (_isInteractionCalled)
        {
            _isInteractionCalled = false;
            _timer = 0;
        }
        else
        {
            _timer += Time.deltaTime;
            if (_timer > 0.05f)
            {
                NoTouch();
                _timer = 0;
            }
        }
    }

    public override void NoTouch()
    {
        _lastTouchVec = Vector2.zero;
    }

    public override void RayInteraction(RaycastHit hit)
    {
        if(hit.collider.gameObject == gameObject)
        {
            _isInteractionCalled = true;
            Draw(hit.textureCoord * _textureSize);
        }      
    }

    private void Draw(Vector2 p)
    {
        int dotSize = 1;　// ここの数字要調節
        float lerpCountAdjustNum = dotSize * 1f; // ここの数字要調節 大きいほど荒くなる

        if (_lastTouchVec.magnitude == 0)
        {
            _lastTouchVec = p;
        }

        float lineLength = Vector2.Distance(_lastTouchVec, p);
        int lerpCount = Mathf.CeilToInt(lineLength / lerpCountAdjustNum);
        int maxlerpCount = 20;
        lerpCount = Mathf.Min(lerpCount, maxlerpCount);

        for (int i = 0; i <= lerpCount; i++)
        {
            //t に入れる割合値を "現在の回数/合計回数" で出す
            var lerpWeight = (float)i / lerpCount;

            //始点、終点、割合を渡して補完する座標を算出
            var lerpPosition = Vector2.Lerp(_lastTouchVec, p, lerpWeight);

            //lerpSizeの大きさの点をlerpPositionに描画
            Dot(lerpPosition, dotSize);
        }

        //更新
        _lastTouchVec = p;

        //テクスチャを反映させる
        _instanceTexture.Apply();
        gameObject.GetComponent<Renderer>().material.mainTexture = _instanceTexture;

    }

    /// <summary>
    /// 指定されたピクセルにところに点を打つ
    /// </summary>
    private void Dot(Vector2 p, int dotSize, object color = null)
    {
        if (color == null) color = Color.black;
        var data = _instanceTexture.GetRawTextureData<Color32>();
        //四角のドットを打つアルゴリズム
        for (int i = Mathf.Max(-dotSize, -(int)p.x); i <= Mathf.Min(dotSize, _textureSize - (int)p.x); i++)
        {
            for (int j = Mathf.Max(-dotSize, -(int)p.y); j <= Mathf.Min(dotSize, _textureSize - (int)p.y); ++j)
            {
                //SetPixelsより軽い処理
                data[((int)p.x + i) + _textureSize * ((int)p.y + j)] = Color.black;
            }
        }
    }

    private void Clear()
    {
        for (int x = 0; x < _textureSize; x++)
        {
            for (int y = 0; y < _textureSize; y++)
            {
                _buffer[x + _textureSize * y] = _clearBuffer[x + _textureSize * y];
            }
        }
        _instanceTexture.SetPixels(_buffer);
        _instanceTexture.Apply();
        gameObject.GetComponent<Renderer>().material.mainTexture = _instanceTexture;
    }

    private void SafeCreateDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            return;
        }
        Directory.CreateDirectory(path);
        return;
    }
}
