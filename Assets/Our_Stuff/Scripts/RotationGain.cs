// Position Rewind|Presence|70070
namespace VRTK
{
    using UnityEngine;
   

    /// <summary>
    /// Event Payload
    /// </summary>
    /// <param name="collidedPosition">The position of the play area when it collded.</param>
    /// <param name="resetPosition">The position of the play area when it has been rewinded to a safe position.</param>
    public struct RotationGainEventArgs
    {
       
        public Vector3 collidedPosition;
        public Vector3 resetPosition;
    }

    /// <summary>
    /// Event Payload
    /// </summary>
    /// <param name="sender">this object</param>
    /// <param name="e"><see cref="PositionRewindEventArgs"/></param>
    public delegate void RotationGainEventHandler(object sender, PositionRewindEventArgs e);

    /// <summary>
    /// Attempts to rewind the position of the play area to a last know valid position upon the headset collision event.
    /// </summary>
    /// <remarks>
    /// **Required Components:**
    ///  * `VRTK_BodyPhysics` - A Body Physics script to manage the collisions of the body presence within the scene.
    ///  * `VRTK_HeadsetCollision` - A Headset Collision script to determine when the headset is colliding with valid geometry.
    ///
    /// **Script Usage:**
    ///  * Place the `VRTK_PositionRewind` script on any active scene GameObject.
    /// </remarks>
    /// <example>
    /// `VRTK/Examples/017_CameraRig_TouchpadWalking` has the position rewind script to reset the user's position if they walk into objects.
    /// </example>
    [AddComponentMenu("VRTK/Scripts/Presence/RotationGain")]
    public class RotationGain : MonoBehaviour
    {
        /// <summary>
        /// Emitted when the draggable item is successfully dropped.
        /// </summary>
        public Transform cameraRig;
        public Transform world;

        protected Vector3 oldRotation;
        OVRPose headPose;

        private void Start()
        {
 
        }

        private void Update()
        {
            Vector3 newRotation = cameraRig.rotation.eulerAngles;
            if (newRotation.y != oldRotation.y)
                world.RotateAround(new Vector3(cameraRig.position.x, 0, cameraRig.position.z), Vector3.up, (-(newRotation.y - oldRotation.y)*0.5f));
            oldRotation = newRotation;
        }

    }   
}