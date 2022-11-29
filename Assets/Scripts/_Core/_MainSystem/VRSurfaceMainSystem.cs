using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class VRSurfaceMainSystem : MonoBehaviour
{
    [Header("algorithm")]
    [SerializeField] private BaseTargetDeterminer goalDeterminer = null;
    [SerializeField] private BaseDispatcher dispatcher = null;
    [SerializeField] private BasePathPlanner pathPlanner = null;

    [Header("Player")]
    [SerializeField] private GameObject _player = null;
    public GameObject Player
    {
        get
        {
            return _player;
        }
    }

    [Header("Prop")]
    [SerializeField] private Prop[] _props = null;

    private float timer = 0;

    void Start()
    {
        //エラー処理
        if(_player == null)
        {
            Debug.Break();
            throw new System.NullReferenceException($"Set VRPlayer Camera");
        }
        if (_props.Length == 0)
        {
            Debug.Break();
            throw new System.NullReferenceException($"Set Simulated VR Surface");
        }
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer < 0.1f) return;
        timer = 0;

        var goalsList = goalDeterminer.FindTarget(_player.transform);

        //UIや３Dオブジェクトの一部など、物理面を配置すべき場所を特定
        //全てのゴールから優先的に配置するべきゴールの順番を設定
        //goalsList.AddRange(goalDeterminer.FindGoals(player.transform));

        //特定した配置場所にどのプロップを配置するか決める
        dispatcher.Dispatch(_props, goalsList);

        //RVOのシミュレーションを進める
        pathPlanner.PathPlan(_player.transform);
    }

}
