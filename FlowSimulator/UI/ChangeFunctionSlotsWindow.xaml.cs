using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using FlowGraphBase;
using FlowGraphBase.Logger;

namespace FlowSimulator.UI
{
    public partial class ChangeFunctionSlotsWindow : Window
    {

        private bool _dialogResult;
        private SequenceFunction function;

        public ChangeFunctionSlotsWindow(SequenceFunction fun)
        {
            InitializeComponent();

            function = fun;       
            Closing += OnClosing;

            UpdateListBox(InputSlotsListBox, function.Inputs);
            UpdateListBox(OutputSlotsListBox, function.Outputs);
        }

        void OnClosing(object sender, CancelEventArgs e)
        {
            DialogResult = _dialogResult;
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            _dialogResult = true;
            Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            _dialogResult = false;
            Close();
        }

        private void UpdateListBox(System.Windows.Controls.ListBox lB, IEnumerable<SequenceFunctionSlot> collection)
        {
            lB.Items.Clear();
            foreach (SequenceFunctionSlot slot in collection)
            {
                lB.Items.Add(slot.Id + " " + slot.VariableType + " " + slot.Name);
                //lB.Items.Add(slot.VariableType + " " + slot.Name);
            }
        }

        private void AddSlotInputButton_Click(object sender, RoutedEventArgs e)
        {
            ModFunSlotWindow win = new ModFunSlotWindow(function, 1)
            {
                Title = "Новый слот",
                Owner = MainWindow.Instance
            };

            if (win.ShowDialog() == false)
            {
                return;
            } 
            else
            {
                UpdateListBox(InputSlotsListBox, function.Inputs);
            }
        }

        private void AddSlotOutputButton_Click(object sender, RoutedEventArgs e)
        {
            ModFunSlotWindow win = new ModFunSlotWindow(function, 2)
            {
                Title = "Новый слот",
                Owner = MainWindow.Instance
            };

            if (win.ShowDialog() == false)
            {
                return;
            }
            else
            {
                UpdateListBox(OutputSlotsListBox, function.Outputs);
            }
        }

        private void RemoveSlotFromFunction(int id, IEnumerable<SequenceFunctionSlot> collection)
        {
            List<int> idList = new List<int>();
            foreach (SequenceFunctionSlot s in collection)
            {
                idList.Add(s.Id);
            }
            function.RemoveSlotById(idList[id]);
        }

        private void RemoveSlotInputButton_Click(object sender, RoutedEventArgs e)
        {
            if (InputSlotsListBox.SelectedIndex >= 0)
            {
                RemoveSlotFromFunction(InputSlotsListBox.SelectedIndex, function.Inputs);
                InputSlotsListBox.Items.RemoveAt(InputSlotsListBox.SelectedIndex);
                UpdateListBox(InputSlotsListBox, function.Inputs);
            }
        }

        private void RemoveSlotOutputButton_Click(object sender, RoutedEventArgs e)
        {
            if (OutputSlotsListBox.SelectedIndex >= 0)
            {
                RemoveSlotFromFunction(OutputSlotsListBox.SelectedIndex, function.Outputs);
                OutputSlotsListBox.Items.RemoveAt(OutputSlotsListBox.SelectedIndex);
                UpdateListBox(OutputSlotsListBox, function.Outputs);
            }
        }
    }
}
