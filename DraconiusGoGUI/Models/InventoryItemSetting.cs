﻿using System;

namespace DraconiusGoGUI.Models
{
    [Serializable]
    public class InventoryItemSetting
    {
        //public ItemId Id { get; set; }
        public int MaxInventory { get; set; }

        public InventoryItemSetting()
        {
            //Id = ItemId.ItemUnknown;
            MaxInventory = 100;
        }

        public string FriendlyName
        {
            get
            {
                return string.Empty;
                //return Id.ToString().Replace("Item", "");
            }
        }
    }
}
