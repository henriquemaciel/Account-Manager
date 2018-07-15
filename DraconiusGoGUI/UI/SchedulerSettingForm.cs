using DraconiusGoGUI.AccountScheduler;
using DraconiusGoGUI.Enums;
using System;
using System.Drawing;
using System.Windows.Forms;
using DraconiusGoGUI.Extensions;

namespace DraconiusGoGUI.UI
{
    public partial class SchedulerSettingForm : Form
    {
        private Scheduler _scheduler;
        private Color _color;

        public SchedulerSettingForm(Scheduler scheduler)
        {
            InitializeComponent();

            _scheduler = scheduler;

            foreach(SchedulerOption option in Enum.GetValues(typeof(SchedulerOption)))
            {
                comboBoxMasterAction.Items.Add(option);
                comboBoxCreatureAction.Items.Add(option);
                comboBoxPokestopAction.Items.Add(option);
            }

            textBoxChosenColor.BackColor = Color.FromArgb(43, 43, 43);
        }

        private void SchedulerSettingForm_Load(object sender, EventArgs e)
        {


            UpdateDetails();
        }

        private void UpdateDetails()
        {
            Text = _scheduler.Name;

            _color = _scheduler.NameColor;

            textBoxChosenColor.ForeColor = _color;

            textBoxName.Text = _scheduler.Name;

            numericUpDownStartTime.Value = new Decimal(_scheduler.StartTime);
            numericUpDownEndTime.Value = new Decimal(_scheduler.EndTime);
            numericUpDownCheckSpeed.Value = _scheduler.CheckTime;

            comboBoxCreatureAction.SelectOption<SchedulerOption>(_scheduler.CreatureLimiter.Option);
            numericUpDownCreatureMin.Value = _scheduler.CreatureLimiter.Min;
            numericUpDownCreatureMax.Value = _scheduler.CreatureLimiter.Max;

            comboBoxPokestopAction.SelectOption<SchedulerOption>(_scheduler.PokeStoplimiter.Option);
            numericUpDownPokestopsMin.Value = _scheduler.PokeStoplimiter.Min;
            numericUpDownPokestopsMax.Value = _scheduler.PokeStoplimiter.Max;

            comboBoxMasterAction.SelectOption<SchedulerOption>(_scheduler.MasterOption);
        }

        private bool SaveValues()
        {
            if(comboBoxMasterAction.HasNullItem() || comboBoxCreatureAction.HasNullItem() || comboBoxPokestopAction.HasNullItem())
            {
                MessageBox.Show("Please select valid options for the actions", "Warning");
                return false;
            }

            _scheduler.Name = textBoxName.Text;

            _scheduler.StartTime = (double)numericUpDownStartTime.Value;
            _scheduler.EndTime = (double)numericUpDownEndTime.Value;
            _scheduler.CheckTime = (int)numericUpDownCheckSpeed.Value;

            _scheduler.CreatureLimiter.Option = (SchedulerOption)comboBoxCreatureAction.SelectedItem;
            _scheduler.CreatureLimiter.Min = (int)numericUpDownCreatureMin.Value;
            _scheduler.CreatureLimiter.Max = (int)numericUpDownCreatureMax.Value;

            _scheduler.PokeStoplimiter.Option = (SchedulerOption)comboBoxPokestopAction.SelectedItem;
            _scheduler.PokeStoplimiter.Min = (int)numericUpDownPokestopsMin.Value;
            _scheduler.PokeStoplimiter.Max = (int)numericUpDownPokestopsMax.Value;

            _scheduler.MasterOption = (SchedulerOption)comboBoxMasterAction.SelectedItem;

            _scheduler.NameColor = _color;

            return true;
        }

        private void ButtonDone_Click(object sender, EventArgs e)
        {
            if(SaveValues())
            {
                DialogResult = DialogResult.OK;
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if(colorDialogNameColor.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            _color = colorDialogNameColor.Color;
            textBoxChosenColor.ForeColor = _color;
        }
    }
}
