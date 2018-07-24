using DraconiusGoGUI.DracoManager;
using DracoProtos.Core.Base;

using System;

namespace DraconiusGoGUI.Models
{
    [Serializable]
    public class InventoryItemSetting
    {
        public ItemType Id { get; set; }
        public int MaxInventory { get; set; }

        public InventoryItemSetting()
        {
            Id = ItemType.MAGIC_BALL_SIMPLE;
            MaxInventory = 100;
        }

        public string FriendlyName { get; set; }
    }
}
