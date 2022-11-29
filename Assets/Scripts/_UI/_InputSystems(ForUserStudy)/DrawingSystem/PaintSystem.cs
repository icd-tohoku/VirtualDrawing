using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEditor;

public class PaintSystem : RayInputSystem
{
    private enum Method
    {
        VRSurface,
        MidAir,
        Tablet
    }
    [Header("ファイル名")]
    [SerializeField] private Method _method = Method.VRSurface;
    [SerializeField] private string _directryName = "sample";
    [SerializeField] private int _allTaskNum = 2;

    [Header("Canvases")]
    [SerializeField]private List<GameObject> _canvasObjects = null;

    //各DrawCanvasについての情報
    private DrawCanvas _nowSelectedDrawCanvas = null;
    private List<DrawCanvas> _drawCanvasesList = new List<DrawCanvas>();

    //全タスク完了時間測定関係
    private float _allTaskCompleteTime = 0;
    private bool _isDoingTask = false;
    
    private int _finishedTaskNum = 0;
    private float _timer = 0;
    private bool _isAbleToDraw = true;
    private bool _isInteractionCalled = false;

    private Queue<string> _modelQueue = new Queue<string>();
    private Queue<float> _angleQueue = new Queue<float>();
    private Queue<float> _heightQueue = new Queue<float>();

    private Prop _prop;

    //---------------------------------------------------------------------

    private void Awake()
    {
        //propの設定
        if (_method == Method.VRSurface) _prop = GameObject.Find("SimuratedProp").GetComponent<Prop>();

        _isDoingTask = true;
        _nowSelectedDrawCanvas = null;
        ResetModelQueue();
        
        //Canvasの高さと角度をランダムに決定
        ResetAngleAndHeightQueue();
        foreach(GameObject CanvasObj in _canvasObjects)
        {
            CanvasObj.GetComponent<DrawCanvas>().Angle = _angleQueue.Dequeue();
            CanvasObj.GetComponent<DrawCanvas>().Height = _heightQueue.Dequeue();
        }

        /*for(int i = 1; i < _canvasObjects.Count; i++)
        {
            _canvasObjects[i].SetActive(false);
        }

        _canvasObjects[0].GetComponent<DrawCanvas>().Angle = _angleQueue.Dequeue();*/
    }

    private void Update()
    {
        if (!_isDoingTask) return;

        //全タスク完了時間の計測
        _allTaskCompleteTime += Time.deltaTime;

        //
        if(_nowSelectedDrawCanvas != null && _nowSelectedDrawCanvas.IsFinished)
        {
            _nowSelectedDrawCanvas = null;
            _finishedTaskNum += 1;
            if (_method == Method.VRSurface) _prop.IsSurfaceUsed = false;
            if (_finishedTaskNum == _allTaskNum)
            {
                EndTask(); 
                _isDoingTask = false;
                return;
            }
            else
            {

            }
        }

        

        if (Input.GetKeyDown(KeyCode.E))
        {
            EndTask();
        }
    }

    private void FixedUpdate()
    {
        if (_isInteractionCalled)
        {
            _isInteractionCalled = false;
            _timer = 0;
        }
        else
        {
            _timer += Time.deltaTime;
            if(_timer > 0.05f)
            {
                NoTouch();
                _timer = 0;
            }
        }
    }

    //---------------------------------------------------------------------

    public override void RayInteraction(RaycastHit hit)
    {
        if (!_isDoingTask || !_isAbleToDraw) return;    

        if(_nowSelectedDrawCanvas == null && hit.collider.gameObject.CompareTag("DrawCanvas"))
        {
            var tempDrawCanvas = hit.collider.gameObject.GetComponentInParent<DrawCanvas>();

            //今使用中のSteckyNoteやタスクの終わったStickyNoteは選択しない
            if (tempDrawCanvas == _nowSelectedDrawCanvas || tempDrawCanvas.IsFinished) return;

            //そのDrawCanvasを選択
            _nowSelectedDrawCanvas = tempDrawCanvas;
            _drawCanvasesList.Add(_nowSelectedDrawCanvas);            

            //DrawCanvasの準備
            var modelName = _modelQueue.Dequeue();
            _nowSelectedDrawCanvas.ReadyToDraw(modelName);

            StartCoroutine(StopDrawing());
        }
        else if(_nowSelectedDrawCanvas != null && hit.collider.gameObject.CompareTag("DrawCanvas"))
        {
            _isInteractionCalled = true;

            if (_method == Method.VRSurface) _prop.IsSurfaceUsed = true;

            //Rayが当たった場所を送って点を打ってもらう
            _nowSelectedDrawCanvas.DrawLine(hit);
        }
        else if(_nowSelectedDrawCanvas != null && hit.collider.gameObject.CompareTag("KeyBoard"))
        {
            string KeyText = hit.collider.gameObject.GetComponentInChildren<Text>().text;
            switch (KeyText)
            {
                case "END":
                    _nowSelectedDrawCanvas.EndThisDrawing();
                    break;
                case "RESET":
                    _nowSelectedDrawCanvas.ClearCanvas();
                    break;
                default:
                    break;
            }
        }
    }

    public override void NoTouch()
    {
        if(_nowSelectedDrawCanvas != null)
        {
            _nowSelectedDrawCanvas.ResetLastTouchVec();
        }
    }


    //-------------------------private関数--------------------------------------

    private void EndTask()
    {
        _isDoingTask = false;

        //FilePathの作成
        string filePath = Application.dataPath + $@"\Data\Drawing\{_method}\{_directryName}_{_method}";
        SafeCreateDirectory(filePath);
        
        //以下、研究データの生成
        string result = "";

        //ラベルを作る
        result += "Task,Model,Height,Angle,TaskTime,DrawingTime,MeanDeviation,TimeToStart" + "\n";

        //各タスクごとに計算
        float sumCompTime = 0;
        float sumError = 0;
        float sumDrawingTime = 0;
        float sumTimeToStart = 0;
        int drawCanvasNum = _drawCanvasesList.Count;
        for (int i = 0; i < _drawCanvasesList.Count; i++)
        {
            if (_drawCanvasesList[i].Error == 0f)
            {
                drawCanvasNum -= 1;
                continue;
            }

                //各タスクの必要データを取得
                result +=
                $@"{i},{_drawCanvasesList[i].ModelName},{_drawCanvasesList[i].Height},{_drawCanvasesList[i].Angle},{_drawCanvasesList[i].CompleteTime},{_drawCanvasesList[i].DrawingTime},{_drawCanvasesList[i].Error},{_drawCanvasesList[i].TimeToStart}" + "\n";

            sumCompTime += _drawCanvasesList[i].CompleteTime;
            sumError += _drawCanvasesList[i].Error;
            sumDrawingTime += _drawCanvasesList[i].DrawingTime;
            sumTimeToStart += _drawCanvasesList[i].TimeToStart;

            _drawCanvasesList[i].SaveInputSurface(filePath + $@"\{i}_{_directryName}_{_method}_{_drawCanvasesList[i].ModelName}.png");

            SaveDotsAsCSV(_drawCanvasesList[i].DotList, filePath + $@"\{i}_{_directryName}_{_method}_{_drawCanvasesList[i].ModelName}_dots.csv");
        }

        //全タスクの必要データを取得
        result +=
            $@"all,-,-,-,{sumCompTime / drawCanvasNum},{sumDrawingTime / drawCanvasNum},{sumError / drawCanvasNum},{sumTimeToStart / drawCanvasNum}" + "\n";

        result += $@"allTaskTime,{_allTaskCompleteTime}";

        //ファイルを出力
        filePath += $@"\{_directryName}_{_method}_result.csv";
        CSVManager.CreateFile(filePath);
        CSVManager.Write(filePath, result, FileMode.Append);
        AssetDatabase.Refresh();

        Debug.Log($"{filePath}にデータを保存しました");
    }

    private IEnumerator StopDrawing()
    {
        _isAbleToDraw = false;

        yield return new WaitForSeconds(1f);

        _isAbleToDraw = true;
    }

    private void ResetModelQueue(string path = null)
    {
        if(path == null) path = Application.dataPath + $@"/Resources/Models";

        for(int i = 0; i < 3; i++)
        {
            string[] selectedNames = {"Octagram", "Boxes", "Decagram" }; //一時的にDiamondでなくOctagramを採用

            var names = selectedNames
                .OrderBy(_ => Guid.NewGuid())
                .Select(j => j);

            foreach (string name in names)
            {
                _modelQueue.Enqueue(name);
            }
        }
        
        //ファイルの中の図形ランダムに選ぶなら下の方法
        /*var names = Directory.GetFiles(path, "*.png")
            .Select(i => Path.GetFileNameWithoutExtension(i))
            .OrderBy(_ => Guid.NewGuid())
            .Select(i => i);*/
    }

    private void ResetAngleQueue()
    {
        for (int i = 0; i < 3; i++)
        {
            float[] selectedAngles = { 20, 45, 70 };

            var angles = selectedAngles
                .OrderBy(_ => Guid.NewGuid())
                .Select(j => j);

            foreach(float angle in angles)
            {
                _angleQueue.Enqueue(angle);
            }
        }
    }

    private void ResetAngleAndHeightQueue()
    {
        int[] selectedIndex = { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

        var randIndex = selectedIndex
            .OrderBy(_ => Guid.NewGuid())
            .Select(j => j);

        foreach(int index in randIndex)
        {
            switch (index)
            {
                case 1:
                    _angleQueue.Enqueue(25f);
                    _heightQueue.Enqueue(1f);
                    break;
                case 2:
                    _angleQueue.Enqueue(25f);
                    _heightQueue.Enqueue(1.2f);
                    break;
                case 3:
                    _angleQueue.Enqueue(25f);
                    _heightQueue.Enqueue(1.4f);
                    break;
                case 4:
                    _angleQueue.Enqueue(45f);
                    _heightQueue.Enqueue(1f);
                    break;
                case 5:
                    _angleQueue.Enqueue(45f);
                    _heightQueue.Enqueue(1.2f);
                    break;
                case 6:
                    _angleQueue.Enqueue(45f);
                    _heightQueue.Enqueue(1.4f);
                    break;
                case 7:
                    _angleQueue.Enqueue(65f);
                    _heightQueue.Enqueue(1f);
                    break;
                case 8:
                    _angleQueue.Enqueue(65f);
                    _heightQueue.Enqueue(1.2f);
                    break;
                case 9:
                    _angleQueue.Enqueue(65f);
                    _heightQueue.Enqueue(1.4f);
                    break;
                default:
                    Debug.LogError("ResetQueue Error");
                    _angleQueue.Enqueue(20f);
                    _heightQueue.Enqueue(1f);
                    break;

            }
        }
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

    private void SaveDotsAsCSV(List<Vector3> dots, string filePath)
    {
        string dotsText = "";
        foreach(Vector3 dot in dots)
        {
            dotsText += $@"{dot.x},{dot.y},{dot.z}" + "\n";
        }

        CSVManager.CreateFile(filePath);
        CSVManager.Write(filePath, dotsText, FileMode.Append);
        AssetDatabase.Refresh();
    }
}
