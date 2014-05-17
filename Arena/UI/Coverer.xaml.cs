using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Threading;

namespace Arena.UI
{
    /// <summary>
    /// Delegate Function for Coverer.ExecuteOnBar.
    /// </summary>
    /// <param name="Bar"> The ProgressBar to operate on</param>
    public delegate void BarExecutable(ProgressBar Bar);

    public partial class Coverer : UserControl
    {
        protected Mutex Lock { get; set; }

        public Coverer()
        {
            InitializeComponent();

            Lock = new Mutex();

            SetVisible(false);
        }

        public void SetVisible(bool Visible)
        {
            if (Dispatcher.CheckAccess())
            {
                Lock.WaitOne();

                Visibility = Visible ? Visibility.Visible : Visibility.Hidden;
                IsHitTestVisible = Visible;

                Cover.Visibility = Visible ? Visibility.Visible : Visibility.Hidden;
                Cover.IsHitTestVisible = Visible;
                Bar.Visibility = Visible ? Visibility.Visible : Visibility.Hidden;
                Bar.IsHitTestVisible = Visible;

                Lock.ReleaseMutex();
            }
            else
            {
                Dispatcher.Invoke((Action<bool>)SetVisible, Visible);
            }
        }

        /// <summary>
        /// Executes a delegate function
        /// </summary>
        /// <param name="Function">The delegate Function to be executed.</param>
        /// <param name="Async">Whether the Functionr requires locking of the ProgressBar</param>
        public void ExecuteOnBar(BarExecutable Function, bool Async=true)
        {
            if (Bar.Dispatcher.CheckAccess())
            {
                /*
                 *  Don't care if an exception is thrown - just pass it up the stack.
                 *  DO care if we're running Asynchronously - if an exception is thrown the Mutex will deadlock.
                 *  Try-Finally block doesn't hinder exception but ensures Mutex is released.
                */
                try
                {
                    if (Async)
                    {
                        Lock.WaitOne();
                    }

                    Function.Invoke(Bar);
                }
                finally
                {
                    if (Async)
                    {
                        Lock.ReleaseMutex();
                    }
                }
            }
            else
            {
                Bar.Dispatcher.Invoke((Action<BarExecutable, bool>)ExecuteOnBar, new object[] { Function, Async });
            }
        }
    }
}