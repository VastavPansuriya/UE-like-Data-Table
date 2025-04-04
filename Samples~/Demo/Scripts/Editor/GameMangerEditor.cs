using UnityEditor;
using UnityEngine;

namespace Great.Datatable.Demo
{
    [CustomEditor(typeof(GameManager))]
    public class GameMangerEditor : Editor
    {
        GameManager gameManager;
        private void OnEnable()
        {
            gameManager = (GameManager)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.Space(10);
            if (GUILayout.Button("PrintLog"))
            {
                gameManager.PrintLogs();
            }
        }
    }
}
