using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class JoyStun : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    //public jBack;
    RectTransform m_rtBack;
    RectTransform m_rtJoystick;

    Transform Cube;
    float m_Radius;
    float m_Speed = 5.0f;

    Vector3 m_VecMove;
    bool m_bTouch = false;

    void Start()
    {
        m_rtBack = transform.Find("JoystickBack").GetComponent<RectTransform>();
        m_rtJoystick = transform.Find("JoystickBack/Joystick").GetComponent<RectTransform>();

        Cube = GameObject.Find("Cube").transform;

        m_Radius = m_rtBack.rect.width * 0.5f;
    }

    void Update() {
        if (m_bTouch) {
            Cube.position += m_VecMove; // this is what moves cube
        }
    }

    void OnTouch(Vector2 vecTouch) {
        Vector2 vec = new Vector2(vecTouch.x - m_rtBack.position.x, vecTouch.y - m_rtBack.position.y);

        // make sure that vec value does not exceed m_Radius
        vec = Vector2.ClampMagnitude(vec, m_Radius);
        m_rtJoystick.localPosition = vec;

        // move the joystick background to the distance ratio of the joystick
        float fSqr = (m_rtBack.position - m_rtJoystick.position).sqrMagnitude / (m_Radius * m_Radius);

        // normalize touch position
        Vector2 vecNormal = vec.normalized;

        m_VecMove = new Vector3(vecNormal.x * m_Speed * Time.deltaTime * fSqr, 0f, vecNormal.y * m_Speed * Time.deltaTime * fSqr); // this moves cube
        Cube.eulerAngles = new Vector3(0f, Mathf.Atan2(vecNormal.x, vecNormal.y) * Mathf.Rad2Deg, 0f);
    }

    public void OnDrag(PointerEventData eventData) {
        OnTouch(eventData.position);
        m_bTouch = true; //this moves cube
    }

    public void OnPointerDown(PointerEventData eventData) {
        OnTouch(eventData.position);
        m_bTouch = true;//this moves cube
    }

    public void OnPointerUp(PointerEventData eventData) {
        // return to the original position
        m_rtJoystick.localPosition = Vector3.zero;
        m_bTouch = false;//this stops cube
    }
}
