using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StickyNote : MonoBehaviour
{
    private Text modelTextDisplay;
    private Text inputTextDisplay;
    private Color defaultColor;
    
    /// <summary>
    /// 見本のテキスト
    /// </summary>
    private string modelText = "";
    public string ModelText
    {
        get
        {
            return modelText;
        }
        set
        {
            modelText = value;
            modelTextDisplay.text = modelText;
        }
    }

    /// <summary>
    /// ユーザが入力しているテキスト
    /// </summary>
    private string inputText = "";
    public string InputText
    {
        get
        {
            return inputText;
        }
        set
        {
            if (isDoingTask)
            {
                inputText = value;
                
                if(modelText == inputText)
                {
                    //入力が完了したとき
                    CompleteThisTyping();
                    inputTextDisplay.text = inputText;
                    inputTextDisplay.color = Color.green;
                }
                else if (modelText.StartsWith(inputText))
                {
                    //入力中で、テキストに間違いがない時
                    inputTextDisplay.text = inputText + "_";
                    inputTextDisplay.color = defaultColor;
                }
                else if (inputText.Length >= inputTextDisplay.text.Length)
                {
                    //テキストに間違いがあり、文字が入力されたとき
                    inputTextDisplay.text = inputText + "_";
                    _audioSource.PlayOneShot(_audioSource.clip);
                    inputTextDisplay.color = Color.red;
                }
                else
                {
                    //テキストに間違いがあり、文字が消された時
                    inputTextDisplay.text = inputText + "_";
                    inputTextDisplay.color = Color.red;
                }
            }          
        }
    }
     
    /// <summary>
    /// このタイピングにかかった時間を計算するのに使う
    /// </summary>
    private bool isDoingTask = false;

    private bool isTyping = false;

    /// <summary>
    /// このタイピングが終わっているかどうかの確認に使う
    /// </summary>
    private bool isFinished = false;
    public bool IsFinished
    {
        get
        {
            return isFinished;
        }
    }

    private int wordNum = 0;
    public int WordNum
    {
        get
        {
            return wordNum;
        }
    }

    private float completeTimer = 0;
    public float CompleteTime
    {
        get
        {
            return completeTimer;
        }
    }

    private float _typingTime = 0;
    public float TypingTime
    {
        get
        {
            return _typingTime;
        }
    }

    private int errorTime = 0;
    public int ErrorTime
    {
        get
        {
            return errorTime;
        }
    }

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

    private float _timeToStart = 0f;
    public float TimeToStart
    {
        get
        {
            return _timeToStart;
        }
    }

    private AudioSource _audioSource;
    //----------------------------------------MonoBehavior--------------------------------------------
    void Awake()
    {
        try
        {
            modelTextDisplay = transform.Find("hed").transform.Find("Canvas").transform.Find("ModelText").GetComponent<Text>();
            inputTextDisplay = transform.Find("note").transform.Find("Canvas").transform.Find("InputText").GetComponent<Text>();

            modelTextDisplay.text = "";
            inputTextDisplay.text = "";

            defaultColor = inputTextDisplay.color;

            _height = gameObject.transform.position.y;

            _audioSource = GetComponent<AudioSource>();
        }
        catch(System.NullReferenceException e)
        {
            Debug.Break();
            Debug.LogError(e);
        }
    }

    private void Update()
    {
        if (isFinished || !isDoingTask) return;

        //タイピング中の時間を図る
        completeTimer += Time.deltaTime;

        if (isTyping) _typingTime += Time.deltaTime;
        else _timeToStart += Time.deltaTime;

    }

    //------------------------------------------Public関数--------------------------------------------

    public void ReadyToType(string modelText)
    {
        isDoingTask = true;

        this.modelText = modelText;
        modelTextDisplay.text = this.modelText;
        wordNum = modelText.Length;

        this.inputText = "";
        inputTextDisplay.text = "_";
    }

    public void StartTyping()
    {
        if (!isTyping)
        {
            isTyping = true;
            _typingTime = 0;
        }
    }

    public void AddErrorTime()
    {
        errorTime += 1;
    }

    //-----------------------------------------------private関数---------------------------------------------
    /// <summary>
    /// //modelTextとinputTextが一致した時の処理
    /// </summary>
    private void CompleteThisTyping()
    {
        isDoingTask = false;
        isTyping = false;
        isFinished = true;

        gameObject.SetActive(false);
    }

    private void ResetStatus()
    {
        completeTimer = 0;
        this.modelText = "";
        modelTextDisplay.text = this.modelText;
        this.inputText = "";
        inputTextDisplay.text = this.inputText;
        errorTime = 0;
    }
}
