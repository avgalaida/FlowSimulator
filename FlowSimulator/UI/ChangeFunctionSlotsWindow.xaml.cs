using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using FlowGraphBase;

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

            UpdateListBox(1, function.Inputs);
            UpdateListBox(2, function.Outputs);
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

        private void UpdateListBox(int id, IEnumerable<SequenceFunctionSlot> collection)
        {
            if (id == 1)
            {
                InputSlotsListBox.Items.Clear();
                foreach (SequenceFunctionSlot s in collection)
                {
                    InputSlotsListBox.Items.Add(s.Id + " " + s.VariableType + " " + s.Name);
                }
            }
            else if (id == 2)
            {
                OutputSlotsListBox.Items.Clear();
                foreach (SequenceFunctionSlot s in collection)
                {
                    OutputSlotsListBox.Items.Add(s.VariableType + " " + s.Name);
                }
            }
        }

        private void AddSlotInputButton_Click(object sender, RoutedEventArgs e)
        {
            ModFunSlotWindow win = new ModFunSlotWindow(function)
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
                UpdateListBox(1, function.Inputs);
                
            }
        }
    }
}
