using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TiltFive;

namespace IKProject3
{
    public class JointController : MonoBehaviour
    {
        private GameObject[] joint = new GameObject[3];
        private GameObject[] arm = new GameObject[3];
        private float[] armL = new float[3];
        private Vector3[] angle = new Vector3[3];

        private GameObject TCP;
        private Vector3 screenPoint; 
        private Vector3 offset;
        private Text TCPValueText;
        private Renderer TCPRenderer;
        private Text TCPLabel;

        private bool isGrabbing;

        void Start()
        {
            for (int i = 0; i < joint.Length; i++)
            {
                joint[i] = GameObject.Find("Joint_" + i.ToString());
                arm[i] = GameObject.Find("Arm_" + i.ToString());

                if (joint[i] == null || arm[i] == null)
                {
                    Debug.LogError("Joint or Arm GameObject not found");
                    return;
                }

                if(i == 0) armL[i] = arm[i].transform.localScale.y;
                else armL[i] = arm[i].transform.localScale.x;
            }

            TCP = GameObject.Find("TCP");
            if (TCP == null)
            {
                Debug.LogError("TCP GameObject not found");
                return;
            }

            TCPRenderer = TCP.GetComponent<Renderer>();
            if (TCPRenderer == null)
            {
                Debug.LogError("Renderer component not found in TCP GameObject");
                return;
            }

            TCPValueText = GameObject.Find("TCP_Value")?.GetComponent<Text>();
            TCPLabel = GameObject.Find("TCP_Display_Label")?.GetComponent<Text>();

            if (TCPValueText == null || TCPLabel == null)
            {
                Debug.LogError("Text component not found in TCP_Value or TCP_Display_Label GameObject");
                return;
            }
        }

        void Update()
        {
            if (TiltFive.Input.GetTrigger(TiltFive.Input.ControllerIndex.Wand) > 0.5f)
            {
                if (!isGrabbing)
                {
                    RaycastHit hit;
                    Ray ray = Camera.main.ScreenPointToRay(new Vector3(TiltFive.Input.GetStickAxis(TiltFive.Input.ControllerIndex.Wand).x, TiltFive.Input.GetStickAxis(TiltFive.Input.ControllerIndex.Wand).y, 0));

                    if (Physics.Raycast(ray, out hit))
                    {
                        if (hit.transform.gameObject == TCP)
                        {
                            screenPoint = Camera.main.WorldToScreenPoint(TCP.transform.position);
                            offset = TCP.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(TiltFive.Input.GetStickAxis(TiltFive.Input.ControllerIndex.Wand).x, TiltFive.Input.GetStickAxis(TiltFive.Input.ControllerIndex.Wand).y, screenPoint.z));
                            isGrabbing = true;
                        }
                    }
                }
                else
                {
                    Vector3 curScreenPoint = new Vector3(TiltFive.Input.GetStickAxis(TiltFive.Input.ControllerIndex.Wand).x, TiltFive.Input.GetStickAxis(TiltFive.Input.ControllerIndex.Wand).y, screenPoint.z);
                    Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + offset;
                    TCP.transform.position = curPosition;
                    TCPValueText.text = curPosition.x.ToString("F2") + ", " + curPosition.y.ToString("F2") + ", " + curPosition.z.ToString("F2");
                    ComputeIK(TCP.transform.position);
                }
            }
            else
            {
                isGrabbing = false;
            }
        }

        void ComputeIK(Vector3 pos)
        {
            float x = pos.x;
            float y = pos.y;
            float z = pos.z;

            angle[0].y = -Mathf.Atan2(z, x);
            float a = x / Mathf.Cos(angle[0].y);
            float b = y - armL[0];

            if (Mathf.Pow(a * a + b * b, 0.5f) > (armL[1] + armL[2]))
            {
                TCPRenderer.material.color = Color.red; // Set TCP color to red
                TCPLabel.color = Color.red; // Set label color to red
                TCPValueText.color = Color.red; // Set value color to red
                Debug.Log("TCP is out of bounds"); // Print to the console
            }
            else
            {
                TCPRenderer.material.color = Color.white; // Set TCP color to white
                TCPLabel.color = Color.white; // Set label color to white
                TCPValueText.color = Color.white; // Set value color to white

                float alfa = Mathf.Acos((armL[1] * armL[1] + armL[2] * armL[2] - a * a - b * b) / (2f * armL[1] * armL[2]));
                angle[2].z = -Mathf.PI + alfa;
                float beta = Mathf.Acos((armL[1] * armL[1] + a * a + b * b - armL[2] * armL[2]) / (2f * armL[1] * Mathf.Pow((a * a + b * b), 0.5f)));
                angle[1].z = Mathf.Atan2(b, a) + beta;

                for (int i = 0; i < joint.Length; i++)
                {
                    joint[i].transform.localEulerAngles = angle[i] * Mathf.Rad2Deg;
                }
            }
        }
    }
}
