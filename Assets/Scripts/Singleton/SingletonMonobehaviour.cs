using System;
using UnityEngine;


    public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {

        private static T _instance;
        public static T Instance
        {
            get
            {
                try
                {
                    if (_instance == null)
                    {
                        Type t = typeof(T);

                        _instance = (T)FindObjectOfType(t);
                        if (_instance == null)
                        {
                            Debug.LogWarning(t + " をアタッチしているGameObjectはありません. ");
                        }
                    }

                    return _instance;
                }
                catch (UnityException ex)
                {
                    Debug.LogError(ex.Message);
                    return null;
                }
            }
        }

        virtual protected void Awake()
        {
            if (CheckInstance())
            {
                // インスタンスが作られていない場合
                //DontDestroyOnLoad(gameObject);

                // 初期化
                Init();
            }
        }

        /// <summary>
        /// シーン遷移時に自身を破棄する
        /// </summary>
        virtual protected void OnDestroy()
        {
            Destroy(gameObject);
        }

        /// <summary>
        /// まだインスタンスが作られていない場合に呼び出される
        /// </summary>
        abstract protected void Init();

        /// <summary>
        /// 他のゲームオブジェクトにアタッチされているか調べる.
        /// アタッチされている場合は破棄する.
        /// </summary>
        /// <returns></returns>
        protected bool CheckInstance()
        {
            if (_instance == null)
            {
                _instance = this as T;
                return true;
            }
            else if (Instance == this)
            {
                return true;
            }
            Debug.Log($"{typeof(T)}のインスタンスはすでに作成されていたため、{gameObject.scene.name}に存在する{typeof(T)}は破棄されました。");
            Destroy(gameObject);
            return false;
        }
    }
