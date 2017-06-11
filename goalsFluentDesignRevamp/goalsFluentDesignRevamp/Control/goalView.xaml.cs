using goalsFluentDesignRevamp.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace goalsFluentDesignRevamp.Control
{
    public sealed partial class goalView : UserControl
    {

        public Model.goal goalItem { get { return this.DataContext as Model.goal; } }
        public goalView()
        {
            this.InitializeComponent();

            this.DataContextChanged += (s, e) => Bindings.Update();
            

          
        }

        public void testVisibility()
        {
            nameTextBlock.Visibility = Visibility.Collapsed;
        }

       

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (goal.listOfGoals.Count > 0)
            {
                var control = (goalView)sender;
                var item = (goal)control.DataContext;
                if (item.name == "poof")
                {
                    testVisibility();
                }

            }
        }
    }

}


