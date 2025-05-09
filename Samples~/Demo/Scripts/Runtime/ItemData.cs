using UnityEngine;


namespace Great.Datatable.Demo
{
    [CreateAssetMenu(menuName = "Data/ItemData")]
    public class ItemData : DataTableRow
    {
        public int MaxStack = 1;
        public Sprite Icon;

        public override string ToString()
        {
            return $"Name: {Name}, MaxStack: {MaxStack}";
        }
    }
}