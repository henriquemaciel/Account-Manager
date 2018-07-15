using Newtonsoft.Json;
using DraconiusGoGUI.Enums;
using System;
using System.Collections.Generic;

namespace DraconiusGoGUI.Models
{
    public class AccountExportModel
    {
        [JsonProperty("type")]
        public AuthType Type { get; set; }
        [JsonProperty("username")]
        public string Username { get; set; }
        [JsonProperty("password")]
        public string Password { get; set; }
        [JsonProperty("level")]
        public int Level { get; set; }
        [JsonProperty("pokedex")]
        public List<PokedexEntryExportModel> Pokedex { get; set; }
        [JsonProperty("items")]
        public List<ItemDataExportModel> Items { get; set; }
        [JsonProperty("Creature")]
        public List<CreatureDataExportModel> Creature { get; set; }
        [JsonProperty("eggs")]
        public List<EggDataExportModel> Eggs { get; set; }
        [JsonProperty("exportTime")]
        public DateTime ExportTime { get; set; }

        public AccountExportModel()
        {
            ExportTime = DateTime.Now;
        }
    }

    public class PokedexEntryExportModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("timesEncountered")]
        public int TimesEncountered { get; set; }
        [JsonProperty("timesCaught")]
        public int TimesCaught { get; set; }

        /*
        public PokedexEntryExportModel(PokedexEntry entry)
        {
            Id = (int)entry.CreatureId;
            Name = entry.CreatureId.ToString();
            TimesEncountered = entry.TimesEncountered;
            TimesCaught = entry.TimesCaptured;
        }
        */
    }

    public class ItemDataExportModel
    {
        [JsonProperty("itemName")]
        public string ItemName { get; set; }
        [JsonProperty("count")]
        public int Count { get; set; }

        /*
        public ItemDataExportModel(ItemData itemData)
        {
            ItemName = itemData.ItemId.ToString().Replace("Item", "");
            Count = itemData.Count;
        }
        */
    }

    public class CreatureDataExportModel
    {
        [JsonProperty("pokedexEntry")]
        public int PokedexEntry { get; set; }
        [JsonProperty("CreatureName")]
        public string CreatureName { get; set; }
        [JsonProperty("cp")]
        public int CP { get; set; }
        [JsonProperty("iv")]
        public double IV { get; set; }

        /*
        public CreatureDataExportModel(CreatureData Creature, double iv)
        {
            PokedexEntry = (int)Creature.CreatureId;
            CreatureName = Creature.CreatureId.ToString();
            CP = Creature.Cp;
            IV = iv;
        }
        */
    }

    public class EggDataExportModel
    {
        [JsonProperty("targetDistance")]
        public double TargetDistance { get; set; }
        [JsonProperty("id")]
        public ulong Id { get; set; }

        /*
        public EggDataExportModel(CreatureData Creature)
        {
            TargetDistance = Creature.EggKmWalkedTarget;
            Id = Creature.Id;
        }
        */
    }
}
