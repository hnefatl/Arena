﻿using System;
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
    /// <summary>
    /// Interaction logic for ConfirmDialog.xaml
    /// </summary>
    public partial class ConfirmDialog : Window
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

        public ConfirmDialog()
        {
            InitializeComponent();
        }

        private void Yes_Click(object sender, EventArgs e)
        {
            DialogResult = true;
            Close();
        }
        private void No_Click(object sender, EventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
