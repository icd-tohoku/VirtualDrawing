using MathNet.Numerics.Statistics;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 描いた線から平均誤差を計算する
/// </summary>
public static class MeanDeviation
{
    //最後はfloatを返したいかも
    public static double CalcMeanDeviation(List<Vector3> dots, string modelName)
    {
        //エッジを処理
        dots.RemoveRange(dots.Count - 10, 10);
        dots.RemoveRange(0, 10);

        //ストロークを時間をもとにリストを分割
        List<List<Vector2>> strokeList = DivideDos(dots);

        //デバッグ...リストが正しく分割されているか。
        /*for (int i = 0; i < strokeList.Count; i++)
        {
            string filePath = Application.dataPath + $@"/Data/Drawing/Debug/1_listDivided{i}.png";
            SaveAsPng(strokeList[i], filePath);
        }*/

        //ノイズを除去
        for (int i = 0; i < strokeList.Count; i++)
        {
            var tempList = MedianFilter(strokeList[i], 6);
            strokeList[i].Clear();
            strokeList[i].AddRange(tempList);
        }

        //デバッグ...ノイズ除去ができているか
        /*for (int i = 0; i < strokeList.Count; i++)
        {
            string filePath = Application.dataPath + $@"/Data/Drawing/Debug/2_Filterd{i}.png";
            SaveAsPng(strokeList[i], filePath);
        }*/

        //100点のサンプリングするためのサンプリング長さを求める。
        float allStrokeLength = 0;
        for (int i = 0; i < strokeList.Count; i++)
        {
            allStrokeLength += CalcStrokeLength(strokeList[i]);
        }
        float samplingDis = allStrokeLength / 100f;


        //線形にリサンプリングをする
        List<Vector2> resampledDots = new List<Vector2>();
        for (int i = 0; i < strokeList.Count; i++)
        {
            resampledDots.AddRange(LinearResampling(strokeList[i], samplingDis));
        }

        //デバッグ...リサンプリングができているか
        /*string filePath_Resample = Application.dataPath + $@"/Data/Drawing/Debug/3_Resampled.png";
        SaveAsPng(resampledDots, filePath_Resample);*/

        List<string[]> modelInfo = GetModelInfo(modelName);
        Vector2 startPos = new Vector2(float.Parse(modelInfo[0][1]), float.Parse(modelInfo[0][2]));

        //スタート位置の調整
        var temp = MuchStartDot(resampledDots, startPos);
        resampledDots.Clear();
        resampledDots.AddRange(temp);

        //デバッグ...スタート位置が変更されたか
        /*string filePath_StartAjust = Application.dataPath + $@"/Data/Drawing/Debug/4_StartAjust.png";
        SaveAsPng(resampledDots, filePath_StartAjust);*/


        //全ての点の誤差を平均する
        float[] deviations = new float[resampledDots.Count];
        for (int i = 0; i < resampledDots.Count; i++)       // スタート位置は平均に含めない方がいいか ？
        {
            deviations[i] = CalcDeviation(resampledDots[i], modelInfo);
            //Debug.Log($"誤差{i}: {deviations[i]}");
        }

        var meanDeviation = deviations.Mean();
        return meanDeviation;
    }

    //-----------------------------------------------------------------------
    /// <summary>
    /// 時間を参考にストロークを複数に分ける
    /// </summary>
    /// <param name="dots"></param>
    /// <returns></returns>
    private static List<List<Vector2>> DivideDos(List<Vector3> dots)
    {
        //timeDuration秒以上差は別ストロークとみなす。
        float timeDuration = 0.5f;

        List<List<Vector2>> strokeList = new List<List<Vector2>>();
        strokeList.Add(new List<Vector2>());
        strokeList[0].Add(new Vector2(dots[0].x, dots[0].y));

        //第三要素の時間を参考に、前の要素よりある程度時間が離れているところでListを区切る
        int j = 0;
        for (int i = 1; i < dots.Count; i++)
        {
            if (dots[i].z - dots[i-1].z > timeDuration)
            {               
                if (strokeList[j].Count < 3)
                {
                    //前のstrokeが短過ぎたらそのストロークを消す
                    strokeList.Remove(strokeList[j]);
                }
                else
                {
                    //前のストロークが十分に長ければ、次のストロークを作成する。
                    j++;               
                }
                strokeList.Add(new List<Vector2>());
            }

            strokeList[j].Add(new Vector2(dots[i].x, dots[i].y));
        }
        return strokeList;
    }

    /// <summary>
    /// ストロークをリサンプリングする
    /// </summary>
    /// <param name="dots"></param>
    /// <param name="samplingIntervalDis"></param>
    /// <returns></returns>
    private static List<Vector2> LinearResampling(List<Vector2> dots, float samplingIntervalDis)
    {
        float[] strokeLength = new float[dots.Count];
        strokeLength[0] = 0;

        for(int i = 1; i < dots.Count; i++)
        {
            strokeLength[i] = strokeLength[i - 1] + Vector2.Distance(dots[i], dots[i - 1]);
        }

        int samplingPointNum = Mathf.CeilToInt(strokeLength[strokeLength.Length - 1] / samplingIntervalDis);

        List<Vector2> resampledList = new List<Vector2>();
        resampledList.Add(dots[0]);

        int j = 1;
        for(int p = 1; p <= samplingPointNum - 2; p++)
        {
            while(strokeLength[j] < p * samplingIntervalDis)
            {
                j++;
            }

            var C = (p * samplingIntervalDis - strokeLength[j - 1]) / (strokeLength[j] - strokeLength[j - 1]);
            var newDot = dots[j - 1] + (dots[j] - dots[j - 1]) * C;
            resampledList.Add(newDot);
        }

        resampledList.Add(dots[dots.Count - 1]);

        return resampledList;
    }

    /// <summary>
    /// 全体のストロークの長さを計算する
    /// </summary>
    /// <param name="dots"></param>
    /// <returns></returns>
    private static float CalcStrokeLength(List<Vector2> dots)
    {
        float length = 0;
        for (int i = 1; i< dots.Count; i++)
        {
            length += Vector2.Distance(dots[i], dots[i - 1]);
        }

        return length;
    }

    /// <summary>
    /// 各点をwindowSizeで中央値フィルタリングする
    /// </summary>
    /// <param name="dots"></param>
    /// <param name="windowSize"></param>
    /// <returns></returns>
    private static List<Vector2> MedianFilter(List<Vector2> dots, int windowSize)
    {
        List<Vector2> filteredList = new List<Vector2>();

        List<float> tempXList = new List<float>();
        List<float> tempYList = new List<float>();

        int i = 0;
        while (i < dots.Count)
        {
            //各要素をtempリストに格納
            tempXList.Add(dots[i].x);
            tempYList.Add(dots[i].y);

            //windowSize個の要素が集まったら、またはdotsの最後まで来たら、それぞれの要素の中央値をとる
            if (tempXList.Count >= windowSize || i == dots.Count - 1)
            {
                var x = tempXList.Median();
                var y = tempYList.Median();

                filteredList.Add(new Vector2(x, y));

                tempXList.RemoveAt(0);
                tempYList.RemoveAt(0);
            }
            i++;
        }

        return filteredList;
    }

    /// <summary>
    /// お手本のスタート位置と入力文字のスタート位置を合わせる
    /// </summary>
    /// <param name="dots"></param>
    /// <param name="startVec"></param>
    /// <returns></returns>
    private static List<Vector2> MuchStartDot(List<Vector2> dots, Vector2 startVec)
    {
        List<Vector2> muchedDots = new List<Vector2>();

        var startOffset = startVec - dots[0];

        for(int i = 0; i < dots.Count; ++i)
        {
            var x = dots[i].x + startOffset.x;
            var y = dots[i].y + startOffset.y;
            muchedDots.Add(new Vector2(x, y));
        }
        return muchedDots;
    }

    /// <summary>
    /// お手本の情報(.csvのリスト)を取得
    /// </summary>
    /// <param name="modelName"></param>
    /// <returns></returns>
    private static List<string[]> GetModelInfo(string modelName)
    {
        string csvFilePath = Application.dataPath + $@"/Resources/Models/{modelName}.csv";

        List<string[]> modelInfo = CSVManager.Read(csvFilePath);

        return modelInfo;
    }

    private static float CalcDeviation(Vector2 dot, List<string[]> modelInfo)
    {
        float deviation = float.MaxValue;

        for(int i = 1; i < modelInfo.Count; i++)
        {
            if(modelInfo[i][0] == "NotVertical")
            {
                //y = ax + b のaとbの値を取得
                var a = float.Parse(modelInfo[i][1]);
                var b = float.Parse(modelInfo[i][2]);

                //点と直線の距離
                //直線 ax - y + b = 0 と点(dot.x, dot.y)の距離を測定
                var distance = Mathf.Abs(a * dot.x - dot.y + b) / Mathf.Sqrt(a * a + 1f);

                deviation = Mathf.Min(deviation, distance);
            }
            else if(modelInfo[i][0] == "Vertical")
            {
                var modelx = float.Parse(modelInfo[i][1]);
                deviation = Mathf.Min(deviation, Mathf.Abs(modelx - dot.x));
            }
            else
            {
                throw new FormatException();
            }
        }

        return deviation;
    }

    //-----------------------------データの読み込み・書き込み------------------------------------------

    private static void SaveAsCSV(List<Vector2> dots, string filePath)
    {
        if (!filePath.EndsWith(".csv")) filePath += ".csv";

        string data = "x,y\n";

        foreach(Vector2 dot in dots)
        {
            data += $"{dot.x},{dot.y}\n";
        }

        CSVManager.Write(filePath, data, FileMode.Append);

        Debug.Log($"{filePath}にストロークデータを保存しました");
    }

    private static void SaveAsPng(List<Vector2> dots, string pngFilePath, int textureSize = 1024)
    {
        //dotsの点群の位置に黒く点を打ったpngファイルを生成する。
        Texture2D texture2D = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        texture2D.filterMode = FilterMode.Point;
        Color[] pixels = texture2D.GetPixels();
        Color[] buffer = new Color[pixels.Length];
        pixels.CopyTo(buffer, 0);

        foreach(Vector2 dot in dots)
        {
            buffer.SetValue(Color.black, (int)dot.x + textureSize * (int)dot.y);
        }

        texture2D.SetPixels(buffer);
        texture2D.Apply();

        var bytes = texture2D.EncodeToPNG();

        if (!pngFilePath.EndsWith(".png")) pngFilePath += ".png";

        //画像ファイルを保存する
        File.WriteAllBytes(pngFilePath, bytes);
        Debug.Log($"{pngFilePath}にストロークデータを保存しました");


        // 最後にRefresh
        AssetDatabase.Refresh();
    }

}
