using System;
using Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

public class FirstPersonController : MonoBehaviour
{
    [SerializeField] protected Camera camera;
    [SerializeField] protected CinemachineVirtualCamera virtualCamera;

    [SerializeField] protected CharacterController characterController;

    [SerializeField] protected BehaviourOptions behaviourOptions = new BehaviourOptions();

    protected Vector3 motion;

    [SerializeField] protected bool isCursorLock = true;
    [SerializeField] protected float axisSpeedX = 500;
    [SerializeField] protected float axisSpeedY = 400;
    [SerializeField] protected bool invertX;
    [SerializeField] protected bool invertY = true;

    private void Start()
    {
        Initialize();
    }

    private void Update()
    {
        ApplyMotion();
    }

    protected virtual void Initialize()
    {
        SetCursorLock(isCursorLock);

        var pov = virtualCamera.GetCinemachineComponent<CinemachinePOV>();
        pov.m_HorizontalAxis.m_MaxSpeed = axisSpeedX;
        pov.m_HorizontalAxis.m_InvertInput = invertX;
        pov.m_VerticalAxis.m_MaxSpeed = axisSpeedY;
        pov.m_VerticalAxis.m_InvertInput = invertY;
    }

    protected virtual void ApplyMotion()
    {
        var motionX = Input.GetAxis("Horizontal") * behaviourOptions.speed * camera.transform.right;
        var motionY = Input.GetAxis("Vertical") * behaviourOptions.speed * camera.transform.forward;
        var nextMotion = motionX + motionY;
        motion = new Vector3(nextMotion.x, motion.y, nextMotion.z);

        var isJump = Input.GetButtonDown("Jump");
        var collisionFlags = characterController.Move(motion);
        if (collisionFlags == CollisionFlags.Below)
        {
            if (isJump)
            {
                motion.y = behaviourOptions.jumpSpeed;
            }
        }
        else
        {
            motion.y -= behaviourOptions.gravity;
        }
    }

    public void SetCursorLock(bool isEnable)
    {
        isCursorLock = isEnable;
        if (isCursorLock)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
        }
    }
}

[Serializable]
public class BehaviourOptions
{
    public float speed = 0.08f;
    public float gravity = 0.0098f;

    public float jumpSpeed = 0.15f;
}
