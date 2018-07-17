using DraconiusGoGUI.Enums;
using DraconiusGoGUI.Extensions;
using DraconiusGoGUI.DracoManager;
using DraconiusGoGUI.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using DracoProtos.Core.Base;

namespace DraconiusGoGUI.UI
{
    public partial class AccountSettingsForm : System.Windows.Forms.Form
    {
        private Manager _manager;

        public bool AutoUpdate { get; set; }

        public AccountSettingsForm(Manager manager)
        {
            InitializeComponent();

            _manager = manager;

            #region Catching
            /*
            olvColumnCatchId.AspectGetter = delegate (object x)
            {
                var setting = (CatchSetting)x;

                return (int)setting.Id;
            };
            */
            #endregion

            #region Evolving
            /*
            olvColumnEvolveId.AspectGetter = delegate (object x)
            {
                var setting = (EvolveSetting)x;

                return (int)setting.Id;
            };
            */
            #endregion

            #region Transfer
            /*
            olvColumnTransferId.AspectGetter = delegate (object x)
            {
                var setting = (TransferSetting)x;

                return (int)setting.Id;
            };
            */

            #endregion

            #region Upgrade
            /*
            olvColumnUpgradeId.AspectGetter = delegate (object x)
            {
                var setting = (UpgradeSetting)x;

                return (int)setting.Id;
            };
            */
            #endregion

        }

        private void AccountSettingsForm_Load(object sender, EventArgs e)
        {
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            foreach (AccountState state in Enum.GetValues(typeof(AccountState)))
            {
                if (state == AccountState.Good || state == AccountState.Conecting)
                {
                    continue;
                }

                comboBoxMinAccountState.Items.Add(state);
            }

            foreach (AllianceType team in Enum.GetValues(typeof(AllianceType)))
            {
                cbTeam.Items.Add(team.ToString());
            }

            UpdateDetails(_manager.UserSettings);

            UpdateListViews();

            comboBoxLocationPresets.DataSource = _manager.FarmLocations;
            comboBoxLocationPresets.DisplayMember = "Name";
         }

        private void UpdateListViews()
        {
            fastObjectListViewRecycling.SetObjects(_manager.UserSettings.ItemSettings);
            fastObjectListViewCatch.SetObjects(_manager.UserSettings.CatchSettings);
            fastObjectListViewEvolve.SetObjects(_manager.UserSettings.EvolveSettings);
            fastObjectListViewTransfer.SetObjects(_manager.UserSettings.TransferSettings);
            fastObjectListViewUpgrade.SetObjects(_manager.UserSettings.UpgradeSettings);
        }

        private void UpdateDetails(Settings settings)
        {
            textBoxPtcPassword.Text = settings.Password;
            textBoxPtcUsername.Text = settings.Username;
            textBoxLat.Text = settings.Latitude.ToString();
            textBoxLong.Text = settings.Longitude.ToString();
            textBoxName.Text = settings.AccountName;
            textBoxWalkSpeed.Text = settings.WalkingSpeed.ToString();
            textBoxCreatureBeforeEvolve.Text = settings.MinCreatureBeforeEvolve.ToString();
            textBoxMaxLevel.Text = settings.MaxLevel.ToString();
            textBoxPercTransItems.Text = settings.PercTransItems.ToString();
            textBoxPercTransPoke.Text = settings.PercTransPoke.ToString();
            textBoxProxy.Text = settings.Proxy.ToString();
            checkBoxRecycle.Checked = settings.RecycleItems;
            checkBoxEvolve.Checked = settings.EvolveCreature;
            checkBoxTransfers.Checked = settings.TransferCreature;
            checkBoxTransferSlashCreatures.Checked = settings.TransferSlashCreatures;
            checkBoxUseLuckyEgg.Checked = settings.UseLuckyEgg;
            checkBoxIncubateEggs.Checked = settings.IncubateEggs;
            checkBoxOnlyUnlimitedIncubator.Checked = settings.OnlyUnlimitedIncubator;
            checkBoxCatchCreature.Checked = settings.CatchCreature;
            numericUpDownRunForHours.Value = new Decimal(settings.RunForHours);
            numericUpDownMaxLogs.Value = settings.MaxLogs;
            numericUpDownMaxFailBeforeReset.Value = settings.MaxFailBeforeReset;
            checkBoxStopOnIPBan.Checked = settings.StopOnIPBan;
            checkBoxAutoRotateProxies.Checked = settings.AutoRotateProxies;
            checkBoxRemoveOnStop.Checked = settings.AutoRemoveOnStop;
            checkBoxClaimLevelUp.Checked = settings.ClaimLevelUpRewards;
            numericUpDownSearchFortBelow.Value = new Decimal(settings.SearchFortBelowPercent);
            numericUpDownCreaturesDayLimit.Value = new Decimal(settings.CatchCreatureDayLimit);
            numericUpDownStopsDayLimit.Value = new Decimal(settings.SpinPokestopsDayLimit);
            numericUpDownForceEvolveAbove.Value = new Decimal(settings.ForceEvolveAbovePercent);
            checkBoxStopOnAPIUpdate.Checked = settings.StopOnAPIUpdate;

            if (!string.IsNullOrEmpty(settings.DefaultTeam) && settings.DefaultTeam != "Neutral")
            {
                checkBoxSpinGyms.Enabled = true;
                checkBoxSpinGyms.Checked = settings.SpinGyms;
                checkBoxDeployToGym.Enabled = true;
                checkBoxDeployToGym.Checked = settings.DeployCreature;
                checkBoxGoToGymsOnly.Enabled = true;
                checkBoxGoToGymsOnly.Checked = settings.GoOnlyToGyms;
            }
            else
            {
                checkBoxSpinGyms.Enabled = false;
                checkBoxSpinGyms.Checked = false;
                checkBoxDeployToGym.Enabled = false;
                checkBoxDeployToGym.Checked = false;
                checkBoxGoToGymsOnly.Enabled = false;
                checkBoxGoToGymsOnly.Checked = false;
            }

            cbUseIncense.Checked = settings.UseIncense;
            cbUseLuckEggConst.Checked = settings.UseLuckEggConst;
            checkBoxReqFortDetails.Checked = settings.RequestFortDetails;

            numericUpDownWalkingOffset.Value = new Decimal(settings.WalkingSpeedOffset);

            //Device settings
            textBoxDeviceId.Text = settings.DeviceId;
            textBoxDeviceModel.Text = settings.DeviceModel;
            textBoxDeviceBrand.Text = settings.DeviceBrand;
            textBoxDeviceModelBoot.Text = settings.DeviceModelBoot;
            textBoxFirmwareBrand.Text = settings.FirmwareBrand;
            textBoxFirmwareType.Text = settings.FirmwareType;
            textBoxHardwareManufacturer.Text = settings.HardwareManufacturer;
            textBoxHardwareModel.Text = settings.HardwareModel;
            //End device settings

            checkBoxUseBerries.Checked = settings.UseBerries;
            checkBoxGetARBonus.Checked = settings.GetArBonus;
            checkBoxCompleteTutorial.Checked = settings.CompleteTutorial;
            checkBoxTransferAtOnce.Checked = settings.TransferAtOnce;
            checkBoxUpgradeCreatures.Checked = settings.UpgradeCreature;
            cbAutoUpdate.Checked = AutoUpdate;
            numericUpDownDisableCatchDelay.Value = new Decimal(_manager.UserSettings.DisableCatchDelay);

            checkBoxAutoFavShiny.Checked = settings.AutoFavoritShiny;
            checkBoxSniperNoInPokedex.Checked = settings.SnipeAllCreaturesNoInPokedex;
            checkBoxTooBalls.Checked = settings.IgnoreStopsIfTooBalls;
            numericUpDownTooBalls.Value = new Decimal(settings.BallsToIgnoreStops);
            checkBoxSoftBypass.Checked = settings.UseSoftBanBypass;
            numericUpDownSoftBypass.Value = new Decimal(settings.SoftBanBypassTimes);
            numericUpDownLvForConsLukky.Value = new Decimal(settings.LevelForConstLukky);

            for (int i = 0; i < comboBoxMinAccountState.Items.Count; i++)
            {
                if ((AccountState)comboBoxMinAccountState.Items[i] == settings.StopAtMinAccountState)
                {
                    comboBoxMinAccountState.SelectedIndex = i;
                    break;
                }
            }

            for (int i = 0; i < cbTeam.Items.Count; i++)
            {
                if (cbTeam.Items[i].ToString() == settings.DefaultTeam)
                {
                    cbTeam.SelectedIndex = i;
                    if (cbTeam.SelectedItem.ToString() != "Neutral" && !string.IsNullOrEmpty(settings.DefaultTeam))
                        cbTeam.Enabled = false;
                    break;
                }
            }
        }

        private void CheckBoxMimicWalking_CheckedChanged(object sender, EventArgs e)
        {
            textBoxWalkSpeed.Enabled = false;
        }

        private void ButtonSave_Click(object sender, EventArgs e)
        {
            if (SaveSettings())
            {
                MessageBox.Show("Settings saved.\nSome settings won't take effect until the account stops running.", "Info");
            }
        }

        private bool SaveSettings()
        {
            Settings userSettings = _manager.UserSettings;

            ProxyEx proxyEx = null;

            int maxLevel;
            if (!Int32.TryParse(textBoxMaxLevel.Text, out maxLevel) || maxLevel < 0)
            {
                MessageBox.Show("Invalid Max level", "Warning");
                return false;
            }

            int perctransitems;
            if (!Int32.TryParse(textBoxPercTransItems.Text, out perctransitems) || perctransitems < 0)
            {
                MessageBox.Show("Invalid % Transfer Items", "Warning");
                return false;
            }

            int perctranspoke;
            if (!Int32.TryParse(textBoxPercTransPoke.Text, out perctranspoke) || perctranspoke < 0)
            {
                MessageBox.Show("Invalid % Transfer Creature", "Warning");
                return false;
            }

            int minCreatureBeforeEvolve;
            if (!Int32.TryParse(textBoxCreatureBeforeEvolve.Text, out minCreatureBeforeEvolve) || minCreatureBeforeEvolve < 0)
            {
                MessageBox.Show("Invalid Creature before evolve", "Warning");
                return false;
            }
            int walkingSpeed;
            if (!Int32.TryParse(textBoxWalkSpeed.Text, out walkingSpeed) || walkingSpeed <= 0)
            {
                MessageBox.Show("Invalid walking speed", "Warning");
                return false;
            }
            double defaultLat;
            if (!Double.TryParse(textBoxLat.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out defaultLat))
            {
                MessageBox.Show("Invalid latitude", "Warning");
                return false;
            }
            double defaultLong;
            if (!Double.TryParse(textBoxLong.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out defaultLong))
            {
                MessageBox.Show("Invalid longitude", "Warning");
                return false;
            }

            if (!String.IsNullOrEmpty(textBoxProxy.Text) && !ProxyEx.TryParse(textBoxProxy.Text, out proxyEx))
            {
                MessageBox.Show("Invalid proxy format", "Warning");
                return false;
            }

            if (comboBoxMinAccountState.SelectedItem == null)
            {
                MessageBox.Show("Please select a valid min account state", "Warning");
                return false;
            }

            if (walkingSpeed < (double)numericUpDownWalkingOffset.Value)
            {
                MessageBox.Show("Walking offset must be more than walking speed", "Warning");
                return false;
            }

            if (String.IsNullOrEmpty(textBoxPtcUsername.Text))
            {
                MessageBox.Show("Invalid Username", "Warning");
                return false;
            }
            if (String.IsNullOrEmpty(textBoxPtcPassword.Text))
            {
                MessageBox.Show("Invalid Password", "Warning");
                return false;
            }

            userSettings.AuthType = textBoxPtcUsername.Text.Contains("@") ? AuthType.GOOGLE : AuthType.DEVICE;

            userSettings.Username = textBoxPtcUsername.Text.Trim();
            userSettings.Password = textBoxPtcPassword.Text.Trim();
            userSettings.Latitude = defaultLat;
            userSettings.Longitude = defaultLong;
            userSettings.WalkingSpeed = walkingSpeed;
            userSettings.AccountName = textBoxName.Text;
            userSettings.TransferCreature = checkBoxTransfers.Checked;
            userSettings.TransferSlashCreatures = checkBoxTransferSlashCreatures.Checked;
            userSettings.EvolveCreature = checkBoxEvolve.Checked;
            userSettings.RecycleItems = checkBoxRecycle.Checked;
            userSettings.MinCreatureBeforeEvolve = minCreatureBeforeEvolve;
            userSettings.UseLuckyEgg = checkBoxUseLuckyEgg.Checked;
            userSettings.IncubateEggs = checkBoxIncubateEggs.Checked;
            userSettings.OnlyUnlimitedIncubator = checkBoxOnlyUnlimitedIncubator.Checked;
            userSettings.MaxLevel = maxLevel;
            userSettings.PercTransItems = perctransitems;
            userSettings.PercTransPoke = perctranspoke;
            userSettings.CatchCreature = checkBoxCatchCreature.Checked;
            userSettings.StopAtMinAccountState = (AccountState)comboBoxMinAccountState.SelectedItem;
            userSettings.SearchFortBelowPercent = (double)numericUpDownSearchFortBelow.Value;
            userSettings.CatchCreatureDayLimit = (int)numericUpDownCreaturesDayLimit.Value;
            userSettings.SpinPokestopsDayLimit = (int)numericUpDownStopsDayLimit.Value;
            userSettings.ForceEvolveAbovePercent = (double)numericUpDownForceEvolveAbove.Value;
            userSettings.ClaimLevelUpRewards = checkBoxClaimLevelUp.Checked;
            userSettings.StopOnAPIUpdate = checkBoxStopOnAPIUpdate.Checked;
            userSettings.SpinGyms = checkBoxSpinGyms.Checked;
            userSettings.DeployCreature = checkBoxDeployToGym.Checked;
            AutoUpdate = cbAutoUpdate.Checked;
            userSettings.UseBerries = checkBoxUseBerries.Checked;
            userSettings.DisableCatchDelay = (int)numericUpDownDisableCatchDelay.Value;
            userSettings.UseIncense = cbUseIncense.Checked;
            userSettings.UseLuckEggConst = cbUseLuckEggConst.Checked;
            userSettings.RunForHours = (double)numericUpDownRunForHours.Value;
            userSettings.MaxLogs = (int)numericUpDownMaxLogs.Value;
            userSettings.StopOnIPBan = checkBoxStopOnIPBan.Checked;
            userSettings.MaxFailBeforeReset = (int)numericUpDownMaxFailBeforeReset.Value;
            userSettings.AutoRotateProxies = checkBoxAutoRotateProxies.Checked;
            userSettings.AutoRemoveOnStop = checkBoxRemoveOnStop.Checked;
            userSettings.RequestFortDetails = checkBoxReqFortDetails.Checked;
            userSettings.IgnoreStopsIfTooBalls = checkBoxTooBalls.Checked;
            userSettings.BallsToIgnoreStops = (int)numericUpDownTooBalls.Value;

            userSettings.WalkingSpeedOffset = (double)numericUpDownWalkingOffset.Value;

            //Device settings
            userSettings.DeviceId = textBoxDeviceId.Text;
            userSettings.DeviceModel = textBoxDeviceModel.Text;
            userSettings.DeviceBrand = textBoxDeviceBrand.Text;
            userSettings.DeviceModelBoot = textBoxDeviceModelBoot.Text;
            userSettings.HardwareManufacturer = textBoxHardwareManufacturer.Text;
            userSettings.HardwareModel = textBoxHardwareModel.Text;
            userSettings.FirmwareBrand = textBoxFirmwareBrand.Text;
            userSettings.FirmwareType = textBoxFirmwareType.Text;
            //End device settings

            userSettings.GetArBonus = checkBoxGetARBonus.Checked;
            userSettings.CompleteTutorial = checkBoxCompleteTutorial.Checked;
            userSettings.TransferAtOnce = checkBoxTransferAtOnce.Checked;

            if (proxyEx != null)
            {
                userSettings.ProxyIP = proxyEx.Address;
                userSettings.ProxyPort = proxyEx.Port;
                userSettings.ProxyUsername = proxyEx.Username;
                userSettings.ProxyPassword = proxyEx.Password;
            }
            else
            {
                userSettings.ProxyUsername = null;
                userSettings.ProxyPassword = null;
                userSettings.ProxyIP = null;
                userSettings.ProxyPort = 0;
            }


            userSettings.DefaultTeam = (string)cbTeam.SelectedItem ?? "Neutral";
            userSettings.GoOnlyToGyms = checkBoxGoToGymsOnly.Checked;
            userSettings.UpgradeCreature = checkBoxUpgradeCreatures.Checked;
            userSettings.AutoFavoritShiny = checkBoxAutoFavShiny.Checked;
            userSettings.SnipeAllCreaturesNoInPokedex = checkBoxSniperNoInPokedex.Checked;
            userSettings.UseSoftBanBypass = checkBoxSoftBypass.Checked;

            int softbanbypass;
            if (!Int32.TryParse(numericUpDownSoftBypass.Text, out softbanbypass))
            {
                MessageBox.Show("SoftBan Bypass value", "Warning");
                return false;
            }
            userSettings.SoftBanBypassTimes = softbanbypass;

            int lvforconslukky;
            if (!Int32.TryParse(numericUpDownLvForConsLukky.Text, out lvforconslukky))
            {
                MessageBox.Show("Min level for use lukky constantly value", "Warning");
                return false;
            }
            userSettings.LevelForConstLukky = lvforconslukky;

            return true;
        }

        private void ButtonDone_Click(object sender, EventArgs e)
        {
            if (SaveSettings())
            {
                DialogResult = DialogResult.OK;
            }
        }

        #region Recycling

        private void EditToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int totalObjects = fastObjectListViewRecycling.SelectedObjects.Count;

            if (totalObjects == 0)
            {
                return;
            }

            var iiSettings = fastObjectListViewRecycling.SelectedObjects[0] as InventoryItemSetting;

            if (iiSettings == null)
            {
                return;
            }

            string num = Prompt.ShowDialog("Max Inventory Amount", "Edit Amount", iiSettings.MaxInventory.ToString());

            if (String.IsNullOrEmpty(num))
            {
                return;
            }
            int maxInventory;
            if (!Int32.TryParse(num, out maxInventory))
            {
                return;
            }

            foreach (InventoryItemSetting item in fastObjectListViewRecycling.SelectedObjects)
            {
                item.MaxInventory = maxInventory;
            }

            fastObjectListViewRecycling.SetObjects(_manager.UserSettings.ItemSettings);
        }

        #endregion

        #region CatchCreature

        private void TrueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var tSMI = sender as ToolStripMenuItem;

            if (tSMI == null)
            {
                return;
            }

            var checkType = (CheckType)Int32.Parse(tSMI.Tag.ToString());

            foreach (CatchSetting cSetting in fastObjectListViewCatch.SelectedObjects)
            {
                if (checkType == CheckType.Toggle)
                {
                    cSetting.Catch = !cSetting.Catch;
                }
                else if (checkType == CheckType.True)
                {
                    cSetting.Catch = true;
                }
                else
                {
                    cSetting.Catch = false;
                }
            }

            fastObjectListViewCatch.RefreshSelectedObjects();
        }

        private void TrueUsePinaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var tSMI = sender as ToolStripMenuItem;

            if (tSMI == null)
            {
                return;
            }

            var checkType = (CheckType)Int32.Parse(tSMI.Tag.ToString());

            foreach (CatchSetting cSetting in fastObjectListViewCatch.SelectedObjects)
            {
                if (checkType == CheckType.Toggle)
                {
                    cSetting.UsePinap = !cSetting.UsePinap;
                }
                else if (checkType == CheckType.True)
                {
                    cSetting.UsePinap = true;
                }
                else
                {
                    cSetting.UsePinap = false;
                }
            }

            fastObjectListViewCatch.RefreshSelectedObjects();
        }

        private void TrueLocalSnipeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var tSMI = sender as ToolStripMenuItem;

            if (tSMI == null)
            {
                return;
            }

            var checkType = (CheckType)Int32.Parse(tSMI.Tag.ToString());

            foreach (CatchSetting cSetting in fastObjectListViewCatch.SelectedObjects)
            {
                if (checkType == CheckType.Toggle)
                {
                    cSetting.Snipe = !cSetting.Snipe;
                }
                else if (checkType == CheckType.True)
                {
                    cSetting.Snipe = true;
                }
                else
                {
                    cSetting.Snipe = false;
                }
            }

            fastObjectListViewCatch.RefreshSelectedObjects();
        }

        private void RestoreDefaultsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Reset defaults?", "Confirmation", MessageBoxButtons.YesNoCancel);

            if (result != DialogResult.Yes)
            {
                return;
            }

            _manager.RestoreCatchDefaults();

            fastObjectListViewCatch.SetObjects(_manager.UserSettings.CatchSettings);
        }

        #endregion

        #region Evolving

        private void RestoreDefaultsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Reset defaults?", "Confirmation", MessageBoxButtons.YesNoCancel);

            if (result != DialogResult.Yes)
            {
                return;
            }

            _manager.RestoreEvolveDefaults();

            fastObjectListViewEvolve.SetObjects(_manager.UserSettings.EvolveSettings);
        }

        private void EditCPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int count = fastObjectListViewEvolve.SelectedObjects.Count;

            if (count == 0)
            {
                return;
            }

            int defaultCP = ((EvolveSetting)fastObjectListViewEvolve.SelectedObjects[0]).MinCP;

            string cp = Prompt.ShowDialog("Enter minimum CP:", "Edit CP", defaultCP.ToString());

            if (String.IsNullOrEmpty(cp))
            {
                return;
            }
            int changeCp;
            if (!Int32.TryParse(cp, out changeCp) || changeCp < 0)
            {
                MessageBox.Show("Invalid amount", "Warning");

                return;
            }

            foreach (EvolveSetting setting in fastObjectListViewEvolve.SelectedObjects)
            {
                setting.MinCP = changeCp;
            }

            fastObjectListViewEvolve.SetObjects(_manager.UserSettings.EvolveSettings);
        }

        private void TrueEvoleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var tSMI = sender as ToolStripMenuItem;

            if (tSMI == null)
            {
                return;
            }

            var checkType = (CheckType)Int32.Parse(tSMI.Tag.ToString());

            foreach (EvolveSetting eSetting in fastObjectListViewEvolve.SelectedObjects)
            {
                if (checkType == CheckType.Toggle)
                {
                    eSetting.Evolve = !eSetting.Evolve;
                }
                else if (checkType == CheckType.True)
                {
                    eSetting.Evolve = true;
                }
                else
                {
                    eSetting.Evolve = false;
                }
            }

            fastObjectListViewEvolve.SetObjects(_manager.UserSettings.EvolveSettings);
        }

        #endregion

        #region Transfer

        private void EditTransferSettings(List<TransferSetting> settings)
        {
            if (settings.Count == 0)
            {
                return;
            }

            var transferSettingForm = new TransferSettingsForm(settings);
            transferSettingForm.ShowDialog();

            fastObjectListViewTransfer.RefreshObjects(settings);
        }

        private void EditToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            EditTransferSettings(fastObjectListViewTransfer.SelectedObjects.Cast<TransferSetting>().ToList());
        }

        private void RestoreDefaultsToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Reset defaults?", "Confirmation", MessageBoxButtons.YesNoCancel);

            if (result != DialogResult.Yes)
            {
                return;
            }

            _manager.RestoreTransferDefaults();

            fastObjectListViewTransfer.SetObjects(_manager.UserSettings.TransferSettings);
        }

        #endregion

        #region Upgrade

        private void TrueUpgradeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var tSMI = sender as ToolStripMenuItem;

            if (tSMI == null)
            {
                return;
            }

            var checkType = (CheckType)Int32.Parse(tSMI.Tag.ToString());

            foreach (UpgradeSetting cSetting in fastObjectListViewUpgrade.SelectedObjects)
            {
                if (checkType == CheckType.Toggle)
                {
                    cSetting.Upgrade = !cSetting.Upgrade;
                }
                else if (checkType == CheckType.True)
                {
                    cSetting.Upgrade = true;
                }
                else
                {
                    cSetting.Upgrade = false;
                }
            }

            fastObjectListViewUpgrade.RefreshSelectedObjects();
        }

        private void RestoreDefaultsToolStripMenuItemUpgrade_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Reset defaults?", "Confirmation", MessageBoxButtons.YesNoCancel);

            if (result != DialogResult.Yes)
            {
                return;
            }

            _manager.RestoreUpgradeDefaults();

            fastObjectListViewUpgrade.SetObjects(_manager.UserSettings.UpgradeSettings);
        }

        #endregion

        private void ComboBoxLocationPresets_SelectedIndexChanged(object sender, EventArgs e)
        {
            var fLocation = comboBoxLocationPresets.SelectedItem as FarmLocation;
            if (fLocation != null)
            {
                if (fLocation.Name == "Current")
                {
                    textBoxLat.Text = _manager.UserSettings.Latitude.ToString();
                    textBoxLong.Text = _manager.UserSettings.Longitude.ToString();
                }
                else
                {
                    textBoxLat.Text = fLocation.Latitude.ToString();
                    textBoxLong.Text = fLocation.Longitude.ToString();
                }
            }
        }

        private async void ButtonExportConfig_Click(object sender, EventArgs e)
        {
            if (!SaveSettings())
            {
                return;
            }

            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "Json Files (*.json)|*.json|All Files (*.*)|*.*";

                if (sfd.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                SaveSettings();
                
                bool full = false;
                DialogResult dialogResult = MessageBox.Show("Export full config? ", "Info", MessageBoxButtons.YesNo);

                if (DialogResult.Yes == dialogResult)
                    full = true;

                MethodResult result = await _manager.ExportConfig(sfd.FileName, full);

                if (result.Success)
                {
                    SaveSettings();
                    MessageBox.Show("Config exported", "Info");
                }
            }
        }

        private async void ButtonImportConfig_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Title = "Open config file";
                ofd.Filter = "Json Files (*.json)|*.json|All Files (*.*)|*.*";

                if (ofd.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                MethodResult result = await _manager.ImportConfigFromFile(ofd.FileName);

                if (!result.Success)
                {
                    return;
                }

                UpdateDetails(_manager.UserSettings);
                UpdateListViews();
            }
        }

        private void ButtonDeviceRandom_Click(object sender, EventArgs e)
        {
            _manager.RandomDeviceId();

            textBoxDeviceId.Text = _manager.UserSettings.DeviceId;

            //UpdateDetails(_manager.UserSettings);
        }

        private void ButtonResetDefaults_Click(object sender, EventArgs e)
        {
            _manager.RandomDevice();

            UpdateDetails(_manager.UserSettings);
        }

        private void CheckBoxAutoRotateProxies_CheckedChanged(object sender, EventArgs e)
        {
            checkBoxRemoveOnStop.Enabled = checkBoxAutoRotateProxies.Checked;
        }

        private void AccountSettingsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void CbTeam_TextChanged(object sender, EventArgs e)
        {
            if (cbTeam.SelectedItem.ToString() != "Neutral")
            {
                checkBoxSpinGyms.Enabled = true;
                checkBoxDeployToGym.Enabled = true;
                checkBoxGoToGymsOnly.Enabled = true;
            }
            else
            {
                checkBoxSpinGyms.Enabled = false;
                checkBoxSpinGyms.Checked = false;
                checkBoxDeployToGym.Enabled = false;
                checkBoxDeployToGym.Checked = false;
                checkBoxGoToGymsOnly.Enabled = false;
                checkBoxGoToGymsOnly.Checked = false;
            }
        }
    }
}
