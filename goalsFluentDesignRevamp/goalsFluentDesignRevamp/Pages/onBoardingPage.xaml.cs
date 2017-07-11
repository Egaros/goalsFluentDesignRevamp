using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Composition;
using Microsoft.Toolkit.Uwp.UI.Animations;
using System.Threading.Tasks;
using Windows.UI.Xaml.Hosting;
using System.Numerics;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace goalsFluentDesignRevamp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class onBoardingPage : Page
    {
        Compositor _compositor;
        SpriteVisual _hostSprite;

        public onBoardingPage()
        {
            this.InitializeComponent();
        }

        private void onBoardingFlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int currentFlipViewIndex = findCurrentFlipView();
            changeFlipViewIndicator(currentFlipViewIndex);

        }

        private void changeFlipViewIndicator(int currentFlipViewIndex)
        {
           
            switch (currentFlipViewIndex)
            {
                case 0:
                    try
                    {
                        firstListBox.Background = new SolidColorBrush(Colors.White);
                        clearOtherIndicators("first");
                        break;
                    }
                    catch (Exception)
                    {
                        break;

                    }

                case 1:
                    secondListBox.Background = new SolidColorBrush(Colors.White);
                    clearOtherIndicators("second");
                    break;
                case 2:
                    thirdListBox.Background = new SolidColorBrush(Colors.White);
                    clearOtherIndicators("third");
                    break;
                case 3:
                    fourthListBox.Background = new SolidColorBrush(Colors.White);
                    clearOtherIndicators("fourth");
                    break;
                case 4:
                    fifthListBox.Background = new SolidColorBrush(Colors.White);
                    clearOtherIndicators("fifth");
                    break;
                default:
                    firstListBox.Background = new SolidColorBrush(Colors.White);
                    clearOtherIndicators("first");
                    break;
            }

           
        }

        private bool checkifIndicatorsAreLoaded()
        {

            List<ListBox> allIndicators = new List<ListBox> { firstListBox, secondListBox, thirdListBox, fourthListBox, fifthListBox };
            bool indicatorsLoaded = checkIfEachIndicatorHasLoaded(allIndicators);


            return indicatorsLoaded;
        }

        private bool checkIfEachIndicatorHasLoaded(List<ListBox> allIndicators)
        {
            bool allIndicatorsHaveLoaded = true;
            foreach (var indicator in allIndicators)
            {
                if (indicator.IsEnabled == false)
                {
                    allIndicatorsHaveLoaded = false;
                }
            }
            return allIndicatorsHaveLoaded;
        }

        private void clearOtherIndicators(string selectedListBox)
        {
            List<ListBox> allIndicators = new List<ListBox> { firstListBox, secondListBox, thirdListBox, fourthListBox, fifthListBox };
            List<ListBox> unselectedIndicators = new List<ListBox>();
            switch (selectedListBox)
            {
                case "first":
                    allIndicators.Remove(firstListBox);
                    unselectedIndicators = allIndicators;
                    changeBackgroudnOfUnselectedIndicators(unselectedIndicators);
                    
                    break;

                case "second":
                    allIndicators.Remove(secondListBox);
                    unselectedIndicators = allIndicators;
                    changeBackgroudnOfUnselectedIndicators(unselectedIndicators);
                    
                    break;

                case "third":
                    allIndicators.Remove(thirdListBox);
                    unselectedIndicators = allIndicators;
                    changeBackgroudnOfUnselectedIndicators(unselectedIndicators);
                    
                    break;
                case "fourth":
                    allIndicators.Remove(fourthListBox);
                    unselectedIndicators = allIndicators;
                    changeBackgroudnOfUnselectedIndicators(unselectedIndicators);
                    
                    break;

                case "fifth":
                    allIndicators.Remove(fifthListBox);
                    unselectedIndicators = allIndicators;
                    changeBackgroudnOfUnselectedIndicators(unselectedIndicators);
                    
                    break;

                default:
                    allIndicators.Remove(firstListBox);
                    unselectedIndicators = allIndicators;
                    changeBackgroudnOfUnselectedIndicators(unselectedIndicators);
                    break;
            }
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var qualifiers = Windows.ApplicationModel.Resources.Core.ResourceContext.GetForCurrentView().QualifierValues;

            if (qualifiers.ContainsKey("DeviceFamily") && qualifiers["DeviceFamily"] == "Desktop" && Windows.Foundation.Metadata.ApiInformation.IsMethodPresent("Windows.UI.Composition.Compositor", "CreateHostBackdropBrush"))
            {
               
                applyAcrylicAccent(transparentArea);
                makeButtonsTranslucent();

            }
        }

        private void makeButtonsTranslucent()
        {
            double backgroundOpacity = 0.1;
            startNewButton.Background.Opacity = backgroundOpacity;
            carryOnFromCloudButton.Background.Opacity = backgroundOpacity;
        }

        private void applyAcrylicAccent(Panel panel)
        {
            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
             _hostSprite = _compositor.CreateSpriteVisual();
            _hostSprite.Size = new Vector2((float)panel.ActualWidth, (float)panel.ActualHeight);
            ElementCompositionPreview.SetElementChildVisual(panel, _hostSprite);
            _hostSprite.Opacity = 0.8f;



            _hostSprite.Brush = _compositor.CreateHostBackdropBrush();
        }




        private void changeBackgroudnOfUnselectedIndicators(List<ListBox> unselectedIndicators)
        {
            foreach (var unselectedIndicator in unselectedIndicators)
            {
                unselectedIndicator.Background = new SolidColorBrush(Color.FromArgb(255, 43, 43, 43));
            }
        }

        private int findCurrentFlipView()
        {
            return onBoardingFlipView.SelectedIndex;
        }

        private void startNewButton_Click(object sender, RoutedEventArgs e)
        {
            App.NavService.NavigateTo(typeof(whatsNewPage));
        }

        private void carryOnFromCloudButton_Click(object sender, RoutedEventArgs e)
        {
            App.NavService.NavigateTo(typeof(MainPage), "carryOnFromCloud");
        }

      

        private void transparentArea_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_hostSprite != null)
                _hostSprite.Size = e.NewSize.ToVector2();
        }
    }
}
