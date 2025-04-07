using System;
using System.Collections.Generic;
using UnityEngine;

namespace Great.Datatable.Demo
{
    public class GameManager : MonoBehaviour
    {
        public DataTable itemDataTable;
        public DataTable playerAnimDataTable;

        private void Start()
        {
            PrintLogs();
        }

        public void PrintLogs()
        {
            Player();
            Item();
        }

        private void Player()
        {
            List<PlayerAnimationData> playerAnimationDatas = playerAnimDataTable.GetRows<PlayerAnimationData>();
            foreach (PlayerAnimationData data in playerAnimationDatas)
            {
                Debug.Log($"<color=red>AnimationData All Data Log:</color> {data}");
            }
        }

        private void Item()
        {
            ItemData itemData = itemDataTable.GetByIndex<ItemData>(0);
            Debug.Log($"<color=yellow>ItemData Single Data Log:</color>" + itemData.ToString());
        }
    }
}