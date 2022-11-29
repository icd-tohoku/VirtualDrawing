using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Valve.VR;

public class VIVEControllerInput : MonoBehaviour
{
    [SerializeField] private RayInputSystem rayInputSystem;
    public EventSystem eventSystem = null;
    public StandaloneInputModule inputModule = null;


    private enum RayMode
    {
        line,
        Sphere,
    }
    [SerializeField] private RayMode rayMode = RayMode.Sphere; 

    private LineRenderer m_LineRenderer = null;
    private float rayDefaultLength = 0.04f;

    private void Awake()
    {
        if (rayMode == RayMode.line)
        {
            m_LineRenderer = GetComponent<LineRenderer>();
        }
            
        if (rayInputSystem == null)
        {           
            Debug.LogError("set rayInput");
        }
    }


    private void Update()
    {
        if(rayMode == RayMode.Sphere)
        {
            UpdateSphereRay();
        }
        else if(rayMode == RayMode.line)
        {
            UpdateLine();
        }


    }

    private void OnDrawGizmos()
    {
        Vector3 startPos = transform.TransformPoint(new Vector3(0, -0.015f, -0.141f));
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(startPos, 0.01f) ;

        Vector3 endPos = startPos + -1 * transform.forward * 0.025f;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(endPos, 0.01f);
    }

//--------------------------------------------------------------------------------------

    private void UpdateSphereRay()
    {
        Vector3 startPos = transform.TransformPoint(new Vector3(0, -0.015f, -0.141f));
        RaycastHit hit = new RaycastHit();
        Ray ray = new Ray(startPos, -1 * transform.forward);

        if (Physics.SphereCast(ray, 0.010f, out hit, 0.025f))
        {
            rayInputSystem.RayInteraction(hit);
        }
        
    }

    /// <summary>
    /// コントローラからRayがぶつかった点の間に線を引く。
    /// </summary>
    private void UpdateLine()
    {
        Vector3 startPos = transform.TransformPoint(new Vector3(0, -0.017f, -0.155f));
        Vector3 endPosition = startPos + (-1 * transform.forward * rayDefaultLength);

        //Rayを飛ばして当たったオブジェクトを取得
        RaycastHit hit = new RaycastHit();
        Ray ray = new Ray(startPos, -1 * transform.forward);

        if (Physics.Raycast(ray, out hit, rayDefaultLength))
        {
            //テスト用
            //endPosition = startPos + (-1 * transform.forward * GetCanvasDistance());

            endPosition = hit.point;
            rayInputSystem.RayInteraction(hit);
        }

        //Set LineRenderer
        //コントローラの位置からendPointまで線を引く
        m_LineRenderer.SetPosition(0, startPos);
        m_LineRenderer.SetPosition(1, endPosition);    
    }

    private float GetCanvasDistance()
    {
        //get data
        PointerEventData eventData = new PointerEventData(eventSystem);
        eventData.position = inputModule.inputOverride.mousePosition;

        //Raycastusing data
        List<RaycastResult> results = new List<RaycastResult>();
        eventSystem.RaycastAll(eventData, results);

        //get closest
        RaycastResult closestResult = FindFirstRaycast(results);
        float distance = closestResult.distance;

        //clamp
        distance = Mathf.Clamp(distance, 0.0f, rayDefaultLength);
        return distance;
    }

    private RaycastResult FindFirstRaycast(List<RaycastResult> results)
    {
        foreach(RaycastResult result in results)
        {
            if (!result.gameObject)
                continue;

            return result;
        }

        return new RaycastResult();
    }
}
