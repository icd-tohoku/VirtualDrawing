using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class DrawController : MonoBehaviour
{
    [SerializeField] public LineList _lineList;
    [SerializeField] private string _name;
    [SerializeField] private int _dotSize = 5;
    [SerializeField] private float _lerpAjust = 1.5f;

    private GameObject _inputSurface;

    private Texture2D _instanceTexture;
    private Color[] _buffer;
    private Color[] _clearBuffer;
    private int _textureSize;

    //------------------------Monobehavior---------------------------------------

    private void Awake()
    {
        //描画用surfaceの準備
        _inputSurface = gameObject;
        Texture2D mainTexture = (Texture2D)_inputSurface.GetComponent<Renderer>().material.mainTexture;
        _textureSize = mainTexture.width;
        Color[] pixels = mainTexture.GetPixels();

        _buffer = new Color[pixels.Length];
        pixels.CopyTo(_buffer, 0);

        _instanceTexture = new Texture2D(mainTexture.width, mainTexture.height, TextureFormat.RGBA32, false);
        _instanceTexture.filterMode = FilterMode.Point;


        //Clear用のバッファ
        _clearBuffer = new Color[pixels.Length];
        pixels.CopyTo(_clearBuffer, 0);


        _lineList.action += () =>
        {
            Draw(_lineList.AddStart, _lineList.AddEnd);
        };
        _lineList.DotStart += () =>
          {
              Dot(_lineList.StartDrawingPoint, _lineList.DotSize, Color.red);
              _instanceTexture.SetPixels(_buffer);
              _instanceTexture.Apply();
              _inputSurface.GetComponent<Renderer>().material.mainTexture = _instanceTexture;
          };
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            Clear();
            foreach(var linePara in _lineList.Lines)
            {
                Draw(linePara.StartPoint(), linePara.EndPoint());
            }
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            foreach(var linePara in _lineList.Lines)
            {
                Draw(linePara.StartPoint(), linePara.EndPoint());
            }
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            SaveData();
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            Clear();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            Dot(_lineList.StartDrawingPoint, _lineList.DotSize, Color.red);
            _instanceTexture.SetPixels(_buffer);
            _instanceTexture.Apply();
            _inputSurface.GetComponent<Renderer>().material.mainTexture = _instanceTexture;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit_info = new RaycastHit();
            float max_distance = 100f;

            bool is_hit = Physics.Raycast(ray, out hit_info, max_distance);

            if (is_hit)
            {
                //TODO: ヒットした時の処理;
                Vector2 point =new Vector2 (hit_info.point.x, hit_info.point.y);
                Vector2 texturePoint = new Vector2(map(point.x, 5, -5, 0, 1023), map(point.y, -4, 6, 0, 1023));
                _lineList.AddStart = texturePoint;
            }
        }
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit_info = new RaycastHit();
            float max_distance = 100f;

            bool is_hit = Physics.Raycast(ray, out hit_info, max_distance);

            if (is_hit)
            {
                //TODO: ヒットした時の処理;
                Vector2 point = new Vector2(hit_info.point.x, hit_info.point.y);
                Vector2 texturePoint = new Vector2(map(point.x, 5, -5, 0, 1023), map(point.y, -4, 6, 0, 1023));
                _lineList.AddEnd = texturePoint;
            }
        }
    }

    //---------------------------private関数------------------------------------
    //----------------------------データ関連-----------------------------------

    private void SaveData()
    {
        var fileName = $@"{Application.dataPath}/Scripts/DrawingModel/Model/{_name}_{DateTime.Now: yyyyMMddhhmmss}";
        SaveTexture($@"{fileName}.png", _instanceTexture);
        CSVManager.CreateFile($@"{fileName}.csv");
        string data = "";
        data += $"StartPoint,{_lineList.StartDrawingPoint.x},{_lineList.StartDrawingPoint.y}\n";
        foreach (var linePara in _lineList.Lines)
        {
            data += $"{linePara.ToString()}" + "\n";
        }
        CSVManager.Write($@"{fileName}.csv", data, FileMode.Append);

        AssetDatabase.Refresh();
    }

    private void SaveTexture(string filePath, Texture2D texture)
    {
        // バイト配列に変換する
        var bytes = texture.EncodeToPNG();

        // パスは拡張子付きであること
        if (!filePath.EndsWith(".png")) filePath += ".png";

        //画像ファイルを保存する
        File.WriteAllBytes(filePath, bytes);

        // 最後にRefresh
        AssetDatabase.Refresh();
    }

    private void PutPng(string pngFilePath, Texture2D texture)
    {
        byte[] readBinary = ReadPngFile(pngFilePath);

        //大本のテクスチャを書き換えないためにテクスチャをインスタンス化
        Texture2D instanceTexture = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
        this._instanceTexture.filterMode = FilterMode.Point;

        instanceTexture.LoadImage(readBinary);

        //テクスチャを反映させる
        //_modelSurface.GetComponent<Renderer>().material.mainTexture = instanceTexture;


    }

    private byte[] ReadPngFile(string path)
    {
        FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
        BinaryReader bin = new BinaryReader(fileStream);
        byte[] values = bin.ReadBytes((int)bin.BaseStream.Length);

        bin.Close();

        return values;
    }

    private void CalculateError()
    {

    }



    //---------------------------描画関連------------------------------------

    private void Draw(Vector2 start, Vector2 end)
    {
      　// ここの数字要調節
        float lerpCountAdjustNum = _dotSize * _lerpAjust; // ここの数字要調節 大きいほど荒くなる

        float lineLength = Vector2.Distance(start, end);
        int lerpCount = Mathf.CeilToInt(lineLength / lerpCountAdjustNum);
        //int maxlerpCount = 100;
        //lerpCount = Mathf.Min(lerpCount, maxlerpCount);

        for (int i = 0; i <= lerpCount; i++)
        {
            //t に入れる割合値を "現在の回数/合計回数" で出す
            var lerpWeight = (float)i / lerpCount;

            //始点、終点、割合を渡して補完する座標を算出
            var lerpPosition = Vector2.Lerp(start, end, lerpWeight);

            //lerpSizeの大きさの点をlerpPositionに描画
            Dot(lerpPosition, _dotSize, Color.black);
        }

        //テクスチャを反映させる
        _instanceTexture.SetPixels(_buffer);
        _instanceTexture.Apply();
        _inputSurface.GetComponent<Renderer>().material.mainTexture = _instanceTexture;
        
        //ここらへんでpとその点が付いた時間(float)？をベクトルに保存
        //new Vector3(p.x, p.y, Time.time)
    }

    /// <summary>
    /// 指定されたピクセルにところに点を打つ
    /// </summary>
    private void Dot(Vector2 p, int dotSize, Color color)
    {
        //円形のドットを打つアルゴリズム
        /*for (int i = Mathf.Max(-dotSize,-(int)p.x); i <= Mathf.Min(dotSize , _textureSize - (int)p.x) ; i++)
        {
            for (int j =Mathf.Max( -(dotSize - Mathf.Abs(i)) , -(int)p.y); j <= Mathf.Min((dotSize - Mathf.Abs(i)),_textureSize - (int)p.y); ++j)
            {
                _buffer.SetValue(Color.black, ((int)p.x + i) + _textureSize * ((int)p.y + j));
            }
        }*/

        for (int i = Mathf.Max(-dotSize, -(int)p.x); i <= Mathf.Min(dotSize, _textureSize - (int)p.x); i++)
        {
            for (int j = Mathf.Max(-dotSize, -(int)p.y); j <= Mathf.Min(dotSize, _textureSize - (int)p.y); ++j)
            {
                if (_buffer.GetValue(((int)p.x + i) + _textureSize * ((int)p.y + j)).Equals(Color.white))
                {
                    _buffer.SetValue(color, ((int)p.x + i) + _textureSize * ((int)p.y + j));
                }
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
        _inputSurface.GetComponent<Renderer>().material.mainTexture = _instanceTexture;
    }

    private float map(float x, float in_min, float in_max, float out_min, float out_max)
    {
        return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
    }
}
