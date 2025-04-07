using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using PlasticGui.WorkspaceWindow;


namespace Great.Datatable
{
    [CreateAssetMenu(menuName = "Data Table")]
    public class DataTable : ScriptableObject
    {

#if UNITY_EDITOR
        [SerializeField, TextArea] private string developerNote; // Only visible in Editor
        public static bool showIDs = true;
#endif

        [HideInInspector] public string RowTypeName;
        public List<DataTableRow> Rows = new List<DataTableRow>();

        #region Runtime Methods

        /// <summary>
        /// Return the value from the list by using both index and name
        /// </summary>
        public T GetByIndexAndName<T>(int index, string name) where T : DataTableRow =>
            Rows.FirstOrDefault(r => r.Index == index && r.Name == name) as T;

        /// <summary>
        /// Return the value from the list by using only index
        /// </summary>
        public T GetByIndex<T>(int index) where T : DataTableRow =>
            Rows.FirstOrDefault(r => r.Index == index) as T;

        /// <summary>
        /// Return the value from the list by using only name
        /// </summary>
        public T GetByName<T>(string name) where T : DataTableRow =>
            Rows.FirstOrDefault(r => r.Name == name) as T;

        /// <summary>
        /// Return the Dictionary of the existing database
        /// </summary>
        public Dictionary<(int, string), T> CreateIndexNameDictionary<T>() where T : DataTableRow =>
            Rows.ToDictionary(r => (r.Index, r.Name), r => r as T);

        /// <summary>
        /// Return the list of existing database
        /// </summary>
        public List<DataTableRow> GetRows()
        {
            return Rows;
        }

        /// <summary>
        /// Return the converted/casted list of existing database
        /// </summary>
        public List<T> GetRows<T>() where T : DataTableRow
        {
            List<T> castedRows = new List<T>();
            foreach (var row in Rows)
            {
                T castedRow = row as T;
                castedRows.Add(castedRow);
            }
            return castedRows;
        }

        /// <summary>
        /// Return the readonly list of existing database
        /// </summary>
        public IReadOnlyList<DataTableRow> GetRowsReadOnly()
        {
            return Rows;
        }

        /// <summary>
        /// Return the converted/casted readonly list of existing database
        /// </summary>
        public IReadOnlyList<T> GetRowsReadOnly<T>() where T : DataTableRow
        {
            List<T> castedRows = new List<T>();
            foreach (var row in Rows)
            {
                T castedRow = row as T;
                castedRows.Add(castedRow);
            }
            return castedRows;
        }

        #endregion

#if UNITY_EDITOR

        public void SortByIndex() => Rows = Rows.OrderBy(r => r.Index).ToList();
        public void SortByName() => Rows = Rows.OrderBy(r => r.Name).ToList();
        public int GetNextIndex() => Rows.Count > 0 ? Rows.Max(r => r.Index) + 1 : 0;

        public void AddRow(DataTableRow obj)
        {
            Rows.Add(obj);
        }

        public void RemoveObject(DataTableRow obj)
        {
            Rows.Remove(obj);
        }

#endif

    }
}