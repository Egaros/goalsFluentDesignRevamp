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
using Microsoft.Toolkit.Uwp.UI.Animations;
using System.Threading.Tasks;
using Windows.UI.Text;

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

                DateTime noTimeLimitTrigger = new DateTime(1, 1, 1);

                if (item.endTime != noTimeLimitTrigger)
                {
                    hideProgressTextBlock();
                    goal.determineTimeLeft(item);
                    timeLeftTextBlock.Text = item.unitsOfTimeRemaining;
                    showTimeRemaining();
                }
                
                

                


                if (item.name=="poof")
                {
                    hideNameTextBlock();   
                }

                if (item.name == "#TeamXbox")
                {
                    makeNameTextBoxGreen();
                }

                if (item.name == "Medium Universal")
                {
                    throwBackToOldBrandName(item);
                }

                if (item.name=="meheni pls")
                {
                    meheniPls();
                }
            }
        }

        private void showTimeRemaining()
        {
            timeLeftTextBlock.Visibility = Visibility.Visible;
        }

        private async void meheniPls()
        {
            nameTextBlock.FontWeight = FontWeights.Bold;
            nameTextBlock.Text = "meheni pls";
            await Task.Delay(1000);
            targetTextBlock.FontWeight = FontWeights.Bold;
            targetTextBlock.Foreground = new SolidColorBrush(Colors.Black);
            targetTextBlock.Text = "Target: meheni pls";
            await Task.Delay(1000);
            progressTextBlock.FontWeight = FontWeights.Bold;
            progressTextBlock.Foreground = new SolidColorBrush(Colors.Black);
            progressTextBlock.Text = "Progress: meheni pls";
            for (int i = 0; i < 4; i++)
            {
                await Task.Delay(1000);
                nameTextBlock.Text += " meheni pls";

            }

            for (int i = 0; i < 4; i++)
            {
                await Task.Delay(1000);
                targetTextBlock.Text += " meheni pls";

            }

            for (int i = 0; i < 10; i++)
            {
                await Task.Delay(1000);
                if (i==1)
                {
                    progressTextBlock.Text += " Surface Phone";
                    await Task.Delay(1000);
                    progressTextBlock.Text = "Target: meheni pls";
                    await Task.Delay(1000);
                }
                progressTextBlock.Text += " meheni pls";

            }

        }

        
        private async void throwBackToOldBrandName(goal item)
        {
            var _compositor = new Compositor();
            nameTextBlock.Opacity = 0;
            nameTextBlock.Foreground = new SolidColorBrush(Colors.Blue);
            bool wait = await nameTextBlock.Fade(0.6f, 1000).StartAsync();
            nameTextBlock.Text = "Goal$";
            wait = await nameTextBlock.Fade(0.8f, 1000).StartAsync();
            nameTextBlock.Text = "Track Goals";
        }

        private async void makeNameTextBoxGreen()
        {
            nameTextBlock.Foreground = new SolidColorBrush(Colors.Green);
            await Task.Delay(1000);
            targetTextBlock.Foreground = new SolidColorBrush(Colors.Green);
            targetTextBlock.Text = "Target: 4K";
            await Task.Delay(1000);
            progressTextBlock.Foreground = new SolidColorBrush(Colors.Green);
            progressTextBlock.Text = "Achieved: At 60FPS 🐱‍🏍🐱‍🏍🐱‍🏍🐱‍🏍🐱‍🏍";
        }

        private async void hideNameTextBlock()
        {
            await Task.Delay(1000);
            nameTextBlock.Opacity = 0;
        }

        private void hideProgressTextBlock()
        {
            progressTextBlock.Visibility = Visibility.Collapsed;
        }


    }
}



