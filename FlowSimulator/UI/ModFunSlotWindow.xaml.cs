using System;
using System.ComponentModel;
using System.Windows;
using FlowGraphBase;

namespace FlowSimulator.UI
{
    public partial class ModFunSlotWindow : Window
    {
        public delegate bool IsValidInputNameDelegate(string name);

        private bool _dialogResult;

        private SequenceFunction function;


        public string InputName
        {
            get => textBoxName.Text;
            set => textBoxName.Text = value;
        }

        public string InputType
        {
            get => comboBox.SelectedItem.ToString();
            set => comboBox.SelectedItem = value;
        }

        public IsValidInputNameDelegate IsValidInputNameCallback
        {
            get;
            set;
        }

        public ModFunSlotWindow(SequenceFunction fun)
        {
            InitializeComponent();     
            
            function = fun;
            Closing += OnClosing;
        }

        void OnClosing(object sender, CancelEventArgs e)
        {
            DialogResult = _dialogResult;
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            if (IsValidInputNameCallback == null
                || (IsValidInputNameCallback != null
                    && IsValidInputNameCallback.Invoke(InputName)))
            {
                Type type = Type.GetType(InputType);
                function.AddInput(InputName, type);

                _dialogResult = true;
                Close();
            }
            else
            {
                _dialogResult = false;
                labelError.Content = "'" + InputName + "' некорректное имя слота.";
            }
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            _dialogResult = false;
            Close();
        }
    }
}
