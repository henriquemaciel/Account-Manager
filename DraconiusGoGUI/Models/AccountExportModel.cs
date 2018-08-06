using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using DracoProtos.Core.Base;
using DracoProtos.Core.Objects;

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
        public bool TimesEncountered { get; set; }
        [JsonProperty("timesCaught")]
        public int TimesCaught { get; set; }

        public PokedexEntryExportModel(FCreadexEntry entry)
        {
            Id = (int)entry.name;
            Name = entry.name.ToString();
            TimesEncountered = entry.seen;
            TimesCaught = entry.caughtQuantity;
        }
    }

    public class ItemDataExportModel
    {
        [JsonProperty("itemName")]
        public string ItemName { get; set; }
        [JsonProperty("count")]
        public int Count { get; set; }

        public ItemDataExportModel(FBagItem itemData)
        {
            ItemName = itemData.type.ToString();
            Count = itemData.count;
        }
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

        public CreatureDataExportModel(FUserCreature Creature, double iv)
        {
            PokedexEntry = (int)Creature.name;
            CreatureName = Creature.name.ToString();
            CP = Creature.cp;
            IV = iv;
        }
    }

    public class EggDataExportModel
    {
        [JsonProperty("targetDistance")]
        public double TargetDistance { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }

        public EggDataExportModel(FEgg Creature)
        {
            TargetDistance = Creature.passedDistance;
            Id = Creature.id;
        }
    }
}
