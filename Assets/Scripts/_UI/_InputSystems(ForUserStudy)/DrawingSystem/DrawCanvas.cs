using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class DrawCanvas : MonoBehaviour
{
    private GameObject _modelSurface;
    private GameObject _inputSurface;

    private bool _isDoingTask = false;
    private bool _isDrawing = false;

    private bool _isFinished = false;
    public bool IsFinished
    {
        get
        {
            return _isFinished;
        }
    }

    private float _completeTimer = 0f;
    public float CompleteTime
    {
        get
        {
            return _completeTimer;
        }
    }

    private float _drawingTimer = 0f;
    public float DrawingTime
    {
        get
        {
            return _drawingTimer;
        }
    }

    private float _error = 0f;
    public float Error
    {
        get
        {
            return _error;
        }
    }

    private string _modelName;
    public string ModelName
    {
        get
        {
            return _modelName;
        }
    }

    //平均誤差を求める
    private List<Vector3> _dotsList = new List<Vector3>();
    public List<Vector3> DotList
    {
        get
        {
            return _dotsList;
        }
    }

    private Texture2D _instanceTexture;
    private Color[] _buffer;
    private Color[] _clearBuffer;
    private int _textureSize;
    private Vector2 _lastTouchVec = new Vector2();

    private float _height;
    public float Height
    {
        set
        {
            _height = value;
            transform.position = new Vector3(transform.position.x, _height, transform.position.z);
        }
        get
        {
            return _height;
        }
    }

    private float _angle;
    public float Angle
    {
        set
        {
            _angle = value;
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.eulerAngles.y, _angle);
        }
        get
        {
            return _angle;
        }
    }

    private float _timeToStart;
    public float TimeToStart
    {
        get
        {
            return _timeToStart;
        }
    }

    //------------------------Monobehavior---------------------------------------

    private void Awake()
    {
        //描画用surfaceの準備
        _inputSurface = transform.Find("InputSurface").gameObject;       
        Texture2D mainTexture = (Texture2D)_inputSurface.GetComponent<Renderer>().material.mainTexture;
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


        //見本用のsurfaceの準備
        _modelSurface = transform.Find("ModelSurface").gameObject;
        _modelSurface.SetActive(false);
       
    }

    void Update()
    {
        if (_isFinished || !_isDoingTask) return;

        //ドローイング中の時間を図る
        _completeTimer += Time.deltaTime;

        if (_isDrawing) _drawingTimer += Time.deltaTime;
        else _timeToStart += Time.deltaTime;
    }

    //--------------------------public関数-------------------------------------

    public void ReadyToDraw(string modelName)
    {
        _isDoingTask = true;
        _modelSurface.SetActive(true);
        _modelName = modelName;

        //hightとangleの取得
        _height = gameObject.transform.position.y;
        _angle = gameObject.transform.localEulerAngles.z;

        //modelSurfaceにお題を表示
        string filePath = Application.dataPath + $@"/Resources/Models/{modelName}.png";
        Texture2D modelTexture = (Texture2D)_modelSurface.GetComponent<Renderer>().material.mainTexture;
        PutPng(filePath, modelTexture);
    }

    public void ClearCanvas()
    {
        Clear();
        _dotsList.Clear();
    }

    public void EndThisDrawing()
    {
        _isDoingTask = false;
        _isDrawing = false;
        _isFinished = true;

        _modelSurface.SetActive(false);

        CalcError();

        //ここでこのSurfaceを消したい
        gameObject.SetActive(false);
    }

    public void DrawLine(RaycastHit hit)
    {
        Draw(hit.textureCoord * _textureSize);

        if (!_isDrawing)
        {
            _isDrawing = true;
        }
    }

    public void ResetLastTouchVec()
    {
        _lastTouchVec = Vector2.zero;
    }

    public void SaveInputSurface(string filePath)
    {
        var texture = (Texture2D)_inputSurface.GetComponent<Renderer>().material.mainTexture;
        SaveTexture(filePath, texture);
    }

    //---------------------------private関数------------------------------------
    //----------------------------データ関連-----------------------------------

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
        _modelSurface.GetComponent<Renderer>().material.mainTexture = instanceTexture;
    }

    private byte[] ReadPngFile(string path)
    {
        FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
        BinaryReader bin = new BinaryReader(fileStream);
        byte[] values = bin.ReadBytes((int)bin.BaseStream.Length);

        bin.Close();

        return values;
    }

    private void CalcError()
    {
        //平均誤差を求める
        if (_dotsList.Count > 50)
        {
            var meanDeviation = (float)MeanDeviation.CalcMeanDeviation(_dotsList, _modelName);　//単位: pxel
            _error = meanDeviation * _inputSurface.transform.localScale.x / (float)_textureSize; //単位: m
        }
        else
        {
            Debug.Log($@"{this.name}に描画されていません");
        }
    }

    //---------------------------描画関連------------------------------------

    private  void Draw(Vector2 p)
    {
        int dotSize = 5;　// ここの数字要調節
        float lerpCountAdjustNum = dotSize * 1.5f; // ここの数字要調節 大きいほど荒くなる

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
        _inputSurface.GetComponent<Renderer>().material.mainTexture = _instanceTexture;

        //ベクトルpとその点が付いた時間をベクトルに保存
        _dotsList.Add(new Vector3(p.x, p.y, Time.time));
    }

    /// <summary>
    /// 指定されたピクセルにところに点を打つ
    /// </summary>
    private void Dot(Vector2 p, int dotSize, object color = null)
    {
        if (color == null) color = Color.black;
        var data = _instanceTexture.GetRawTextureData<Color32>();

        //円形のドットを打つアルゴリズム
        /*for (int i = Mathf.Max(-dotSize,-(int)p.x); i <= Mathf.Min(dotSize , _textureSize - (int)p.x) ; i++)
        {
            for (int j =Mathf.Max( -(dotSize - Mathf.Abs(i)) , -(int)p.y); j <= Mathf.Min((dotSize - Mathf.Abs(i)),_textureSize - (int)p.y); ++j)
            {
                _buffer.SetValue(Color.black, ((int)p.x + i) + _textureSize * ((int)p.y + j));
            }
        }*/

        //四角のドットを打つアルゴリズム
        for (int i = Mathf.Max(-dotSize, -(int)p.x); i <= Mathf.Min(dotSize, _textureSize - (int)p.x); i++)
        {
            for (int j = Mathf.Max(-dotSize, -(int)p.y); j <= Mathf.Min(dotSize, _textureSize - (int)p.y); ++j)
            {
                //SetPixelsを使う場合
                /*if(_buffer.GetValue(((int)p.x + i) + _textureSize * ((int)p.y + j)).Equals(Color.white))
                {
                    _buffer.SetValue(color, ((int)p.x + i) + _textureSize * ((int)p.y + j));
                }*/

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
        _inputSurface.GetComponent<Renderer>().material.mainTexture = _instanceTexture;
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
