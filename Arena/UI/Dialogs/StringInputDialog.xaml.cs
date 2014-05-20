using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Arena.UI.Dialogs
{
    public partial class StringInputDialog : Window
    {
        public string Prompt
        {
            get
            {
                return _Prompt.Text;
            }
            set
            {
                _Prompt.Text = value;
            }
        }
        public string Input
        {
            get
            {
                return _Input.Text;
            }
            set
            {
                _Input.Text = (string)value;
            }
        }

        public StringInputDialog()
        {
            InitializeComponent();

            Title = "Input";
            Prompt = "Input:";
        }

        protected void Ok_Click(object sender, EventArgs e)
        {
            DialogResult = true;
            Close();
        }
        protected void Cancel_Click(object sender, EventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
