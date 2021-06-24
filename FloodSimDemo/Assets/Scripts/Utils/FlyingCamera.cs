using UnityEngine;

namespace Assets.Scripts.Utils
{
    public class FlyingCamera : MonoBehaviour
    {
        public float MainSpeed = 100.0f; //regular speed
        public float ShiftAdd = 250.0f; //multiplied by how long shift is held.  Basically running
        public float MaxShift = 1000.0f; //Maximum speed when holding shift
        public float MouseSensitivity = 0.25f; //How sensitive it with mouse
        public bool RotateOnlyIfMousedown = true;

        public GameObject scaleRulers;
        private bool isScaleVisible = false;

        private Vector3 _lastMousePosition = new Vector3(255, 255, 255); //kind of in the middle of the screen, rather than at the top (play)
        private float _totalRun = 1.0f;

        

        public bool is2D = false;
        private void Start()
        {
           
        }
        void Update()
        {
           
            if (Input.GetMouseButtonDown(1))
            {
                _lastMousePosition = Input.mousePosition; 
            }
            if (Input.GetKeyDown(KeyCode.Y))
            {
                if(isScaleVisible == false)
                {
                    transform.position = new Vector3(3153.149f, 4598.318f, 4672.691f);
                    transform.eulerAngles = new Vector3(95.74799f, 188.75f, 180f);
                    scaleRulers.SetActive(true);
                    isScaleVisible = true;
                }
                else
                {
                    isScaleVisible = false;
                    scaleRulers.SetActive(false);
                    transform.position = new Vector3(2064.1f, 189.45f, 5789.5f);
                    transform.eulerAngles = new Vector3(17.055f, 143.32f, 0f);
                }
            }

            if (!RotateOnlyIfMousedown || (RotateOnlyIfMousedown && Input.GetMouseButton(1)))
            {
                var mouseDelta = Input.mousePosition - _lastMousePosition;
                var rotationDelta = new Vector3(-mouseDelta.y * MouseSensitivity, mouseDelta.x * MouseSensitivity, 0);
                transform.eulerAngles = new Vector3(
                    FlyingCamera.ClampAngle(transform.eulerAngles.x + rotationDelta.x, -89, 89), 
                    transform.eulerAngles.y + rotationDelta.y, 0);
                _lastMousePosition = Input.mousePosition;
            }

            
            var p = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

            if(is2D)
                p = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"),0);


            if (Input.GetKey(KeyCode.LeftShift))
            {
                _totalRun += Time.deltaTime;
                p = p * _totalRun * ShiftAdd;
                p.x = Mathf.Clamp(p.x, -MaxShift, MaxShift);
                p.y = Mathf.Clamp(p.y, -MaxShift, MaxShift);
                p.z = Mathf.Clamp(p.z, -MaxShift, MaxShift);
            }
            else
            {
                _totalRun = Mathf.Clamp(_totalRun * 0.5f, 1f, 1000f);
                p = p * MainSpeed;
            }

            transform.Translate(p * Time.deltaTime);


            if (is2D)
            {
                Vector3 t = transform.eulerAngles;
                t.x = 90f;
                transform.eulerAngles = t;
            }
                
        }

        public static float ClampAngle(float angle, float min, float max)
        {
            // accepts e.g. -80f, 80f
            if (angle < 0f)
                angle = 360 + angle;

            if (angle > 180f)
                return Mathf.Max(angle, 360 + min);

            return Mathf.Min(angle, max);
        }
    }
}
