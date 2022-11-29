using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using RVO;
using Vector2 = RVO.Vector2;

public class RVOPathPlanner : BasePathPlanner
{
    /*[SerializeField] [TextArea] private string instructions = "RVO2を用いてシミュレーションを行う。" +
                                                              "各エージェントの情報は、対応するオブジェクトそれぞれが決めている。" +
                                                              "このクラスではdefaultsの設定と、シミュレーションを1ステップ進めるのみ。";*/
    [Header("AgentDefaultsパラメータ")]
    [SerializeField] private float neighborDist = 3f;
    [SerializeField] private int maxNeighbors = 4;
    [SerializeField] private float timeHorizon = 0.5f;
    [SerializeField] private float timeHorizonObst = 5f;
    [SerializeField] private float defaultAgentRadius = 0.4f;
    [SerializeField] private float maxSpeed = 0.8f;

    [Header("そのほかのパラメータ")]
    [SerializeField] private float _playerRadius = 0.7f;

    private Vector2 _playerLastPosinton;
    private int _playerSid = -1;

    private void Awake()
    {
        Simulator.Instance.setAgentDefaults
            (neighborDist, maxNeighbors, timeHorizon, timeHorizonObst, defaultAgentRadius, maxSpeed, new Vector2(0.0f, 0.0f));

        Simulator.Instance.setTimeStep(Time.fixedDeltaTime);        
    }

    public override void PathPlan(Transform userPos)
    {
        //最初のフレームだけこの処理をする（StartだとまだHMDのトラッキングができていない）
        if(_playerSid < 0)
        {
            _playerSid = Simulator.Instance.addAgent(Get2DPosition(userPos));
            Simulator.Instance.setAgentRadius(_playerSid, _playerRadius);
            _playerLastPosinton = Get2DPosition(userPos);
        }

        //playerの位置をシミュレーションに入れる
        Simulator.Instance.setAgentPosition(_playerSid, Get2DPosition(userPos));

        //playerの速度をシミュレーションに入れる
        var playerVelocity = (Get2DPosition(userPos) - _playerLastPosinton) / Time.deltaTime;
        Simulator.Instance.setAgentPrefVelocity(_playerSid, playerVelocity);

        _playerLastPosinton = Get2DPosition(userPos);

        //シミュレーターを1ステップ進める
        Simulator.Instance.doStep();
    }

    private static Vector2 Get2DPosition(Transform trs)
    {
        var position = trs.position;
        return new Vector2(position.x, position.z);
    }
}
