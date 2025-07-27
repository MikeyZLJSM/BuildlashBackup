using UnityEngine;

// 相机对准一个物体进行旋转和缩放
namespace Controllers
{
    public class CameraScript : MonoBehaviour
    {
        public Transform cenObj => ModulesManager.Instance.GetCenterModule().transform;//围绕的物体

        [Header("旋转速度")] public float rotationSpeed = 5f; //旋转速度

        [Header("鼠标滚轮缩放速度")] public float moveSpeed = 1f; //前后移动速度

        private Vector3 _rotionTransform;
        public Camera camera { get; private set; }
        public static CameraScript Instance { get; private set; }
        public void Awake()
        {
            if (Instance is not null) return;
            Instance = this;
        }
        private void Start()
        {
            camera = GetComponent<Camera>();
            _rotionTransform = cenObj.position;
        }

        private void Update()
        {
            Ctrl_Cam_Move();
            Cam_Ctrl_Rotation();
            transform.LookAt(cenObj);
        }

        //镜头的远离和接近
        public void Ctrl_Cam_Move()
        {
            if (Input.GetAxis("Mouse ScrollWheel") > 0) transform.Translate(Vector3.forward * moveSpeed); //速度可调  自行调整
            if (Input.GetAxis("Mouse ScrollWheel") < 0) transform.Translate(Vector3.forward * -moveSpeed); //速度可调  自行调整
        }

        //摄像机的旋转
        public void Cam_Ctrl_Rotation()
        {
            float mouse_x = Input.GetAxis("Mouse X"); //获取鼠标X轴移动
            float mouse_y = -Input.GetAxis("Mouse Y"); //获取鼠标Y轴移动
            if (Input.GetKey(KeyCode.Mouse1))
            {
                transform.RotateAround(_rotionTransform, Vector3.up, mouse_x * rotationSpeed);
                transform.RotateAround(_rotionTransform, transform.right, mouse_y * rotationSpeed);
            }
        }
    }
}