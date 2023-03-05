using UnityEngine;
using ThunderRoad;

namespace Flips
{
    public class FlipSpell : SpellCastCharge
    {
        Locomotion locomotion;
        Creature creature;
        Player player;
        bool isFrontflip;
        bool isBackflip;
        bool isRightflip;
        bool isLeftflip;
        public float DegreesPerSecond = 360;
        public static bool allowMove;
        public static bool turnBodyByHeadAndHands;
        public static bool isStored = false;
        public static Transform headTransform;
        public override void Load(SpellCaster spellCaster, Level level)
        {
            base.Load(spellCaster, level);
            creature = spellCaster.mana.creature;
            player = creature.player;
            locomotion = player.locomotion;
            locomotion.OnGroundEvent += Locomotion_OnGroundEvent; 
            if (!isStored)
            {
                turnBodyByHeadAndHands = creature.ragdoll.ik.turnBodyByHeadAndHands;
                allowMove = locomotion.allowMove;
                isStored = true;
            }
        }

        private void Locomotion_OnGroundEvent(Vector3 groundPoint, Vector3 velocity, Collider groundCollider)
        {
            if (player.transform.rotation.x != 0)
            {
                player.autoAlign = true;
                creature.ragdoll.ik.turnBodyByHeadAndHands = turnBodyByHeadAndHands;
                locomotion.allowMove = allowMove;
            }
        }

        public override void Unload()
        {
            base.Unload();
            if (player.transform.rotation.x != 0 && spellCaster.other.spellInstance?.GetType() != GetType())
            {
                player.autoAlign = true;
                creature.ragdoll.ik.turnBodyByHeadAndHands = turnBodyByHeadAndHands;
                locomotion.allowMove = allowMove;
            }
            locomotion.OnGroundEvent -= Locomotion_OnGroundEvent;
        }
        public override void Fire(bool active)
        {
            base.Fire(active);
            if (active)
            {
                if (Vector3.Angle(locomotion.moveDirection, -creature.transform.forward) < 45)
                {
                    isBackflip = true;
                    isFrontflip = false;
                    isLeftflip = false;
                    isRightflip = false;
                }
                else if (Vector3.Angle(locomotion.moveDirection, creature.transform.right) < 45)
                {
                    isRightflip = true;
                    isBackflip = false;
                    isFrontflip = false;
                    isLeftflip = false;
                }
                else if (Vector3.Angle(locomotion.moveDirection, -creature.transform.right) < 45)
                {
                    isLeftflip = true;
                    isBackflip = false;
                    isFrontflip = false;
                    isRightflip = false;
                }
                else
                {
                    isFrontflip = true;
                    isBackflip = false;
                    isLeftflip = false;
                    isRightflip = false;
                }
            }
            else
            {
                isBackflip = false;
                isFrontflip = false;
                isLeftflip = false;
                isRightflip = false;
            }
        }
        public override void UpdateCaster()
        {
            base.UpdateCaster();
            if (!locomotion.isGrounded && spellCaster.isFiring)
            {
                Quaternion rotation1 = player.transform.rotation;
                headTransform = player.head.cam.transform;
                headTransform.rotation.Set(0, player.head.cam.transform.rotation.y, 0, 0);
                if (isFrontflip || isBackflip)
                    player.transform.RotateAround(creature.ragdoll.targetPart.transform.position, headTransform.right, (isFrontflip ? DegreesPerSecond : -DegreesPerSecond) * Time.deltaTime * spellCaster.fireAxis);
                else if (isRightflip || isLeftflip)
                    player.transform.RotateAround(creature.ragdoll.targetPart.transform.position, headTransform.forward, (isLeftflip ? DegreesPerSecond : -DegreesPerSecond) * Time.deltaTime * spellCaster.fireAxis);
                player.autoAlign = false;
                creature.ragdoll.ik.turnBodyByHeadAndHands = false;
                creature.ragdoll.ik.AddLocomotionDeltaRotation(player.transform.rotation * Quaternion.Inverse(rotation1), creature.ragdoll.targetPart.transform.position);
            }
            if (WheelMenuSpell.left != null && WheelMenuSpell.left.isShown)
                WheelMenuSpell.left.transform.LookAt(2f * WheelMenuSpell.left.transform.position - player.head.cam.transform.position, player.transform.up);
            if (WheelMenuSpell.right != null && WheelMenuSpell.right.isShown)
                WheelMenuSpell.right.transform.LookAt(2f * WheelMenuSpell.right.transform.position - player.head.cam.transform.position, player.transform.up);
        }
    }
}
