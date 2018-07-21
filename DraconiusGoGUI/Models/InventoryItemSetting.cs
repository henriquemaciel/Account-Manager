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
            Id = 0;
            MaxInventory = 100;
        }

        public string FriendlyName
        {
            get
            {
                return Id.ToString();
            }
        }
    }
}
