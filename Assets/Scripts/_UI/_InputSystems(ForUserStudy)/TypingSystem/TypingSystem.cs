using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.UI;
using TypingPhraseDisplaySample;
using System.Linq;

public class TypingSystem : RayInputSystem
{
    private enum Method
    {
        VRSurface,
        MidAir,
        Tablet
    }
    [Header("ファイル名")]
    [SerializeField] private  Method _method = Method.VRSurface;
    [SerializeField] private string _fileName = "sample";
    [SerializeField] private int _allTaskNum = 2;

    [Header("StickyNotes")]
    [SerializeField] private List<GameObject> _stickyNoteObjects = null;

    private StickyNote _nowSelectedStickyNote = null;
    private List<StickyNote> _stickyNotes = new List<StickyNote>();

    //キーボード入力関係
    private GameObject _lastPressedKey;
    private bool _isAbleToType = true;

    //全タスク完了時間測定関係
    private float _allTaskCompleteTime = 0;
    private bool _isDoingTask = false;
    private int _finishedTaskNum = 0;

    //modelText生成関係
    private string[] _phraseSet;
    private Queue<string> _phraseQueue;
    readonly int _phraseLength = 3;

    //Prop
    private Prop _prop;

    private Coroutine coroutine;

    private Queue<float> _heightQueue = new Queue<float>();

    //---------------------------------------------------------------
    private void Awake()
    {
        // フレーズセットは以下のものを採用
        // https://dl.acm.org/doi/10.1145/765891.765971
        _phraseSet = Resources.Load<TextAsset>("phrases").text.Split('\n');
        ResetSequence();

        //propの設定
        if(_method == Method.VRSurface)_prop = GameObject.Find("SimuratedProp").GetComponent<Prop>();

        //タスクの開始
        _isDoingTask = true;
        _nowSelectedStickyNote = null;

        //StickyNotesの高さをランダムに設定
        ResetHightQueue();
        foreach(GameObject stickyNoteObj in _stickyNoteObjects)
        {
            stickyNoteObj.GetComponent<StickyNote>().Height = _heightQueue.Dequeue();
        }
    }

    private void Update()
    {
        //タスク中でなければ以降は何もしない。
        if (!_isDoingTask) return;

        //全タスクの時間測定
        _allTaskCompleteTime += Time.deltaTime;

        //今のタスクが完了しているかどうかチェック
        if (_nowSelectedStickyNote != null && _nowSelectedStickyNote.IsFinished)
        {
            _nowSelectedStickyNote = null;
            _finishedTaskNum += 1;
            if (_method == Method.VRSurface) _prop.IsSurfaceUsed = false;

            if (_finishedTaskNum == _allTaskNum)
            {
                EndTask();
                return;
            }
            else
            {               
                //_stickyNoteObjects[_finishedTaskNum].SetActive(true);
            }
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            EndTask();
        }
    }

    //--------------------------RayInteraction-------------------------------------

    public override void RayInteraction(RaycastHit hit)
    {
        if (!_isDoingTask) return;

        //StickyNoteの選択
        if (_nowSelectedStickyNote == null && hit.collider.gameObject.CompareTag("TextDisplay"))//何らかの方法でStickyNoteを取得
        {
            var tempStickyNote = hit.collider.gameObject.GetComponentInParent<StickyNote>();

            //今使用中のSteckyNoteやタスクの終わったStickyNoteは選択しない
            if (tempStickyNote == _nowSelectedStickyNote || tempStickyNote.IsFinished) return;

            //そのstickyNoteを選択
            _nowSelectedStickyNote = tempStickyNote;
            _stickyNotes.Add(_nowSelectedStickyNote);

            //テキストリストからmodelTextを選択
            string modelText = "";
            for (int i = 0; i < _phraseLength; i++)
            {
                modelText += (_phraseQueue.Dequeue()).ToLower().TrimStart().TrimEnd() + " ";
            }
            modelText = modelText.TrimEnd();

            //stickyNoteにmodelTextを張る
            _nowSelectedStickyNote.ReadyToType(modelText);

        }
        //keyboardによる入力
        else if (_nowSelectedStickyNote != null &&  hit.collider.gameObject.CompareTag("KeyBoard"))
        {
            if(hit.collider.gameObject == _lastPressedKey)
            {
                //コルーチンのリスタート
                StopCoroutine(coroutine);
                coroutine = StartCoroutine(StopType());
                return;
            }

            if (!_isAbleToType) return;

            //hit.collider.gameObjectの色を灰色に
            hit.collider.gameObject.GetComponent<Renderer>().material.color = Color.gray;

            //キーボード入力
            string KeyText = hit.collider.gameObject.GetComponentInChildren<Text>().text;
            if (_nowSelectedStickyNote.InputText == "") _nowSelectedStickyNote.StartTyping();

            if (_method == Method.VRSurface) _prop.IsSurfaceUsed = true;

            switch (KeyText)
            {
                case "":
                    break;

                case "Back":
                    string inputText = _nowSelectedStickyNote.InputText;
                    if (inputText.Length > 0)
                    {
                        //一文字消して、押した回数をエラーとして記録
                        _nowSelectedStickyNote.InputText = inputText.Remove(inputText.Length - 1, 1);
                        _nowSelectedStickyNote.AddErrorTime();
                    }
                    break;

                case "Space":
                    var lastChar = _nowSelectedStickyNote.InputText[_nowSelectedStickyNote.InputText.Length - 1];
                    if(!char.IsWhiteSpace(lastChar))
                    {
                        _nowSelectedStickyNote.InputText += " ";
                    }                  
                    break;
                default:
                    _nowSelectedStickyNote.InputText += KeyText.ToLower();
                    break;
            }

            //もろもろ更新
            _lastPressedKey = hit.collider.gameObject;
            
            coroutine = StartCoroutine(StopType());
        }
    }

    public override void NoTouch()
    {
        ResetLastKey();
    }

    //-------------------------private関数--------------------------------------

    private void ResetLastKey()
    {
        _lastPressedKey.GetComponent<Renderer>().material.color = Color.black;
        _lastPressedKey = null;

        //ここでlastPressedKeyの色を戻す
    }

    private void EndTask()
    {
        _isDoingTask = false;

        //FilePathの作成
        string filePath = Application.dataPath + $@"/Data/Typing/{_method}/{_fileName}_{_method}.csv";       //拡張し付ける？
        CSVManager.CreateFile(filePath);

        //以下、研究データの生成
        string result = "";

        //ラベルを作る
        result += "Task,modelText,Height,Words,TaskTime(s),TypingTime(s),WPM,Error,TimeToStart" + "\n";

        //以降データ関係の文字列
        float sumTypingTime = 0;
        float sumCompTime = 0;
        int sumWordNum = 0;
        int sumErrorTimes = 0;
        float sumWPM = 0;
        float sumTimeToTask = 0;
        for(int i = 0; i < _stickyNotes.Count; i++)
        {
            //各タスクに関するデータ
            result +=
                $@"{i},{_stickyNotes[i].ModelText},{_stickyNotes[i].Height},{_stickyNotes[i].WordNum},{_stickyNotes[i].CompleteTime},{_stickyNotes[i].TypingTime},{(float)_stickyNotes[i].WordNum / _stickyNotes[i].TypingTime * 60f},{_stickyNotes[i].ErrorTime},{_stickyNotes[i].TimeToStart}" + "\n";

            sumCompTime += _stickyNotes[i].CompleteTime;
            sumTypingTime += _stickyNotes[i].TypingTime;
            sumWordNum += _stickyNotes[i].WordNum;
            sumErrorTimes += _stickyNotes[i].ErrorTime;
            sumWPM += (float)_stickyNotes[i].WordNum / _stickyNotes[i].TypingTime * 60f;
            sumTimeToTask += _stickyNotes[i].TimeToStart;
        }

        //前タスクにかんするデータ
        result +=
            $@"all,-,-,{sumWordNum},{sumCompTime / _stickyNotes.Count},{sumTypingTime / _stickyNotes.Count},{(float)sumWPM / _stickyNotes.Count},{sumErrorTimes},{sumTimeToTask / _stickyNotes.Count}" + "\n";

        result += $@"allTaskTime,{_allTaskCompleteTime}";

        CSVManager.Write(filePath, result, FileMode.Append);

        Debug.Log($"{filePath}にデータを保存しました");
    }

    private void ResetSequence()
    {
        _phraseQueue = new Queue<string>(_phraseSet.GetRandom(_allTaskNum * _phraseLength));
    }

    private IEnumerator StopType()
    {
        _isAbleToType = false;

        yield return new WaitForSeconds(0.1f);//要調整


        ResetLastKey();
        _isAbleToType = true;
    }

    private void ResetHightQueue()
    {
        float[] selectedHights = { 1.0f, 1.0f, 1.0f, 1.2f, 1.2f, 1.2f, 1.4f, 1.4f, 1.4f };

        var hights = selectedHights
            .OrderBy(_ => Guid.NewGuid())
            .Select(j => j);

        foreach(float hight in hights)
        {
            _heightQueue.Enqueue(hight);
        }
    }
}
