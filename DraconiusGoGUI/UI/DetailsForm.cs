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

            olvColumnPokedexId.AspectGetter = (entry) => _manager.Strings.GetCreatureName((entry as FCreadexEntry).name.ToString());

            olvColumnPokedexFriendlyName.AspectGetter = (entry) => (int)(entry as FCreadexEntry).name;

            #endregion

            #region Creature


            olvColumnCreatureId.AspectGetter = (Creature) => (Creature as FUserCreature).id;

            //olvColumnCreatureFavorite.AspectGetter = (Creature) => (Creature as CreatureData).Favorite == 1; 

            //olvColumnCreatureShiny.AspectGetter = (Creature) => (Creature as CreatureData).CreatureDisplay.Shiny;

            //olvColumnCreatureGender.AspectGetter = (Creature) => (Creature as CreatureData).CreatureDisplay.Gender;

            /*
            olvColumnCreatureRarity.AspectGetter = delegate (object Creature)
            {
                CreatureSettings CreatureSettings = _manager.GetCreatureSetting((Creature as CreatureData).CreatureId).Data;
                return CreatureSettings == null ? CreatureRarity.Normal : CreatureSettings.Rarity;
            };

            olvColumnCandyToEvolve.AspectGetter = delegate (object Creature)
            {
                //CreatureSettings CreatureSettings = _manager.GetCreatureSetting((Creature as CreatureData).CreatureId).Data;
                return CreatureSettings == null ? 0 : CreatureSettings.EvolutionBranch.Select(x => x.CandyCost).FirstOrDefault();
            };

            olvColumnCreatureCandy.AspectGetter = delegate (object Creature)
            {
                if (!_manager.CreatureCandy.Any())
                {
                    return 0;
                }

                CreatureSettings settings = _manager.GetCreatureSetting((Creature as CreatureData).CreatureId).Data;

                if (settings == null)
                {
                    return 0;
                }

                Candy family = _manager.CreatureCandy.FirstOrDefault(y => y.FamilyId == settings.FamilyId);

                return family == null ? 0 : family.Candy_;
            };
            */
            olvColumnCreatureName.AspectGetter = delegate (object Creature)
            {
               return _manager.Strings.GetCreatureName((Creature as FUserCreature).name.ToString());
            };
            /*
            olvColumnPrimaryMove.AspectGetter = (Creature) => ((CreatureMove)(Creature as CreatureData).Move1).ToString().Replace("Fast", "");

            olvColumnSecondaryMove.AspectGetter = (Creature) => ((CreatureMove)(Creature as CreatureData).Move2).ToString();

            olvColumnAttack.AspectGetter = (Creature) => (Creature as CreatureData).IndividualAttack;
            olvColumnDefense.AspectGetter = (Creature) => (Creature as CreatureData).IndividualDefense;
            olvColumnStamina.AspectGetter = (Creature) => (Creature as CreatureData).IndividualStamina;


            olvColumnPerfectPercent.AspectGetter = delegate (object Creature)
            {
                double settings = Manager.CalculateIVPerfection(Creature as CreatureData);
                string sDouble = String.Format("{0:0.00}", settings);
                return double.Parse(sDouble);
            };

            olvColumnCreatureHeight.AspectGetter = delegate (object Creature)
            {
                return String.Format("{0:0.00}m", (Creature as CreatureData).HeightM);
            };

            olvColumnCreatureWeight.AspectGetter = delegate (object Creature)
            {
                return String.Format("{0:0.00}Kg", (Creature as CreatureData).WeightKg);
            };

            #endregion

            #region Candy

            olvColumnCandyFamily.AspectGetter = delegate (object x)
            {
                var family = (Candy)x;

                return family.FamilyId.ToString().Replace("Family", "");
            };
            */
            #endregion
            
            #region Inventory

            olvColumnInventoryItem.AspectGetter = delegate (object x)
            {
                var item = (FBagItem)x;

                return _manager.Strings.GetItemName(item.type.ToString());
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

            /*
            if (_manager.Stats != null)
            {
                labelDistanceWalked.Text = String.Format("{0:0.00}km", _manager.Stats.KmWalked);
                labelCreatureCaught.Text = _manager.Stats.CreaturesCaptured.ToString();
                labelPokestopVisits.Text = _manager.Stats.PokeStopVisits.ToString();
                labelUniqueCreature.Text = _manager.Stats.UniquePokedexEntries.ToString();
            }
            */
            if (_manager.Creature != null)
            {
                labelCreatureCount.Text = String.Format("{0}/{1}", _manager.Creature.Count + _manager.Eggs.Count, _manager.MaxCreatureStorage);
                labelDeployedCreatures.Text = _manager.Creature.Where(i => i.isArenaDefender).Count().ToString();
            }

            if (_manager.Items != null)
            {
                labelInventoryCount.Text = String.Format("{0}/{1}", _manager.Items.Sum(x => x.count), _manager.MaxItemStorage);
            }
            /*
            if (_manager.PlayerData != null)
            {
                //BuddyCreature buddy = _manager.PlayerData.BuddyCreature ?? new BuddyCreature();
                //CreatureData myBuddy = _manager.Creature.Where(x => x.Id == buddy.Id).FirstOrDefault() ?? new CreatureData();
                //labelCreatureBuddy.Text = myBuddy.CreatureId != CreatureId.Missingno ? String.Format("{0}", myBuddy.CreatureId) : "Not set";
                labelPlayerUsername.Text = _manager.PlayerData.Username;
                DateTime date = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(_manager.PlayerData.CreationTimestampMs);
                labelCreateDate.Text = date.ToString();
                string tutocompleted = "Not Completed";
                if (_manager.PlayerData.TutorialState.Contains(TutorialState.AccountCreation)
                    && _manager.PlayerData.TutorialState.Contains(TutorialState.AvatarSelection)
                    && _manager.PlayerData.TutorialState.Contains(TutorialState.FirstTimeExperienceComplete)
                    && _manager.PlayerData.TutorialState.Contains(TutorialState.GymTutorial)
                    && _manager.PlayerData.TutorialState.Contains(TutorialState.LegalScreen)
                    && _manager.PlayerData.TutorialState.Contains(TutorialState.NameSelection)
                    && _manager.PlayerData.TutorialState.Contains(TutorialState.CreatureBerry)
                    && _manager.PlayerData.TutorialState.Contains(TutorialState.CreatureCapture)
                    && _manager.PlayerData.TutorialState.Contains(TutorialState.PokestopTutorial)
                    && _manager.PlayerData.TutorialState.Contains(TutorialState.UseItem)
                    )
                    tutocompleted = "Completed";
                    
                labelTutorialState.Text = tutocompleted;
            }
            */
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
                //bool fav = (bool)olvColumnCreatureFavorite.GetValue(CreatureData);
                //bool bubby = _manager?.PlayerData?.BuddyCreature?.Id == CreatureData.Id == true;
                //if (fav)
                //{
                //    e.SubItem.ForeColor = Color.Gold;
                //}
                //else if (bubby)
                //{
                //    e.SubItem.ForeColor = Color.Blue;
                //}
                //else if (!String.IsNullOrEmpty(CreatureData.DeployedFortId))
                //{
                    //deployed
                //    e.SubItem.ForeColor = Color.LightGreen;
                //}
            }
            else if (e.Column == olvColumnCreatureCandy)
            {
                /*
                int candy = (int)olvColumnCreatureCandy.GetValue(CreatureData);
                int candyToEvolve = (int)olvColumnCandyToEvolve.GetValue(CreatureData);

                if (candyToEvolve > 0)
                {
                    e.SubItem.ForeColor = candy >= candyToEvolve ? Color.Green : Color.Red;
                }
                */
            }
            else if (e.Column == olvColumnPerfectPercent)
            {
                double perfectPercent = Convert.ToDouble(olvColumnPerfectPercent.GetValue(CreatureData));

                if (perfectPercent >= 93)
                {
                    e.SubItem.ForeColor = Color.Green;
                }
                else if (perfectPercent >= 86)
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
                /*
                if (CreatureData.IndividualAttack >= 13)
                {
                    e.SubItem.ForeColor = Color.Green;
                }
                else if (CreatureData.IndividualAttack >= 11)
                {
                    e.SubItem.ForeColor = Color.Yellow;
                }
                else if (CreatureData.IndividualAttack >= 9)
                {
                    e.SubItem.ForeColor = Color.Orange;
                }
                else
                {
                    e.SubItem.ForeColor = Color.Red;
                }
                */

            }
            else if (e.Column == olvColumnDefense)
            {
                /*
                if (CreatureData.IndividualDefense >= 13)
                {
                    e.SubItem.ForeColor = Color.Green;
                }
                else if (CreatureData.IndividualDefense >= 11)
                {
                    e.SubItem.ForeColor = Color.Yellow;
                }
                else if (CreatureData.IndividualDefense >= 9)
                {
                    e.SubItem.ForeColor = Color.Orange;
                }
                else
                {
                    e.SubItem.ForeColor = Color.Red;
                }
                */
            }
            else if (e.Column == olvColumnStamina)
            {
                /*
                if (CreatureData.IndividualStamina >= 13)
                {
                    e.SubItem.ForeColor = Color.Green;
                }
                else if (CreatureData.IndividualStamina >= 11)
                {
                    e.SubItem.ForeColor = Color.Yellow;
                }
                else if (CreatureData.IndividualStamina >= 9)
                {
                    e.SubItem.ForeColor = Color.Orange;
                }
                else
                {
                    e.SubItem.ForeColor = Color.Red;
                }
                */
            }
        }

        private async void UpgradeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*
            DialogResult result = MessageBox.Show(String.Format("Are you sure you want to upgrade {0} Creature?", fastObjectListViewCreature.SelectedObjects.Cast<CreatureData>().Count()), "Confirmation", MessageBoxButtons.YesNo);

            if (result != DialogResult.Yes)
            {
                return;
            }
           
            if (fastObjectListViewCreature.SelectedObjects.Count == 0 || fastObjectListViewCreature.SelectedObjects.Cast<CreatureData>().FirstOrDefault() == null)
                return;

            contextMenuStripCreatureDetails.Enabled = false;

            MethodResult managerResult = await _manager.UpgradeCreature(fastObjectListViewCreature.SelectedObjects.Cast<CreatureData>());

            DisplayDetails();

            contextMenuStripCreatureDetails.Enabled = true;

            fastObjectListViewCreature.SetObjects(_manager.Creature);

            MessageBox.Show("Finished upgrade Creature");
            */
        }

        private async void TransferToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*
            DialogResult result = MessageBox.Show(String.Format("Are you sure you want to transfer {0} Creature?", fastObjectListViewCreature.SelectedObjects.Count), "Confirmation", MessageBoxButtons.YesNo);

            if (result != DialogResult.Yes)
            {
                return;
            }

            if (fastObjectListViewCreature.SelectedObjects.Count == 0 || fastObjectListViewCreature.SelectedObjects.Cast<CreatureData>().FirstOrDefault() == null)
                return;

            contextMenuStripCreatureDetails.Enabled = false;

            MethodResult managerResult = await _manager.TransferCreature(fastObjectListViewCreature.SelectedObjects.Cast<CreatureData>());

            DisplayDetails();

            contextMenuStripCreatureDetails.Enabled = true;

            fastObjectListViewCreature.SetObjects(_manager.Creature);

            MessageBox.Show("Finished transferring Creature");
            */
        }

        private async void EvolveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*
            DialogResult result = MessageBox.Show(String.Format("Are you sure you want to evolve {0} Creature?", fastObjectListViewCreature.SelectedObjects.Count), "Confirmation", MessageBoxButtons.YesNo);

            if (result != DialogResult.Yes)
            {
                return;
            }

            if (fastObjectListViewCreature.SelectedObjects.Count == 0 || fastObjectListViewCreature.SelectedObjects.Cast<CreatureData>().FirstOrDefault() == null)
                return;

            contextMenuStripCreatureDetails.Enabled = false;

            await _manager.EvolveCreature(fastObjectListViewCreature.SelectedObjects.Cast<CreatureData>());

            DisplayDetails();

            contextMenuStripCreatureDetails.Enabled = true;

            fastObjectListViewCreature.SetObjects(_manager.Creature);

            MessageBox.Show("Finished evolving Creature");
            */
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
                //fastObjectListViewCandy.SetObjects(_manager.CreatureCandy);
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
                fastObjectListViewPokedex.SetObjects(_manager.Pokedex);
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

            //await _manager.FavoriteCreature(fastObjectListViewCreature.SelectedObjects.Cast<CreatureData>(), true);

            DisplayDetails();

            favoriteToolStripMenuItem.Enabled = true;

            //fastObjectListViewCreature.SetObjects(_manager.Creature);

            MessageBox.Show("Finished favoriting Creature");
        }

        private async void SetUnfavoriteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            favoriteToolStripMenuItem.Enabled = false;

            //await _manager.FavoriteCreature(fastObjectListViewCreature.SelectedObjects.Cast<CreatureData>(), false);

            DisplayDetails();

            favoriteToolStripMenuItem.Enabled = true;

            //fastObjectListViewCreature.SetObjects(_manager.Creature);

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

            /*foreach (ItemData item in fastObjectListViewInventory.SelectedObjects)
            {
                int toDelete = amount;

                if (amount > item.Count)
                {
                    toDelete = item.Count;
                }

                await _manager.RecycleItem(item, toDelete);

                await Task.Delay(500);
            }*/

            //_manager.UpdateInventory(InventoryRefresh.Items);

            //fastObjectListViewInventory.SetObjects(_manager.Items);

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

            /*MethodResult<List<CreatureData>> result = _manager.GetCreatureToTransfer();

            if (result.Success)
            {
                fastObjectListViewCreature.SetObjects(result.Data);
            }
            else
            {
                MessageBox.Show("Failed to get Creature to be transfered");
            }*/

            showFutureTransfersToolStripMenuItem.Enabled = true;
        }

        private void FastObjectListViewEggs_FormatCell(object sender, FormatCellEventArgs e)
        {
            /*
            var egg = (CreatureData)e.Model;
            var eggIncubator = new EggIncubator();

            foreach (var inc in _manager.Incubators)
            {
                if (inc.CreatureId == egg.Id)
                    eggIncubator = inc;
            }

            if (e.Column == olvColumnEggWalked)
            {
                if (eggIncubator.CreatureId != 0)
                    e.SubItem.Text = String.Format("{0:0.00} km", _manager.Stats.KmWalked - eggIncubator.StartKmWalked);
                else
                    e.SubItem.Text = "0.00 km";
            }
            else if (e.Column == olvColumnEggDistance)
                e.SubItem.Text = String.Format("{0:0.00}km", egg.EggKmWalkedTarget);
            */                
        }

        private async void SetABuddyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*if (fastObjectListViewCreature.SelectedObjects.Cast<CreatureData>().Count() > 1)
            {
                MessageBox.Show(String.Format("Select one Creature to set a buddy you have set {0} Creatures.", fastObjectListViewCreature.SelectedObjects.Cast<CreatureData>().Count()), "Information", MessageBoxButtons.OK);

                return;
            }

            DialogResult result = MessageBox.Show(String.Format("Are you sure you want to set a buddy {0} Creature?", fastObjectListViewCreature.SelectedObjects.Cast<CreatureData>().FirstOrDefault().CreatureId), "Confirmation", MessageBoxButtons.YesNo);

            if (result != DialogResult.Yes)
            {
                return;
            }

            contextMenuStripCreatureDetails.Enabled = false;

            MethodResult managerResult = await _manager.SetBuddyCreature(fastObjectListViewCreature.SelectedObjects.Cast<CreatureData>().FirstOrDefault());

            DisplayDetails();

            contextMenuStripCreatureDetails.Enabled = true;

            fastObjectListViewCreature.SetObjects(_manager.Creature);

            MessageBox.Show("Finished set a buddy Creature");
            */
        }

        private async void RenameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*
            DialogResult result = MessageBox.Show(String.Format("Are you sure you want to rename {0} Creature(s)?", fastObjectListViewCreature.SelectedObjects.Cast<CreatureData>().Count()), "Confirmation", MessageBoxButtons.YesNo);

            if (result != DialogResult.Yes)
            {
                return;
            }

            contextMenuStripCreatureDetails.Enabled = false;

            MethodResult managerResult = await _manager.RenameCreature(fastObjectListViewCreature.SelectedObjects.Cast<CreatureData>());

            DisplayDetails();

            contextMenuStripCreatureDetails.Enabled = true;

            fastObjectListViewCreature.SetObjects(_manager.Creature);

            MessageBox.Show("Finished to rename Creature(s)");
            */
        }
    }
}
