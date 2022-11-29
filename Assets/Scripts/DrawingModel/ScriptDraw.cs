using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ScriptDraw : MonoBehaviour
{
    [SerializeField] DrawController controller;
    private LineList _lineList;

    [SerializeField] List<OrthogonalPolygonPara> polygonParas = new List<OrthogonalPolygonPara>();

    [System.Serializable]
    private struct OrthogonalPolygonPara
    {
        public int Num;
        public Vector2 BasePoint;
        public float BaseDeg;
        public float R;
        public int Skip;

        public OrthogonalPolygonPara(int n, Vector2 bp, float bd, float r, int skip)
        {
            Num = n;BasePoint = bp;BaseDeg = bd;R = r;Skip = skip;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        _lineList = controller._lineList;

        _lineList.Lines.Clear();

        ///////////////////////////////////////ここに動作を記述////////////////////////////////////////////////////////

        //AddAggregate( CulPolygonPoints(3, new Vector2(512, 512), 90, 400), 1);
        //AddAggregate( CulPolygonPoints(3, new Vector2(512, 512), -90, 200), 1);

        //AddAggregate( CulPolygonPoints(4, new Vector2(512, 512), 45, 400), 1);
        //AddAggregate( CulPolygonPoints(4, new Vector2(512, 512), 45, 200), 1);
        //AddAggregate( CulPolygonPoints(4, new Vector2(512, 512), 45, 50), 1);


    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            _lineList.Lines.Clear();

            foreach (var para in polygonParas)
            {
                AddAggregate( CulPolygonPoints(para.Num,para.BasePoint,para.BaseDeg,para.R), para.Skip);
            }
        }
    }

    /// <summary>
    /// {num}角形の座標群を返します
    /// </summary>

    private List<Vector2> CulPolygonPoints(int num, Vector2 basePoint,float baseDeg ,float r)
    {
        List<Vector2> points = new List<Vector2>();
        float oneAngle = 360 / num;
        for(int i = 0; i < num; i++)
        {
            points.Add((new Vector2((int) (r * Mathf.Cos(Mathf.Deg2Rad * (baseDeg + oneAngle * i))), (int) (r * Mathf.Sin(Mathf.Deg2Rad * (baseDeg + oneAngle * i)))) + basePoint));
        }
        return points;
    }


    /// <summary>
    /// 配列を順番につないで行きます．最後の点は最初の点と接続されます．
    /// ただし，skipが設定されたときはi番目とi+skip番目をつなぎます．
    /// </summary>
    private void AddAggregate(List<Vector2> points, int skip = 1)
    {
        int len = points.Count;
        for (int i = 0; i < len; i++)
        {
            _lineList.Add(points[i], points[(i + skip) % len]);
            
        }
    }
}
