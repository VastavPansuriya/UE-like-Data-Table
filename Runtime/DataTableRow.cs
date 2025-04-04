using UnityEngine;


namespace Great.Datatable
{
    public abstract class DataTableRow : ScriptableObject
    {
        public string Name;
        [HideInInspector] public string ID;

#if UNITY_EDITOR
        [TextArea] public string DeveloperNotes;
#endif

        [Space(20)]
        [Header("Game Data")]
        [SerializeField, HideInInspector] private int _index;
        public int Index => _index;
        public bool Enabled = true;
    }
}