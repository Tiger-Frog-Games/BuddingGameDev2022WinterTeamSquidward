using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Micosmo.SensorToolkit {

    public enum LocomotionMode { None, RigidBodyFlying, RigidBodyCharacter, UnityCharacterController }
    public enum LocomotionMode2D { None, RigidBody2D }

    [System.Serializable]
    public class LocomotionSystem {
        public float SpringForce = 3f;
        public float DampForce = 2 * Mathf.Sqrt(3f);

        public bool ConstrainMotion;
        public float MaxForwardSpeed;
        public float MaxStrafeSpeed;
        public float MaxTurnSpeedDegrees;
        public float MaxAccel;

        public float MaxSpeedMultiplier = 1f;

        public LocomotionStrafeSettings Strafing;

        float maxAngularAccel => MaxAccel * 1000f;
        float attenMaxSpeed => MaxForwardSpeed * MaxSpeedMultiplier;
        float attenMaxStrafeSpeed => MaxStrafeSpeed * MaxSpeedMultiplier;
        float attenMaxTurnSpeed => MaxTurnSpeedDegrees * MaxSpeedMultiplier;

        Vector3 kinematicVelocity;
        Vector3 kinematicAngularVelocity; // Should be degrees/second

        Vector2 kinematicVelocity2D;
        float kinematicAngularVelocity2D; // Should be degrees/second

        public void FlyableSeek(Rigidbody rb, Vector3 pos) {
            var dir = Strafing.GetFaceTarget(rb.transform, pos);
            if (rb.isKinematic) {
                FlyableSeekKinematic(rb, pos, dir, Vector3.up);
            } else {
                FlyableSeekWithForces(rb, pos, dir, Vector3.up);
            }
        }

        public void CharacterSeek(Rigidbody rb, Vector3 pos, Vector3 up) {
            var dir = Vector3.ProjectOnPlane(Strafing.GetFaceTarget(rb.transform, pos), up).normalized;
            if (rb.isKinematic) {
                CharacterSeekKinematic(rb, pos, dir, up);
            } else {
                CharacterSeekWithForces(rb, pos, dir, up);
            }
        }

        public void RigidBody2DSeek(Rigidbody2D rb, Vector2 pos) {
            var dir = Vector3.ProjectOnPlane(Strafing.GetFaceTarget(rb.transform, pos), Vector3.back).normalized;
            if (rb.isKinematic) {
                RigidBody2DSeekKinematic(rb, pos, dir);
            } else {
                RigidBody2DSeekWithForces(rb, pos, dir);
            }
        }

        public void CharacterSeek(CharacterController cc, Vector3 pos, Vector3 up) {
            var dir = Vector3.ProjectOnPlane(Strafing.GetFaceTarget(cc.transform, pos), up).normalized;
            CharacterControllerSeek(cc, pos, dir, up);
        }

        void FlyableSeekWithForces(Rigidbody rb, Vector3 tpos, Vector3 tdir, Vector3 tup) {
            var angularAccel = MotionUtils.SeekAngularAccel(SpringForce, DampForce, new ReferenceFrame(rb.transform), Mathf.Rad2Deg * rb.angularVelocity, tdir, tup);
            var transAccel = MotionUtils.SeekAccel(SpringForce, DampForce, rb.position - tpos, rb.velocity);
            AccelerateForces(rb, angularAccel, transAccel);
        }

        void FlyableSeekKinematic(Rigidbody rb, Vector3 tpos, Vector3 tdir, Vector3 tup) {
            var angularAccel = MotionUtils.SeekAngularAccel(SpringForce, DampForce, new ReferenceFrame(rb.transform), rb.rotation*kinematicAngularVelocity, tdir, tup);
            var transAccel = MotionUtils.SeekAccel(SpringForce, DampForce, rb.position - tpos, kinematicVelocity);
            AccelerateKinematic(rb, angularAccel, transAccel);
        }

        void CharacterSeekWithForces(Rigidbody rb, Vector3 tpos, Vector3 tdir, Vector3 tup) {
            var angularAccel = MotionUtils.SeekPlanarAngularAccel(SpringForce, DampForce, new ReferenceFrame(rb.transform), Mathf.Rad2Deg * rb.angularVelocity, tdir, tup);
            var transAccel = MotionUtils.SeekAccel(SpringForce, DampForce, rb.position - tpos, rb.velocity);
            AccelerateForces(rb, angularAccel, transAccel);
        }

        void CharacterSeekKinematic(Rigidbody rb, Vector3 tpos, Vector3 tdir, Vector3 tup) {
            var angularAccel = MotionUtils.SeekPlanarAngularAccel(SpringForce, DampForce, new ReferenceFrame(rb.transform), rb.rotation * kinematicAngularVelocity, tdir, tup);
            var transAccel = MotionUtils.SeekAccel(SpringForce, DampForce, rb.position - tpos, kinematicVelocity);
            AccelerateKinematic(rb, angularAccel, transAccel);
        }

        void CharacterControllerSeek(CharacterController cc, Vector3 tpos, Vector3 tdir, Vector3 tup) {
            var angularAccel = MotionUtils.SeekPlanarAngularAccel(SpringForce, DampForce, new ReferenceFrame(cc.transform), cc.transform.rotation * kinematicAngularVelocity, tdir, tup);
            var transAccel = MotionUtils.SeekAccel(SpringForce, DampForce, cc.transform.position - tpos, kinematicVelocity);
            AccelerateCharacterController(cc, angularAccel, transAccel);
        }

        void RigidBody2DSeekWithForces(Rigidbody2D rb, Vector2 tpos, Vector2 tdir) {
            var angularAccel = MotionUtils.SeekAngularAccel2D(SpringForce, DampForce, rb.transform.up, rb.angularVelocity, tdir);
            var transAccel = MotionUtils.SeekAccel(SpringForce, DampForce, rb.position - tpos, rb.velocity);
            AccelerateForces(rb, angularAccel, transAccel);
        }

        void RigidBody2DSeekKinematic(Rigidbody2D rb, Vector2 tpos, Vector2 tdir) {
            var angularAccel = MotionUtils.SeekAngularAccel2D(SpringForce, DampForce, rb.transform.up, kinematicAngularVelocity2D, tdir);
            var transAccel = MotionUtils.SeekAccel(SpringForce, DampForce, rb.position - tpos, kinematicVelocity2D);
            AccelerateKinematic(rb, angularAccel, transAccel);
        }

        void AccelerateForces(Rigidbody rb, Vector3 angularAccel, Vector3 transAccel) {
            if (ConstrainMotion) {
                angularAccel = Vector3.ClampMagnitude(angularAccel, maxAngularAccel);
                transAccel = Vector3.ClampMagnitude(transAccel, MaxAccel);
            }
            rb.AddRelativeTorque(Mathf.Deg2Rad * angularAccel * rb.mass);
            rb.AddForce(transAccel * rb.mass);

            if (ConstrainMotion) {
                rb.angularVelocity = Vector3.ClampMagnitude(rb.angularVelocity, attenMaxTurnSpeed);

                var vel = rb.velocity;
                var dirDotForward = Vector3.Dot(vel.normalized, rb.transform.forward);
                var maxVel = Mathf.Abs(dirDotForward) * attenMaxSpeed + (1f - Mathf.Abs(dirDotForward)) * attenMaxStrafeSpeed;
                rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxVel);
            }
        }

        void AccelerateKinematic(Rigidbody rb, Vector3 angularAccel, Vector3 transAccel) {
            if (ConstrainMotion) {
                angularAccel = Vector3.ClampMagnitude(angularAccel, maxAngularAccel);
                transAccel = Vector3.ClampMagnitude(transAccel, MaxAccel);
            }
            kinematicAngularVelocity += angularAccel * Time.fixedDeltaTime;
            kinematicVelocity += transAccel * Time.fixedDeltaTime;

            if (ConstrainMotion) {
                kinematicAngularVelocity = Vector3.ClampMagnitude(kinematicAngularVelocity, attenMaxTurnSpeed);

                var dirDotForward = Vector3.Dot(kinematicVelocity.normalized, rb.transform.forward);
                var maxVel = Mathf.Abs(dirDotForward) * attenMaxSpeed + (1f - Mathf.Abs(dirDotForward)) * attenMaxStrafeSpeed;
                kinematicVelocity = Vector3.ClampMagnitude(kinematicVelocity, maxVel);
            }

            rb.rotation = rb.rotation * Quaternion.AngleAxis(kinematicAngularVelocity.magnitude * Time.fixedDeltaTime, kinematicAngularVelocity.normalized);
            rb.position = rb.position + kinematicVelocity * Time.fixedDeltaTime;
        }

        void AccelerateCharacterController(CharacterController cc, Vector3 angularAccel, Vector3 transAccel) {
            if (ConstrainMotion) {
                angularAccel = Vector3.ClampMagnitude(angularAccel, maxAngularAccel);
                transAccel = Vector3.ClampMagnitude(transAccel, MaxAccel);
            }
            kinematicAngularVelocity += angularAccel * Time.deltaTime;
            kinematicVelocity = cc.velocity + (transAccel * Time.deltaTime);

            if (ConstrainMotion) {
                kinematicAngularVelocity = Vector3.ClampMagnitude(kinematicAngularVelocity, attenMaxTurnSpeed);

                var dirDotForward = Vector3.Dot(kinematicVelocity.normalized, cc.transform.forward);
                var maxVel = Mathf.Abs(dirDotForward) * attenMaxSpeed + (1f - Mathf.Abs(dirDotForward)) * attenMaxStrafeSpeed;
                kinematicVelocity = Vector3.ClampMagnitude(kinematicVelocity, maxVel);
            }

            cc.transform.rotation = cc.transform.rotation * Quaternion.AngleAxis(kinematicAngularVelocity.magnitude * Time.deltaTime, kinematicAngularVelocity.normalized);
            cc.SimpleMove(kinematicVelocity);
        }

        void AccelerateForces(Rigidbody2D rb, float angularAccel, Vector2 transAccel) {
            if (ConstrainMotion) {
                angularAccel = Mathf.Clamp(angularAccel, -maxAngularAccel, maxAngularAccel);
                transAccel = Vector3.ClampMagnitude(transAccel, MaxAccel);
            }
            rb.AddTorque(angularAccel * rb.mass);
            rb.AddForce(transAccel * rb.mass);

            if (ConstrainMotion) {
                rb.angularVelocity = Mathf.Clamp(rb.angularVelocity, -attenMaxTurnSpeed, attenMaxTurnSpeed);

                var vel = rb.velocity;
                var dirDotForward = Vector3.Dot(vel.normalized, rb.transform.up);
                var maxVel = Mathf.Abs(dirDotForward) * attenMaxSpeed + (1f - Mathf.Abs(dirDotForward)) * attenMaxStrafeSpeed;
                rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxVel);
            }
        }

        void AccelerateKinematic(Rigidbody2D rb, float angularAccel, Vector2 transAccel) {
            if (ConstrainMotion) {
                angularAccel = Mathf.Clamp(angularAccel, -maxAngularAccel, maxAngularAccel);
                transAccel = Vector3.ClampMagnitude(transAccel, MaxAccel);
            }
            kinematicAngularVelocity2D += angularAccel * Time.fixedDeltaTime;
            kinematicVelocity2D += transAccel * Time.fixedDeltaTime;

            if (ConstrainMotion) {
                kinematicAngularVelocity2D = Mathf.Clamp(kinematicAngularVelocity2D, -attenMaxTurnSpeed, attenMaxTurnSpeed);

                var dirDotForward = Vector2.Dot(kinematicVelocity2D.normalized, rb.transform.up);
                var maxVel = Mathf.Abs(dirDotForward) * attenMaxSpeed + (1f - Mathf.Abs(dirDotForward)) * attenMaxStrafeSpeed;
                kinematicVelocity2D = Vector3.ClampMagnitude(kinematicVelocity2D, maxVel);
            }

            rb.rotation = rb.rotation + kinematicAngularVelocity2D * Time.fixedDeltaTime;
            rb.position = rb.position + kinematicVelocity2D * Time.fixedDeltaTime;
        }
    }

    [System.Serializable]
    public struct LocomotionStrafeSettings {
        [SerializeField]
        Transform targetTransform;
        [SerializeField]
        Vector3 targetDirection;

        public void SetFaceTarget(Vector3 direction) {
            targetTransform = null;
            targetDirection = direction;
        }

        public void SetFaceTarget(Transform target) {
            targetTransform = target;
            targetDirection = Vector3.zero;
        }

        public void Clear() {
            targetTransform = null;
            targetDirection = Vector3.zero;
        }

        public Vector3 GetFaceTarget(Transform forTransform, Vector3 seekPos) {
            if (targetTransform != null) {
                return (targetTransform.position - forTransform.position).normalized;
            } else if (targetDirection != Vector3.zero) {
                return targetDirection.normalized;
            }
            var toSeek = (seekPos - forTransform.position).normalized;
            if (toSeek != Vector3.zero) {
                return toSeek;
            }
            return forTransform.forward;
        }
    }
}