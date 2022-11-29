using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DispatchByDistance : BaseDispatcher
{
    
    [SerializeField] private Transform[] standbyPoints;     //surfaceと同じ数以上必要

    public override void Dispatch(Prop[] props, List<Transform> goals)
    {
        var assignArray = Assignment(props, goals);
        int standbyPointIndex = 0;

        //各SurfaceのTargetを格納
        for (int propIndex = 0; propIndex < props.Length; ++propIndex)
        {
            if(assignArray[propIndex] <  goals.Count)
            {
                if (goals[assignArray[propIndex]] != props[propIndex].Target)
                {
                    //予測地点に移動するものはDispached状態に
                    props[propIndex].propState = Prop.PropState.dispatched;
                    props[propIndex].Target = goals[assignArray[propIndex]];
                }
            }
            else
            {
                if(standbyPoints[standbyPointIndex] != props[propIndex].Target)
                {
                    //スタンバイポイントに移動するSurfaceはstandby状態にしてスタンバイポイントに送る
                    props[propIndex].propState = Prop.PropState.standby;
                    props[propIndex].Target = standbyPoints[standbyPointIndex];
                }            
                ++standbyPointIndex;
            }
        }
    }

    /// <summary>
    /// Surface配列の順に沿って、各Surfaceが行くべきGoalのIndexを返す
    /// </summary>
    private int[] Assignment(Prop[] props, List<Transform> goals)
    {      
        //例外処理、Surfaceが多すぎたらエラー
        if (props.Length > goals.Count + standbyPoints.Length)
        {
            throw new System.ArgumentException();
        }

        //コスト配列の作成（Surfaceの数に合わせて正方行列を作成する）
        int[,] costs = new int[props.Length, props.Length];
        for (int rowIndex = 0; rowIndex < props.Length; ++rowIndex)
        {
            for (int columnIndex = 0; columnIndex < props.Length; ++columnIndex)
            {
                if(columnIndex < goals.Count)
                {
                    //各Surfaceと各goalの距離をコスト配列に格納
                    var surfacePos = new Vector2(props[rowIndex].gameObject.transform.position.x, props[rowIndex].gameObject.transform.position.z);
                    var goalPos = new Vector2(goals[columnIndex].position.x, goals[columnIndex].position.z);

                    //精度を上げるために100倍している
                    costs[rowIndex, columnIndex] = (int)(Vector2.Distance(surfacePos, goalPos) * 100f);
                }
                else
                {
                    //コスト配列を正方行列にするため、Surfaceの数が多かったらコスト最大の仮想のGoalを作成する。
                    costs[rowIndex, columnIndex] = int.MaxValue;
                }              
            }
        }
        //ハンガリアン法による割り当て　詳しくは以下サイトを参照
        //https://github.com/vivet/HungarianAlgorithm 
        int[] result = HungarianAlgorithm.HungarianAlgorithm.FindAssignments(costs);

        return result;
    }


    private Transform[] ChangeStandbyPointOrder(Transform[] standbyPoints)
    {
        //test

        throw new System.NotImplementedException();
    }

}
