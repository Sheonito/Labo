using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class TouchTest : MonoBehaviour
{
    private List<Vector2> clickPositions;
    private bool isTestPlaying;

    private void Start()
    {
        clickPositions = new List<Vector2>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            Vector2 clickPosition = Input.mousePosition;
            clickPositions.Add(clickPosition);
        }
    }

    public void OnClickButton(string buttonName)
    {
        Debug.Log("OnClickButton: " + buttonName);
    }

    public void PlayTest()
    {
        if (isTestPlaying)
            return;

        StartCoroutine(InvokePlayTest());
    }

    private IEnumerator InvokePlayTest()
    {
        if (isTestPlaying)
            yield break;
        
        isTestPlaying = true;
        List<RaycastResult> results = new List<RaycastResult>();
        for (int i = 0; i < clickPositions.Count; i++)
        {
            // 마우스 클릭 위치를 기반으로 이벤트 데이터 생성
            PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
            pointerEventData.position = clickPositions[i];

            // 이벤트 시뮬레이션: 마우스 클릭 이벤트 전달
            EventSystem.current.RaycastAll(pointerEventData, results);
            foreach (var result in results)
            {
                if (result.gameObject.GetComponent<Selectable>() != null)
                {
                    // 버튼을 클릭했다고 시뮬레이션
                    ExecuteEvents.Execute(result.gameObject, pointerEventData, ExecuteEvents.pointerClickHandler);
                    break;
                }
            }

            yield return new WaitForSeconds(1);
        }

        isTestPlaying = false;
    }
}