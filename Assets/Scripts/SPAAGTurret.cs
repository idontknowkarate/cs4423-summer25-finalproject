using UnityEngine;

public class SPAAGTurret : MonoBehaviour
{
   public Transform player;

   [Header("Turret Parts (Pivots)")]
   public Transform turretBasePivot;  // yaw
   public Transform cannonPivot;      // pitch
   public Transform radarDishPivot;   // spins constantly

   [Header("Rotation Settings")]
   public float rotateSpeed = 50f;
   public float elevationSpeed = 50f;
   public float radarSpinSpeed = 180f;

   void Update()
   {
      if (!player) return;

      // --- yaw: rotate base to face player on Y axis only ---
      Vector3 targetDirection = player.position - turretBasePivot.position;
      Vector3 flatDirection = new Vector3(targetDirection.x, 0, targetDirection.z);

      if (flatDirection != Vector3.zero)
      {
         Quaternion targetRotation = Quaternion.LookRotation(flatDirection) * Quaternion.Euler(0f, 180f, 0f);
         turretBasePivot.rotation = Quaternion.RotateTowards(
            turretBasePivot.rotation,
            targetRotation,
            rotateSpeed * Time.deltaTime
         );
      }

      // --- pitch: rotate cannons (cannonPivot) toward player on X axis only ---
      Vector3 localTargetPos = cannonPivot.InverseTransformPoint(player.position);
      float targetPitch = Mathf.Atan2(localTargetPos.y, localTargetPos.z) * Mathf.Rad2Deg;
      float currentPitch = cannonPivot.localEulerAngles.x;

      // handle wraparound
      if (currentPitch > 180) currentPitch -= 360;

      float newPitch = Mathf.MoveTowards(currentPitch, targetPitch, elevationSpeed * Time.deltaTime);
      cannonPivot.localEulerAngles = new Vector3(newPitch, 0f, 0f);

      // --- radar dish: spin continuously around Y axis ---
      radarDishPivot.Rotate(Vector3.up * radarSpinSpeed * Time.deltaTime, Space.Self);
   }
}
