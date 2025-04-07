
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Great.Datatable
{
    [CustomEditor(typeof(DataTable))]
    public class DataTableEditor : Editor
    {

        private DataTable dataTable;
        private Type[] rowTypes;
        private bool typeLocked;
        private string searchFilter = "";
        private Vector2 scrollPos;
        private SerializedProperty rowsProp;
        private SerializedProperty rowTypeNameProp;
        private SerializedProperty developerNote;

        private Dictionary<int, bool> rowExpandedStates = new Dictionary<int, bool>();
        private SearchType searchType;
        private bool sortByName = false;
        private int selectedTypeIndex = 0;

        public enum SearchType
        {
            Index,
            Name,
            All
        }

        private void OnEnable()
        {
            dataTable = (DataTable)target;
            rowTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(t => t.IsSubclassOf(typeof(DataTableRow)) && !t.IsAbstract)
                .ToArray();

            rowsProp = serializedObject.FindProperty("Rows");
            rowTypeNameProp = serializedObject.FindProperty("RowTypeName");
            developerNote = serializedObject.FindProperty("developerNote");

            if (!string.IsNullOrEmpty(dataTable.RowTypeName))
            {
                typeLocked = true;
                selectedTypeIndex = GetIndexOfSelectedType();
            }
        }

        public int GetIndexOfSelectedType()
        {
            for (int i = 0; i < rowTypes.Length; i++)
            {
                var type = rowTypes[i];
                Debug.Log(type.Name + "," + dataTable.RowTypeName);
                if (type.Name == dataTable.RowTypeName)
                {
                    return i;
                }
            }
            return -1;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();


            if (!typeLocked)
            {
                DrawTypeSelection();
            }
            else
            {
                EditorGUILayout.LabelField(dataTable.RowTypeName, EditorStyles.boldLabel);
                DrawSectionSeparator();
                EditorGUILayout.PropertyField(developerNote);
                EditorGUILayout.Space();
                DrawDataTableManagement();
            }

            serializedObject.ApplyModifiedProperties();
        }


        private void DrawTypeSelection()
        {
            EditorGUILayout.HelpBox("Select the row type for this DataTable", MessageType.Info);

            var typeNames = rowTypes.Select(t => t.Name).ToArray();
            selectedTypeIndex = EditorGUILayout.Popup("Row Type", selectedTypeIndex, typeNames);


            if (GUILayout.Button("Lock Type", GUILayout.Height(30)))
            {
                rowTypeNameProp.stringValue = rowTypes[selectedTypeIndex].Name;
                dataTable.RowTypeName = rowTypes[selectedTypeIndex].Name;
                typeLocked = true;
                SaveChanges();
            }
        }

        private void DrawDataTableManagement()
        {
            DrawTopToolbar();

            EditorGUILayout.Space();

            // Add New Row button
            if (GUILayout.Button("Add New Row", GUILayout.Height(25)))
            {
                AddNewRow();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Data Rows", EditorStyles.boldLabel);

            DrawSearchFilter();
            EditorGUILayout.Space();
            DrawRowList();
        }

        private void DrawTopToolbar()
        {
            EditorGUILayout.BeginHorizontal();
            DataTable.showIDs = GUILayout.Toggle(DataTable.showIDs, "Show IDs", "Button", GUILayout.Width(80));
            sortByName = GUILayout.Toggle(sortByName, "Sort by Name", "Button", GUILayout.Width(100));

            EditorGUILayout.EndHorizontal();
        }
        private List<DataTableRow> GetFilteredRows()
        {
            List<DataTableRow> baseList = new List<DataTableRow>();
            switch (searchType)
            {
                case SearchType.Index:
                    baseList = FilterIndex(baseList);
                    break;
                case SearchType.Name:
                    baseList = FilterName(baseList);
                    break;
                case SearchType.All:
                default:
                    baseList = FilterIndexAndName(baseList);
                    break;
            }


            return sortByName ?
                baseList.OrderBy(r => r.Name).ToList() :
                baseList;
        }

        private List<DataTableRow> FilterIndex(List<DataTableRow> baseList)
        {
            baseList.Clear();
            baseList = dataTable.GetRows()
                  .Where(r => r != null)
                  .Where(r =>
                  {
                      bool isValiedIndex = int.TryParse(searchFilter, out int result);
                      return searchFilter == "" || isValiedIndex && r.Index.ToString().Contains(result.ToString());
                  })
                  .ToList();
            return baseList;
        }

        private List<DataTableRow> FilterName(List<DataTableRow> baseList)
        {
            baseList.Clear();
            baseList = dataTable.GetRows()
                   .Where(r => r != null)
                   .Where(r =>
                   {
                       bool isValiedIndex = int.TryParse(searchFilter, out int result);
                       return searchFilter == "" ||
                                          r.Name.Contains(searchFilter, StringComparison.OrdinalIgnoreCase);
                   })
                   .ToList();
            return baseList;
        }

        private List<DataTableRow> FilterIndexAndName(List<DataTableRow> baseList)
        {
            baseList.Clear();
            baseList = dataTable.GetRows()
                   .Where(r => r != null)
                   .Where(r =>
                   {
                       bool isValiedIndex = int.TryParse(searchFilter, out int result);
                       return searchFilter == "" ||
                                          r.Name.Contains(searchFilter, StringComparison.OrdinalIgnoreCase) ||
                                          isValiedIndex && r.Index == result;
                   })
                   .ToList();
            return baseList;
        }

        private void DrawSearchFilter()
        {
            EditorGUILayout.BeginHorizontal();
            {
                // Search type dropdown
                searchType = (SearchType)EditorGUILayout.EnumPopup(
                    searchType,
                    EditorStyles.toolbarDropDown,
                    GUILayout.Width(80)
                );

                // Search field
                searchFilter = EditorGUILayout.TextField(
                    searchFilter,
                    EditorStyles.toolbarSearchField,
                    GUILayout.ExpandWidth(true)
                );

                // Clear button
                if (GUILayout.Button("×", EditorStyles.miniButton, GUILayout.Width(20)))
                {
                    searchFilter = "";
                    GUI.FocusControl(null);
                }
            }
            EditorGUILayout.EndHorizontal();
        }


        private void DrawRowList()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            // Get filtered and sorted rows
            var filteredRows = GetFilteredRows();

            foreach (var row in filteredRows)
            {
                DrawRow(row);
            }

            EditorGUILayout.EndScrollView();
        }


        private void DrawRow(DataTableRow row)
        {
            if (row == null) return;

            int instanceID = row.GetInstanceID();
            if (!rowExpandedStates.ContainsKey(instanceID))
            {
                rowExpandedStates[instanceID] = false;
            }

            // Main vertical container
            EditorGUILayout.BeginVertical();

            // Header with custom spacing
            EditorGUILayout.BeginHorizontal();
            {
                // Foldout with left padding
                GUILayout.Space(8);  // Add left margin
                rowExpandedStates[instanceID] = EditorGUILayout.Foldout(
                    rowExpandedStates[instanceID],
                    "",
                    true,
                    EditorStyles.foldoutHeader
                );

                GUILayout.Space(-45);


                // Header label
                EditorGUILayout.LabelField(GetRowHeader(row), EditorStyles.boldLabel);

                // Flexible space
                GUILayout.FlexibleSpace();

                // Minimal remove button
                if (GUILayout.Button("×", EditorStyles.miniButtonMid, GUILayout.Width(20)))
                {
                    RemoveRow(row);
                }
            }
            EditorGUILayout.EndHorizontal();

            // Content area with background
            if (rowExpandedStates[instanceID])
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                DrawRowContent(row);
                HandleContextMenu(row);
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndVertical();
        }

        private string GetRowHeader(DataTableRow row)
        {
            if (row == null) return "Null Row";

            return DataTable.showIDs ?
                $"{row.Index}: {row.Name} (ID: {row.ID.Substring(0, 4)})" :
                $"{row.Index}: {row.Name}";
        }

        private void HandleContextMenu(DataTableRow row)
        {
            if (Event.current.type == EventType.ContextClick &&
                GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
            {
                CreateContextMenu(row).ShowAsContext();
                Event.current.Use();
            }
        }


        private void DrawRowContent(DataTableRow row)
        {
            if (row == null) return;

            // Draw index as non-editable field
            EditorGUILayout.LabelField("Index", row.Index.ToString());

            var so = new SerializedObject(row);
            var iterator = so.GetIterator();
            bool enterChildren = true;
            bool drawnSeparator = false;

            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;

                if (iterator.name == "ID" && !DataTable.showIDs) continue;
                if (iterator.name == "_index") continue;
                if (iterator.name == "m_Script") continue;
                if (iterator.name == "Enabled") continue;


                if (!drawnSeparator && iterator.name == "DeveloperNotes")
                {
                    EditorGUILayout.PropertyField(iterator, true);
                    DrawSectionSeparator();
                    drawnSeparator = true;
                    continue;
                }

                EditorGUILayout.PropertyField(iterator, true);
            }
            so.ApplyModifiedProperties();
        }


        private void DrawSectionSeparator()
        {
            EditorGUILayout.Space();
            Rect rect = EditorGUILayout.GetControlRect(false, 1);
            rect.height = 2;
            EditorGUI.DrawRect(rect, new Color(0.35f, 0.35f, 0.35f, 1));
            EditorGUILayout.Space(20);
        }

        private GenericMenu CreateContextMenu(DataTableRow row)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Duplicate"), false, () => DuplicateRow(row));
            menu.AddItem(new GUIContent("Move Up"), false, () => MoveRow(row, -1));
            menu.AddItem(new GUIContent("Move Down"), false, () => MoveRow(row, 1));
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Delete"), false, () => RemoveRow(row));
            return menu;
        }

        private void AddNewRow()
        {
            var rowType = rowTypes[GetIndexOfSelectedType()];
            int counter = 0;
            foreach (var row in rowTypes)
            {
                Debug.Log(counter++ + "," + row.Name);
            }

            var newRow = (DataTableRow)ScriptableObject.CreateInstance(rowType.Name);
            newRow.name = "New_" + rowType.Name;
            newRow.ID = Guid.NewGuid().ToString();

            // Initialize DisplayName if exists
            var displayNameField = newRow.GetType().GetField("Name",
                BindingFlags.Public | BindingFlags.Instance);
            displayNameField?.SetValue(newRow, "New Item");

            AssetDatabase.AddObjectToAsset(newRow, dataTable);
            dataTable.AddRow(newRow);
            RefreshIndexes();
            SaveChanges();
        }

        private void DuplicateRow(DataTableRow original)
        {
            var newRow = Instantiate(original);
            newRow.name = original.name + "_Copy";
            newRow.ID = Guid.NewGuid().ToString();
            AssetDatabase.AddObjectToAsset(newRow, dataTable);
            dataTable.AddRow(newRow);
            RefreshIndexes();
            SaveChanges();
        }

        private void MoveRow(DataTableRow row, int direction)
        {
            List<DataTableRow> dataTableRows = dataTable.GetRows();
            int index = dataTableRows.IndexOf(row);
            int newIndex = Mathf.Clamp(index + direction, 0, dataTableRows.Count - 1);

            if (index != newIndex)
            {
                dataTableRows.RemoveAt(index);
                dataTableRows.Insert(newIndex, row);
                SaveChanges();
            }
            RefreshIndexes();
            SaveChanges();
        }

        private void RemoveRow(DataTableRow row)
        {
            if (row == null) return;

            dataTable.Rows.Remove(row);
            DestroyImmediate(row, true);
            RefreshIndexes();
            SaveChanges();
        }

        public override void SaveChanges()
        {
            base.SaveChanges();
            EditorUtility.SetDirty(dataTable);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            serializedObject.Update();
        }

        private void RefreshIndexes()
        {
            List<DataTableRow> rows = dataTable.Rows;
            for (int i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                SerializedObject serializedRow = new SerializedObject(row);
                serializedRow.FindProperty("_index").intValue = i;
                serializedRow.ApplyModifiedProperties();
            }
        }
    }
}