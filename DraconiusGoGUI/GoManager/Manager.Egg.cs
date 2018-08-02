using DraconiusGoGUI.Enums;
using DraconiusGoGUI.Extensions;
using DracoProtos.Core.Base;
using DracoProtos.Core.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DraconiusGoGUI.DracoManager
{
    public partial class Manager
    {
        private async Task<MethodResult> IncubateEggs()
        {
            if (!UserSettings.IncubateEggs)
            {
                LogCaller(new LoggerEventArgs("Incubating disabled", LoggerTypes.Info));

                return new MethodResult
                {
                    Message = "Incubate eggs disabled",
                    Success = true
                };
            }

            MethodResult<FIncubator> incubatorResponse = GetIncubator();

            if (!incubatorResponse.Success)
            {
                return new MethodResult
                {
                    Message = incubatorResponse.Message,
                    Success = true
                };
            }

            FEgg egg = Eggs.FirstOrDefault(x => String.IsNullOrEmpty(x.id));

            if (egg == null)
            {
                return new MethodResult
                {
                    Message = "No egg to incubate",
                    Success = true
                };
            }

            if (!_client.LoggedIn)
            {
                MethodResult result = await AcLogin();

                if (!result.Success)
                {
                    return result;
                }
            }

            var response = _client.DracoClient.Eggs.StartHatchingEgg(egg.id, incubatorResponse.Data.incubatorId);
                
            if (response == null)
                return new MethodResult();

            var incitem = incubatorResponse.Data.eggId;
            var _egg = egg.id;

            LogCaller(new LoggerEventArgs(String.Format("Incubating egg in {0}. Creature Id: {1}", incitem, _egg), LoggerTypes.Incubate));

            UpdateInventory(InventoryRefresh.Eggs);
            UpdateInventory(InventoryRefresh.Incubators);

            return new MethodResult
            {
                Message = "Success",
                Success = true
            };
        }

        private MethodResult<FIncubator> GetIncubator()
        {
            if(Incubators == null)
            {
                return new MethodResult<FIncubator>();
            }

            FIncubator unusedUnlimitedIncubator = Incubators.FirstOrDefault(x => x.itemType == ItemType.INCUBATOR_PERPETUAL && x.eggId != null);

            if(unusedUnlimitedIncubator != null)
            {
                return new MethodResult<FIncubator>
                {
                    Data = unusedUnlimitedIncubator,
                    Success = true
                };
            }

            if (!UserSettings.OnlyUnlimitedIncubator)
            {
                IEnumerable<FIncubator> incubators = Incubators.Where(x => x.itemType == ItemType.INCUBATOR_PERPETUAL && x.eggId != null);
    
                foreach(FIncubator incubator in incubators)
                {
                    return new MethodResult<FIncubator>
                    {
                        Data = incubator,
                        Success = true
                    };
                }
            }

            return new MethodResult<FIncubator>
            {
                Message = "No unused incubators"
            };
        }
    }
}
