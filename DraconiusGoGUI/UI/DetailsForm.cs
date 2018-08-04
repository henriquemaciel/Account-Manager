using BrightIdeasSoftware;
using DraconiusGoGUI.Enums;
using DraconiusGoGUI.Extensions;
using DraconiusGoGUI.DracoManager;
using DraconiusGoGUI.DracoManager.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DracoProtos.Core.Objects;
using System.Threading.Tasks;
using DracoProtos.Core.Base;

namespace DraconiusGoGUI.UI
{
    public partial class DetailsForm : System.Windows.Forms.Form
    {
        private Manager _manager;
        private int _totalLogs = 0;

        public DetailsForm(Manager manager)
        {
            InitializeComponent();

            _manager = manager;

            fastObjectListViewLogs.PrimarySortColumn = olvColumnDate;
            fastObjectListViewLogs.PrimarySortOrder = SortOrder.Descending;
            fastObjectListViewLogs.ListFilter = new TailFilter(500);

            fastObjectListViewPokedex.PrimarySortColumn = olvColumnPokedexFriendlyName;
            fastObjectListViewPokedex.PrimarySortOrder = SortOrder.Ascending;

            fastObjectListViewLogs.BackColor = Color.FromArgb(43, 43, 43);

            fastObjectListViewPokedex.BackColor = Color.FromArgb(43, 43, 43);
            fastObjectListViewPokedex.ForeColor = Color.LightGray;

            fastObjectListViewCreature.BackColor = Color.FromArgb(43, 43, 43);
            fastObjectListViewCreature.ForeColor = Color.LightGray;

            fastObjectListViewInventory.BackColor = Color.FromArgb(43, 43, 43);
            fastObjectListViewInventory.ForeColor = Color.LightGray;

            fastObjectListViewEggs.BackColor = Color.FromArgb(43, 43, 43);
            fastObjectListViewEggs.ForeColor = Color.LightGray;

            fastObjectListViewCandy.BackColor = Color.FromArgb(43, 43, 43);
            fastObjectListViewCandy.ForeColor = Color.LightGray;

            #region Pokedex

            //ToString for sorting purposes
            olvColumnPokedexFriendlyName.AspectGetter = (entry) => (int)(entry as FCreadexEntry).element;

            olvColumnPokedexId.AspectGetter = (entry) => _manager.Strings.GetCreatureName((entry as FCreadexEntry).name);

            olvColumnPokedexFriendlyName.AspectGetter = (entry) => (int)(entry as FCreadexEntry).name;

            #endregion

            #region Creature


            olvColumnCreatureId.AspectGetter = (Creature) => (Creature as FUserCreature).id;


            olvColumnCandyToEvolve.AspectGetter = delegate (object Creature)
            {
                int cost = (Creature as FUserCreature).improveCandiesCost;
                return cost == 0 ? 0 : cost;
            };

            olvColumnCreatureCandy.AspectGetter = delegate (object Creature)
            {
                if (!_manager.CreatureCandy.Any())
                {
                    return 0;
                }

                var candy = _manager.CreatureCandy[(Creature as FUserCreature).candyType];

                return candy == 0 ? 0 : candy;
            };
            
            olvColumnCreatureName.AspectGetter = delegate (object Creature)
            {
                return String.IsNullOrEmpty((Creature as FUserCreature).alias) ? _manager.Strings.GetCreatureName((Creature as FUserCreature).name) : (Creature as FUserCreature).alias;
            };

            olvColumnPerfectPercent.AspectGetter = delegate (object Creature)
            {
                double settings = Manager.CalculateIVPerfection(Creature as FUserCreature);
                string sDouble = String.Format("{0:0.00}", settings);
                return double.Parse(sDouble);
            };

            #endregion

            #region Candy

            olvColumnCandyFamily.AspectGetter = delegate (object x)
            {
                //var creatureType = (FUserCreature)x;
                return 0;// creatureType.candyType;
            };

            olvColumnCandyAmount.AspectGetter = delegate (object x)
            {
                if (!_manager.CreatureCandy.Any())
                {
                    return 0;
                }

                var candy = 0;// _manager.CreatureCandy[(CreatureType)x];

                return candy == 0 ? 0 : candy;
            };

            #endregion

            #region Inventory

            olvColumnInventoryItem.AspectGetter = delegate (object x)
            {
                var item = (FBagItem)x;

                return _manager.Strings.GetItemName(item.type);
            };
            

            #endregion
        }

        private void DetailsForm_Load(object sender, EventArgs e)
        {
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            _totalLogs = _manager.Logs.Count;

            if (_manager.LogHeaderSettings != null)
            {
                fastObjectListViewLogs.RestoreState(_manager.LogHeaderSettings);
            }

            fastObjectListViewLogs.SetObjects(_manager.Logs);

            var values = new List<LoggerTypes>();

            foreach (LoggerTypes type in Enum.GetValues(typeof(LoggerTypes)))
            {
                if (type == LoggerTypes.LocationUpdate)
                {
                    continue;
                }

                values.Add(type);
            }

            olvColumnStatus.ValuesChosenForFiltering = values;
            fastObjectListViewLogs.UpdateColumnFiltering();

            Text = _manager.AccountName;

            _manager.OnLog += _manager_OnLog;

            DisplayDetails();
        }

        private void _manager_OnLog(object sender, LoggerEventArgs e)
        {
            if (!_manager.IsRunning)
                return;
                
            if (fastObjectListViewLogs.IsDisposed || fastObjectListViewLogs.Disposing)
            {
                return;
            }

            fastObjectListViewLogs.SetObjects(_manager.Logs);

            DisplayDetails();

            if (e.LogType != LoggerTypes.LocationUpdate)
            {
                Invoke(new MethodInvoker(() =>
                {
                    if (tabControlMain.SelectedTab == tabPageLogs)
                    {
                        _totalLogs = _manager.TotalLogs;
                    }
                    else
                    {
                        int newLogs = _manager.TotalLogs - _totalLogs;

                        if (newLogs > 0)
                        {
                            tabPageLogs.Text = String.Format("Logs ({0})", newLogs);
                        }
                    }
                }));
            }
        }

        private void DetailsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _manager.OnLog -= _manager_OnLog;

            _manager.LogHeaderSettings = fastObjectListViewLogs.SaveState();
        }

        private void DisplayDetails()
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(DisplayDetails));

                return;
            }

            labelPlayerLevel.Text = _manager.Level.ToString();
            labelPlayerTeam.Text = !String.IsNullOrEmpty(_manager.Team) ? _manager.Team : "Neutral";
            labelExp.Text = _manager.ExpRatio.ToString();
            labelRunningTime.Text = _manager.RunningTime;
            labelStardust.Text = _manager.TotalStardust.ToString();
            labelExpPerHour.Text = String.Format("{0:0}", _manager.ExpPerHour);
            labelExpGained.Text = _manager.ExpGained.ToString();
            labelPokeCoins.Text = _manager.TotalPokeCoins.ToString();
            
            if (_manager.Stats != null)
            {
                labelDistanceWalked.Text = String.Format("{0:0.00}km", _manager.Stats.totalDistanceF);
                labelCreatureCaught.Text = _manager.Stats.monstersCaughtCount.ToString();
                //labelBuildingVisits.Text = _manager.Stats.BuildingVisits.ToString();
                labelUniqueCreature.Text = _manager.DracoDex.Count().ToString();
                DateTime date = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(_manager.Stats.registerDate);
                labelCreateDate.Text = date.ToString();
                labelCreatureBuddy.Text = (_manager.Stats.buddy != null && _manager.Strings != null) ? String.Format("{0}", _manager.Strings.GetCreatureName(_manager.Stats.buddy.creature)) : "Not set";
            }

            if (_manager.Creature != null)
            {
                labelCreatureCount.Text = String.Format("{0}/{1}", _manager.Creature.Count + _manager.Eggs.Count, _manager.MaxCreatureStorage);
                labelDeployedCreatures.Text = _manager.Creature.Where(i => i.isArenaDefender).Count().ToString();
            }

            if (_manager.Items != null)
            {
                labelInventoryCount.Text = String.Format("{0}/{1}", _manager.Items.Sum(x => x.count), _manager.MaxItemStorage);
            }
            
            if (_manager.PlayerData != null)
            {
                labelPlayerUsername.Text = _manager.PlayerData.nickname;
            }
        }

        private void ButtonUpdateStats_Click(object sender, EventArgs e)
        {
            DisplayDetails();
        }

        private void FastObjectListViewLogs_FormatRow(object sender, FormatRowEventArgs e)
        {
            var log = e.Model as Log;

            if (log == null)
            {
                return;
            }

            e.Item.ForeColor = log.GetLogColor();
        }

        private void FastObjectListViewCreature_FormatCell(object sender, FormatCellEventArgs e)
        {
            var CreatureData = (FUserCreature)e.Model;

            if (CreatureData == null)
                return;

            if (e.Column == olvColumnCreatureName)
            {
                bool bubby = _manager.Stats.buddy?.id == CreatureData.id == true;
                if (bubby)
                {
                    e.SubItem.ForeColor = Color.Blue;
                }
                else if (CreatureData.isArenaDefender || CreatureData.isLibraryDefender)
                {
                    e.SubItem.ForeColor = Color.LightGreen;
                }
            }
            else if (e.Column == olvColumnCreatureCandy)
            {
                int candy = (int)olvColumnCreatureCandy.GetValue(CreatureData);
                int candyToEvolve = (int)olvColumnCandyToEvolve.GetValue(CreatureData);

                if (candyToEvolve > 0)
                {
                    e.SubItem.ForeColor = candy >= candyToEvolve ? Color.Green : Color.Red;
                }
            }
            else if (e.Column == olvColumnPerfectPercent)
            {
                double perfectPercent = Convert.ToDouble(olvColumnPerfectPercent.GetValue(CreatureData));

                if (perfectPercent >= 95)
                {
                    e.SubItem.ForeColor = Color.Green;
                }
                else if (perfectPercent >= 80)
                {
                    e.SubItem.ForeColor = Color.Orange;
                }
                else
                {
                    e.SubItem.ForeColor = Color.Red;
                }
            }
            else if (e.Column == olvColumnAttack)
            {
                if (CreatureData.attackValue >= 5)
                {
                    e.SubItem.ForeColor = Color.Green;
                }
                else if (CreatureData.attackValue >= 4)
                {
                    e.SubItem.ForeColor = Color.Yellow;
                }
                else if (CreatureData.attackValue >= 3)
                {
                    e.SubItem.ForeColor = Color.Orange;
                }
                else
                {
                    e.SubItem.ForeColor = Color.Red;
                }
            }
            else if (e.Column == olvColumnStamina)
            {
                if (CreatureData.staminaValue >= 5)
                {
                    e.SubItem.ForeColor = Color.Green;
                }
                else if (CreatureData.staminaValue >= 4)
                {
                    e.SubItem.ForeColor = Color.Yellow;
                }
                else if (CreatureData.staminaValue >= 3)
                {
                    e.SubItem.ForeColor = Color.Orange;
                }
                else
                {
                    e.SubItem.ForeColor = Color.Red;
                }
            }
            else if (e.Column == olvColumnCreatureGroup)
            {
                if (CreatureData.group == 3)
                {
                    e.SubItem.ForeColor = Color.Red;
                    e.SubItem.Text = "Red";
                }
                else if (CreatureData.staminaValue == 2)
                {
                    e.SubItem.ForeColor = Color.Yellow;
                    e.SubItem.Text = "Yellow";
                }
                else if (CreatureData.staminaValue == 1)
                {
                    e.SubItem.ForeColor = Color.Green;
                    e.SubItem.Text = "Green";
                }
                else
                    e.SubItem.Text = "None";
            }
            else if (e.Column == olvColumnPrimaryMove)
            {
                e.SubItem.Text = _manager.Strings.GetString("skill.main." + CreatureData.mainSkill);
            }
            else if (e.Column == olvColumnSecondaryMove)
            {
                e.SubItem.Text = _manager.Strings.GetString("skill.charge." + CreatureData.chargedSkill);
            }
            else if (e.Column == olvColumnEPS)
            {
                e.SubItem.Text = String.Format("{0:0.0}", CreatureData.mainSkillEps);
            }
            else if (e.Column == olvColumnDPSmain)
            {
                e.SubItem.Text = String.Format("{0:0}", CreatureData.mainSkillDps);
            }
            else if (e.Column == olvColumnDPS)
            {
                e.SubItem.Text = String.Format("{0:0}", CreatureData.chargedSkillDps);
            }
        }

        private async void UpgradeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(String.Format("Are you sure you want to upgrade {0} Creature?", fastObjectListViewCreature.SelectedObjects.Cast<FUserCreature>().Count()), "Confirmation", MessageBoxButtons.YesNo);

            if (result != DialogResult.Yes)
            {
                return;
            }
           
            if (fastObjectListViewCreature.SelectedObjects.Count == 0 || fastObjectListViewCreature.SelectedObjects.Cast<FUserCreature>().FirstOrDefault() == null)
                return;

            contextMenuStripCreatureDetails.Enabled = false;

            MethodResult managerResult = await _manager.UpgradeCreature(fastObjectListViewCreature.SelectedObjects.Cast<FUserCreature>());

            DisplayDetails();

            contextMenuStripCreatureDetails.Enabled = true;

            fastObjectListViewCreature.SetObjects(_manager.Creature);

            MessageBox.Show("Finished upgrade Creature");
        }

        private async void TransferToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(String.Format("Are you sure you want to transfer {0} Creature?", fastObjectListViewCreature.SelectedObjects.Count), "Confirmation", MessageBoxButtons.YesNo);

            if (result != DialogResult.Yes)
            {
                return;
            }

            if (fastObjectListViewCreature.SelectedObjects.Count == 0 || fastObjectListViewCreature.SelectedObjects.Cast<FUserCreature>().FirstOrDefault() == null)
                return;

            contextMenuStripCreatureDetails.Enabled = false;

            MethodResult managerResult = await _manager.TransferCreature(fastObjectListViewCreature.SelectedObjects.Cast<FUserCreature>());

            DisplayDetails();

            contextMenuStripCreatureDetails.Enabled = true;

            fastObjectListViewCreature.SetObjects(_manager.Creature);

            MessageBox.Show("Finished transferring Creature");
        }

        private async void EvolveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(String.Format("Are you sure you want to evolve {0} Creature?", fastObjectListViewCreature.SelectedObjects.Count), "Confirmation", MessageBoxButtons.YesNo);

            if (result != DialogResult.Yes)
            {
                return;
            }

            if (fastObjectListViewCreature.SelectedObjects.Count == 0 || fastObjectListViewCreature.SelectedObjects.Cast<FUserCreature>().FirstOrDefault() == null)
                return;

            contextMenuStripCreatureDetails.Enabled = false;

            await _manager.EvolveCreature(fastObjectListViewCreature.SelectedObjects.Cast<FUserCreature>());

            DisplayDetails();

            contextMenuStripCreatureDetails.Enabled = true;

            fastObjectListViewCreature.SetObjects(_manager.Creature);

            MessageBox.Show("Finished evolving Creature");
        }

        private void TabControlMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControlMain.SelectedTab == tabPageLogs)
            {
                _totalLogs = _manager.TotalLogs;
                tabPageLogs.Text = "Logs";
            }
            else if (tabControlMain.SelectedTab == tabPageCreature)
            {
                _manager.UpdateInventory(InventoryRefresh.Creature);
                fastObjectListViewCreature.SetObjects(_manager.Creature);
            }
            else if (tabControlMain.SelectedTab == tabPageCandy)
            {
                _manager.UpdateInventory(InventoryRefresh.CreatureCandy);
                fastObjectListViewCandy.SetObjects(_manager.CreatureCandy);
            }
            else if (tabControlMain.SelectedTab == tabPageEggs)
            {
                _manager.UpdateInventory(InventoryRefresh.Eggs);
                _manager.UpdateInventory(InventoryRefresh.Incubators);
                fastObjectListViewEggs.SetObjects(_manager.Eggs);
            }
            else if (tabControlMain.SelectedTab == tabPageInventory)
            {
                _manager.UpdateInventory(InventoryRefresh.Items);
                fastObjectListViewInventory.SetObjects(_manager.Items);
            }
            else if (tabControlMain.SelectedTab == tabPagePokedex)
            {
                _manager.UpdateInventory(InventoryRefresh.Pokedex);
                fastObjectListViewPokedex.SetObjects(_manager.DracoDex);
            }
            else if (tabControlMain.SelectedTab == tabPageStats)
            {
                _manager.UpdateInventory(InventoryRefresh.Stats);
                DisplayDetails();
            }
        }

        private void CopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int total = fastObjectListViewLogs.SelectedObjects.Count;

            if (total == 0)
            {
                return;
            }

            string copiedMessage = String.Join(Environment.NewLine, fastObjectListViewLogs.SelectedObjects.Cast<Log>().Select(x => x.ToString()));

            Clipboard.SetText(copiedMessage);
        }

        private async void ExportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "Json Files (*.json)|*.json|All Files (*.*)|*.*";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    MethodResult result = await _manager.ExportLogs(sfd.FileName);

                    if (result.Success)
                    {
                        MessageBox.Show("Logs exported");
                    }
                }
            }
        }

        private async void SetFavoriteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            favoriteToolStripMenuItem.Enabled = false;

            await _manager.FavoriteCreature(fastObjectListViewCreature.SelectedObjects.Cast<FUserCreature>(), true);

            DisplayDetails();

            favoriteToolStripMenuItem.Enabled = true;

            fastObjectListViewCreature.SetObjects(_manager.Creature);

            MessageBox.Show("Finished favoriting Creature");
        }

        private async void SetUnfavoriteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            favoriteToolStripMenuItem.Enabled = false;

            await _manager.FavoriteCreature(fastObjectListViewCreature.SelectedObjects.Cast<FUserCreature>(), false);

            DisplayDetails();

            favoriteToolStripMenuItem.Enabled = true;

            fastObjectListViewCreature.SetObjects(_manager.Creature);

            MessageBox.Show("Finished unfavoriting Creature");
        }

        private async void RecycleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string data = Prompt.ShowDialog("Amount to recycle", "Set recycle amount");

            int amount;
            if (String.IsNullOrEmpty(data) || !Int32.TryParse(data, out amount) || amount <= 0)
            {
                return;
            }

            foreach (FBagItem item in fastObjectListViewInventory.SelectedObjects)
            {
                int toDelete = amount;

                if (amount > item.count)
                {
                    toDelete = item.count;
                }

                await _manager.RecycleItem(item.type, toDelete);

                await Task.Delay(500);
            }

            _manager.UpdateInventory(InventoryRefresh.Items);

            fastObjectListViewInventory.SetObjects(_manager.Items);

            MessageBox.Show("Finished recycling items");
        }

        private void CopyStackTraceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var log = fastObjectListViewLogs.SelectedObject as Log;

            if (log == null || String.IsNullOrEmpty(log.StackTrace))
            {
                return;
            }

            Clipboard.SetText(log.StackTrace);

            MessageBox.Show("Stack trace copied");
        }

        private void ShowFutureTransfersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showFutureTransfersToolStripMenuItem.Enabled = false;

            MethodResult<List<FUserCreature>> result = _manager.GetCreatureToTransfer();

            if (result.Success)
            {
                fastObjectListViewCreature.SetObjects(result.Data);
            }
            else
            {
                MessageBox.Show("Failed to get Creature to be transfered");
            }

            showFutureTransfersToolStripMenuItem.Enabled = true;
        }

        private void FastObjectListViewEggs_FormatCell(object sender, FormatCellEventArgs e)
        {

            var egg = (FEgg)e.Model;
            var eggIncubator = new FIncubator();

            foreach (var inc in _manager.Incubators)
            {
                if (inc.eggId == egg.id)
                    eggIncubator = inc;
            }

            if (e.Column == olvColumnEggWalked)
            {
                if (eggIncubator.eggId != null)
                {
                    if (egg.isEggForRoost)
                    {
                        DateTime time = new DateTime(egg.totalIncubationTime);
                        e.SubItem.Text = String.Format("{0}h", time.ToString("t"));
                    }
                    else
                        e.SubItem.Text = String.Format("{0:0.00} km", (egg.totalDistance - egg.passedDistance) / 1000);
                }
                else
                    e.SubItem.Text = "0.00 km";
            }
            else if (e.Column == olvColumnEggDistance)
            {
                if (egg.isEggForRoost)
                {
                    DateTime time = new DateTime(egg.totalIncubationTime);
                    e.SubItem.Text = String.Format("{0}h", time.ToString("t"));
                }
                else
                    e.SubItem.Text = String.Format("{0:0.00}km", egg.totalDistance / 1000);
            }
            else if (e.Column == olvColumnEggIncubator)
            {
                e.SubItem.Text = String.Format("{0}", egg.incubatorId);

                if (egg.isEggForRoost)
                    e.SubItem.ForeColor = Color.Blue;
            }
        }

        private async void SetABuddyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fastObjectListViewCreature.SelectedObjects.Cast<FUserCreature>().Count() > 1)
            {
                MessageBox.Show(String.Format("Select one Creature to set a buddy you have set {0} Creatures.", fastObjectListViewCreature.SelectedObjects.Cast<FUserCreature>().Count()), "Information", MessageBoxButtons.OK);

                return;
            }

            DialogResult result = MessageBox.Show(String.Format("Are you sure you want to set a buddy {0} Creature?", _manager.Strings.GetCreatureName(fastObjectListViewCreature.SelectedObjects.Cast<FUserCreature>().FirstOrDefault().name)), "Confirmation", MessageBoxButtons.YesNo);

            if (result != DialogResult.Yes)
            {
                return;
            }

            contextMenuStripCreatureDetails.Enabled = false;

            MethodResult managerResult = await _manager.SetBuddyCreature(fastObjectListViewCreature.SelectedObjects.Cast<FUserCreature>().FirstOrDefault());

            DisplayDetails();

            contextMenuStripCreatureDetails.Enabled = true;

            fastObjectListViewCreature.SetObjects(_manager.Creature);

            MessageBox.Show("Finished set a buddy Creature");
        }

        private async void RenameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(String.Format("Are you sure you want to rename {0} Creature(s)?", fastObjectListViewCreature.SelectedObjects.Cast<FUserCreature>().Count()), "Confirmation", MessageBoxButtons.YesNo);

            if (result != DialogResult.Yes)
            {
                return;
            }

            contextMenuStripCreatureDetails.Enabled = false;

            MethodResult managerResult = await _manager.RenameCreature(fastObjectListViewCreature.SelectedObjects.Cast<FUserCreature>());

            DisplayDetails();

            contextMenuStripCreatureDetails.Enabled = true;

            fastObjectListViewCreature.SetObjects(_manager.Creature);

            MessageBox.Show("Finished to rename Creature(s)");
        }
    }
}
