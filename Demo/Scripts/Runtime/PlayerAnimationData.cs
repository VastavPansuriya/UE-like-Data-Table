using UnityEngine;

namespace Great.Datatable.Demo
{
    [CreateAssetMenu(menuName = "Data/PlayerAnimationData")]
    public class PlayerAnimationData : DataTableRow
    {
        public float health;
        public float animationSpeed;
        public AnimationClip AnimationToPlayOnDamage;

        public override string ToString()
        {
            return $"Health: {health}, AnimationSpeed: {animationSpeed}," +
                $" AnimationClipName: {AnimationToPlayOnDamage.name}";
        }
    }
}