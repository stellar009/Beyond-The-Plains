using UnityEngine;

namespace BeyondThePlains.Player
{
    public partial class Player
    {
        void PerformLocomotionAnimation(float value)
        {
            m_Animator.SetFloat(_AnimationBlendHash, value, animationSmoothTime, Time.deltaTime);
        }

        void SwitchCamAngle(float value)
        {
            cameraAnimator.SetFloat(_CameraAnimBlendHash, value, cameraAnimationSmoothTime, Time.deltaTime);
        }
        void HandleAnimations()
        {
            m_MovementDir = new Vector3(m_MovementDirection.x, 0f, m_MovementDirection.z);
            isMoving = m_MovementDir.sqrMagnitude > 0.0001f;

            if (!isMoving) state = _PlayerState.idle;
            else if (m_GameInputs.isSprint && !allowOnlyWalk) state = _PlayerState.run;
            else state = _PlayerState.walk;

            switch (state)
            {
                case _PlayerState.idle:
                    if (allowOnlyWalkCamAngle)
                        SwitchCamAngle(1);
                    else
                        SwitchCamAngle(0);
                    PerformLocomotionAnimation(0);
                    break;

                case _PlayerState.walk:
                    SwitchCamAngle(1);
                    speed = walkSpeed;
                    PerformLocomotionAnimation(0.5f);
                    break;

                case _PlayerState.run:
                    SwitchCamAngle(0);
                    speed = runSpeed;
                    PerformLocomotionAnimation(1);
                    break;
            }
        }
    }
}
