using System.Collections.Generic;
using System;
using System.Linq;
using DraconiusGoGUI.Enums;
using DracoProtos.Core.Objects;
using System.Threading.Tasks;
using DraconiusGoGUI.Extensions;
using DraconiusGoGUI.Models;
using DracoProtos.Core.Base;

namespace DraconiusGoGUI.DracoManager
{
    public partial class Manager
    {
        //private ConcurrentDictionary<string, FBagItem> InventoryItems = new ConcurrentDictionary<string, FBagItem>();
        //private ConcurrentDictionary<string, FCreadexEntry> dexEntries = new ConcurrentDictionary<string, FCreadexEntry>();

        /*
        private string GetInventoryItemHashKey(InventoryItem item)
        {
            if (item == null || item.InventoryItemData == null)
                return null;

            var delta = item.InventoryItemData;

            if (delta.AppliedItems != null)
                return "AppliedItems";

            if (delta.AvatarItem != null)
                return "AvatarItem." + delta.AvatarItem.AvatarTemplateId;

            if (delta.Candy != null)
                return "Candy." + delta.Candy.FamilyId;

            if (delta.EggIncubators != null)
                return "EggIncubators";

            if (delta.InventoryUpgrades != null)
                return "InventoryUpgrades";

            if (delta.Item != null)
                return "Item." + delta.Item.ItemId;

            if (delta.PlayerCamera != null)
                return "PlayerCamera";

            if (delta.PlayerCurrency != null)
                return "PlayerCurrency";

            if (delta.PlayerStats != null)
                return "PlayerStats";

            if (delta.PokedexEntry != null)
                return "PokedexEntry." + delta.PokedexEntry.CreatureId;

            if (delta.CreatureData != null)
                return GetCreatureHashKey(delta.CreatureData.Id);

            if (delta.Quest != null)
                return "Quest." + delta.Quest.QuestType;

            if (delta.RaidTickets != null)
                return delta.RaidTickets.RaidTicket.ToString();

            throw new Exception("Unexpected inventory error. Could not generate hash code.");
        }

        private string GetCreatureHashKey(ulong id)
        {
            return "CreatureData." + id;
        }

        private IEnumerable<AppliedItem> GetAppliedItems()
        {
            return InventoryItems.Select(i => i.Value.InventoryItemData?.AppliedItems)
                .Where(aItems => aItems?.Item != null)
                .SelectMany(aItems => aItems.Item);
        }
        */
        private IEnumerable<FBagItem> GetItemsData()
        {
            UserBag = _client.DracoClient.Inventory.GetUserItems();
            return UserBag.items;
        }
        /*
        private ItemData GetItemData(ItemId itemId)
        {
            return GetItemsData()?.FirstOrDefault(p => p.ItemId == itemId);
        }

        private int GetItemCount(ItemId itemId)
        {
            var itemData = GetItemData(itemId);
            return (itemData != null) ? itemData.Count : 0;
        }
        */
        private int GetItemsCount()
        {
            return _client.DracoClient.Inventory.GetUserItems().items.Count();
        }
        /*
        private PlayerStats GetPlayerStats()
        {
            return InventoryItems.Select(i => i.Value.InventoryItemData?.PlayerStats)
                .Where(i => i != null).FirstOrDefault();
        }
        */
        private IEnumerable<FIncubator> GetIncubators()
        {
            return _client.DracoClient.Eggs.GetHatchingInfo().incubators;
        }

        private IEnumerable<FEgg> GetEggs()
        {
            return _client.DracoClient.Eggs.GetHatchingInfo().eggs;
        }

        private IEnumerable<FUserCreature> GetCreatures()
        {
            return _client.DracoClient.Inventory.GetUserCreatures().userCreatures;
        }
        /*
        private IEnumerable<Candy> GetCandies()
        {
            return InventoryItems
                .Select(kvp => kvp.Value.InventoryItemData?.Candy)
                .Where(p => p != null && p.FamilyId > 0);
        }
        */
        private IEnumerable<FCreadexEntry> GetPokedex()
        {
            return _client.DracoClient.Inventory.GetCreadex().entries;
        }
        /*
        private FUserCreature GetCreature(ulong CreatureId)
        {
            return GetCreatures().FirstOrDefault(p => p.Id == CreatureId);
        }
        /*
        private bool RemoveInventoryItem(string key)
        {
            InventoryItem toRemove;
            try
            {
                return InventoryItems.TryRemove(key, out toRemove);
            }
            catch (ArgumentNullException)
            {
                return false;
            }
        }

        internal void RemoveInventoryItems(IEnumerable<InventoryItem> items)
        {
            foreach (var item in items)
            {
                RemoveInventoryItem(item);
            }
        }

        private bool RemoveInventoryItem(InventoryItem item)
        {
            if (item == null)
                return false;

            return RemoveInventoryItem(GetInventoryItemHashKey(item));
        }

        private void AddRemoveOrUpdateItem(InventoryItem item)
        {
            if (item == null)
                return;

            if (item.DeletedItem != null)
            {
                // Items with DeletedItem have a null InventoryItemData and are not added to inventory.
                // But we still need to remove the Creature with Id == item.DeletedItem.CreatureId from the inventory.
                var CreatureToRemoveKey = $"CreatureData.{item.DeletedItem.CreatureId}"; // Manually construct key.
                RemoveInventoryItem(CreatureToRemoveKey);
            }
            else
            {
                InventoryItems.AddOrUpdate(GetInventoryItemHashKey(item), item, (key, oldItem) =>
                {
                    // Check timestamps to make sure we update with a newer item.
                    if (oldItem.ModifiedTimestampMs < item.ModifiedTimestampMs)
                    {
                        // Copy fields over to the old item.
                        oldItem.InventoryItemData = item.InventoryItemData;
                        oldItem.ModifiedTimestampMs = item.ModifiedTimestampMs;
                    }

                    return oldItem;
                });
            }
        }
        */
        /// <summary>
        /// Load Inventory methodes.
        /// </summary>
       public void UpdateInventory(InventoryRefresh type)
        {
            if (!_client.LoggedIn)
            {
              return;
            }

            LogCaller(new LoggerEventArgs($"Updating inventory. Items to load: {type}", LoggerTypes.Debug));

            try
            {
                //foreach (var item in _client.ClientSession.Player.Inventory.InventoryItems)
                //    AddRemoveOrUpdateItem(item);

                switch (type)
                {
                    case InventoryRefresh.All:
                        Items.Clear();
                        Creature.Clear();
                        Pokedex.Clear();
                        //CreatureCandy.Clear();
                        Incubators.Clear();
                        Eggs.Clear();
                        //Stats = GetPlayerStats();
                        Items = GetItemsData().ToList();
                        Pokedex = GetPokedex().ToList();
                        //CreatureCandy = GetCandies().ToList();
                        Incubators = GetIncubators().ToList();
                        Eggs = GetEggs().ToList();
                        Creature = GetCreatures().ToList();
                        break;
                    case InventoryRefresh.Items:
                        Items.Clear();
                        Items = GetItemsData().ToList();
                        break;
                    case InventoryRefresh.Creature:
                        Creature.Clear();
                        Creature = GetCreatures().ToList();
                        break;
                    case InventoryRefresh.Pokedex:
                        Pokedex.Clear();
                        Pokedex = GetPokedex().ToList();
                        break;
                    case InventoryRefresh.CreatureCandy:
                        //CreatureCandy.Clear();
                        //CreatureCandy = GetCandies().ToList();
                        break;
                    case InventoryRefresh.Incubators:
                        Incubators.Clear();
                        Incubators = GetIncubators().ToList();
                        break;
                    case InventoryRefresh.Eggs:
                        Eggs.Clear();
                        Eggs = GetEggs().ToList();
                        break;
                    case InventoryRefresh.Stats:
                        //Stats = GetPlayerStats();
                        break;
                }
            }
            catch (Exception ex1)
            {
                LogCaller(new LoggerEventArgs(String.Format("Failed updating inventory."), LoggerTypes.Exception, ex1));
                ++_failedInventoryReponses;
            }
        }
        
        public async Task<MethodResult> RecycleFilteredItems()
        {
            if (UserBag.items.Count == 0 || Items == null)
                return new MethodResult
                {
                    Message = "Not items here...."
                };

            if (!UserSettings.RecycleItems)
            {
                return new MethodResult
                {
                    Message = "Item deletion not enabled"
                };
            }

            int itemsCount = 0;

            foreach (var item in Items)
            {
                itemsCount += item.count;
            }

            double configPercentItems = UserSettings.PercTransItems * 0.01;

            double percentInventory = UserBag.allowedItemsSize * configPercentItems;

            if (percentInventory > itemsCount)
            {
                return new MethodResult
                {
                    Message = "It has not yet reached 90% of inventory"
                };
            }

            //TODO: skip ThrowInvalidOperationException(ExceptionResource resource)
            try
            {
                foreach (var item in Items)
                {
                    InventoryItemSetting itemSetting = UserSettings.ItemSettings.FirstOrDefault(x => x.Id == item.type);

                    if (itemSetting == null)
                    {
                        continue;
                    }

                    int toDelete = item.count - itemSetting.MaxInventory;

                    if (toDelete <= 0)
                    {
                        continue;
                    }

                    await RecycleItem(itemSetting, toDelete);

                    await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));
                }

                UpdateInventory(InventoryRefresh.Items);

                return new MethodResult
                {
                    Message = "Success",
                    Success = true
                };
            }
            catch
            {
                return new MethodResult();
            }
        }

        public async Task<MethodResult> RecycleItem(ItemType item, int toDelete)
        {
            InventoryItemSetting itemSetting = UserSettings.ItemSettings.FirstOrDefault(x => x.Id == item);

            return itemSetting == null ? new MethodResult() : await RecycleItem(itemSetting, toDelete);
        }
        
        public async Task<MethodResult> RecycleItem(InventoryItemSetting itemSetting, int toDelete)
        {
            if (!_client.LoggedIn)
            {
                MethodResult result = await AcLogin();

                if (!result.Success)
                {
                    return result;
                }
            }

            bool recycled = (bool)_client.DracoClient.Inventory.DiscardItem(itemSetting.Id, toDelete);

            if (recycled)
            {
                LogCaller(new LoggerEventArgs(String.Format("Deleted {0} {1}", toDelete, itemSetting.FriendlyName), LoggerTypes.Recycle));

                return new MethodResult
                {
                    Success = true
                };
            }
            return new MethodResult();
        }

        private async Task<MethodResult> UseIncense(ItemType item = ItemType.INCENSE)
        {
            if (!_client.LoggedIn)
            {
                MethodResult result = await AcLogin();

                if (!result.Success)
                {
                    return result;
                }
            }
            /*
            if (Items.FirstOrDefault(x => x.type == ItemType.INCENSE).Count == 0)
                return new MethodResult();

            var response = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
            {
                RequestType = RequestType.UseIncense,
                RequestMessage = new UseIncenseMessage
                {
                    IncenseType = item
                }.ToByteString()
            });

            if (response == null)
                return new MethodResult();

            UseIncenseResponse useIncenseResponse = UseIncenseResponse.Parser.ParseFrom(response);

            switch (useIncenseResponse.Result)
            {
                case UseIncenseResponse.Types.Result.IncenseAlreadyActive:
                    return new MethodResult();
                case UseIncenseResponse.Types.Result.LocationUnset:
                    return new MethodResult();
                case UseIncenseResponse.Types.Result.Success:
                    LogCaller(new LoggerEventArgs(String.Format("Used incense {0}.", item), LoggerTypes.Success));
                    return new MethodResult
                    {
                        Success = true
                    };
                case UseIncenseResponse.Types.Result.NoneInInventory:
                    return new MethodResult();
                case UseIncenseResponse.Types.Result.Unknown:
                    return new MethodResult();
            }*/
            return new MethodResult();
        }

        public double FilledInventoryStorage()
        {
            if (Items == null || PlayerData == null)
            {
                return 100;
            }

            return (double)Items.Sum(x => x.count) / UserBag.allowedItemsSize * 100;
        }

        public double FilledCreatureStorage()
        {
            if (Creature == null || Stats == null)
            {
                return 100;
            }

            return (double)(Creature.Count /*+ Eggs.Count*/) / Stats.creatureStorageSize * 100;
        }
    }
}
