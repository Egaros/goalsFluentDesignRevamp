using goalsFluentDesignRevamp;
using System;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using Windows.Phone.UI.Input;
using Windows.UI.Composition;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;

public class Navigation
{
    public static Navigation Instance { get; protected set; }
    private Frame frame;
    private Compositor _compositor;

    public Navigation(ref Frame frame)
    {
        if (Instance != null)
        {
            throw new Exception("Only one navigation service can exist.");
        }

        //assigns the instance of the class and frame
        Instance = this;
        this.frame = frame;

        //registers the back requested events
        SystemNavigationManager.GetForCurrentView().BackRequested += NavigationService_BackRequested;
        if (ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
        {
            HardwareButtons.BackPressed += HardwareButtons_BackPressed;
        }

        _compositor = ElementCompositionPreview.GetElementVisual(frame).Compositor;
    }

    public async void NavigateTo(Type pageType, object parameter = null)
    {
        await AnimatePageOut();

        frame.Navigate(pageType, parameter);



        if (pageType == typeof(MainPage))
        {

            await AnimateScaleIn();
            frame.BackStack.Clear();
        }
        else
        {
            await AnimatePageIn();
        }

        
    }

    private async Task AnimateScaleIn()
    {
        var newPage = frame.Content;
        if (newPage != null)
        {
            var page = newPage as FrameworkElement;

            if (_compositor == null)
                _compositor = ElementCompositionPreview.GetElementVisual(page).Compositor;


            KeyFrameAnimation scaleAnimation = _compositor.CreateScalarKeyFrameAnimation();
            scaleAnimation.InsertExpressionKeyFrame(0f, ".5");
            scaleAnimation.InsertExpressionKeyFrame(1f, "1");
            scaleAnimation.Duration = TimeSpan.FromMilliseconds(250);

            KeyFrameAnimation fadeAnimation = _compositor.CreateScalarKeyFrameAnimation();
            fadeAnimation.InsertExpressionKeyFrame(1f, "1");
            fadeAnimation.Duration = TimeSpan.FromMilliseconds(200);

            var visual = ElementCompositionPreview.GetElementVisual(page);
            visual.Opacity = 0; //sets initial opacity to 0, since we will be animating it to 1

            //this loop is needed, so that we don't continue until the page is actually generated (otherwise, the size of the visual gets reported as 0)
            while (visual.Size.X == 0)
            {
                await Task.Delay(1);
            }

            visual.CenterPoint = new Vector3(visual.Size.X / 2, visual.Size.Y / 2, 0); //sets the center point of the visual (this is used as the anchor point of the scale animation)

            visual.StartAnimation("Scale.X", scaleAnimation);
            visual.StartAnimation("Scale.Y", scaleAnimation);
            visual.StartAnimation("Opacity", fadeAnimation);

            await Task.Delay(250); //waits until the animation completes in order to continue
        }
    }

    public async void NavigateBack()
    {
        if (frame.CanGoBack)
        {
            await AnimatePageOut();

            frame.GoBack();

            await AnimatePageIn();

            
        }
    }

    private async void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
    {
        if (frame.CanGoBack)
        {
            e.Handled = true;

            await AnimatePageOut();

            frame.GoBack();

            await AnimatePageIn();

            
        }
    }

    private async void NavigationService_BackRequested(object sender, BackRequestedEventArgs e)
    {
        if (frame.CanGoBack)
        {
            e.Handled = true;

            await AnimatePageOut();

            frame.GoBack();

            await AnimatePageIn();

            
        }
    }


    private async Task AnimatePageOut()
    {
        var newPage = frame.Content;
        if (newPage != null)
        {
            var page = newPage as FrameworkElement;

            if (_compositor == null)
                _compositor = ElementCompositionPreview.GetElementVisual(page).Compositor;

            var visual = ElementCompositionPreview.GetElementVisual(page);


            KeyFrameAnimation offsetInAnimation = _compositor.CreateScalarKeyFrameAnimation();
            offsetInAnimation.InsertExpressionKeyFrame(1f, "-140");
            offsetInAnimation.Duration = TimeSpan.FromMilliseconds(250);

            KeyFrameAnimation fadeAnimation = _compositor.CreateScalarKeyFrameAnimation();
            fadeAnimation.InsertExpressionKeyFrame(1f, "0");
            fadeAnimation.Duration = TimeSpan.FromMilliseconds(200);

            visual.StartAnimation("Offset.Y", offsetInAnimation);
            visual.StartAnimation("Opacity", fadeAnimation);

            await Task.Delay(250); //waits until the animation completes in order to continue
        }
    }

    private async Task AnimatePageIn()
    {
        var newPage = frame.Content;
        if (newPage != null)
        {
            var page = newPage as FrameworkElement;

            if (_compositor == null)
                _compositor = ElementCompositionPreview.GetElementVisual(page).Compositor;

            var visual = ElementCompositionPreview.GetElementVisual(page);

            visual.Offset = new Vector3(0, -140, 0);
            visual.Opacity = 0f;
            visual.Scale = new Vector3(1, 1, 0);

            KeyFrameAnimation offsetInAnimation = _compositor.CreateScalarKeyFrameAnimation();
            offsetInAnimation.InsertExpressionKeyFrame(1f, "0");
            offsetInAnimation.Duration = TimeSpan.FromMilliseconds(250);


            KeyFrameAnimation fadeAnimation = _compositor.CreateScalarKeyFrameAnimation();
            fadeAnimation.InsertExpressionKeyFrame(1f, "1");
            fadeAnimation.Duration = TimeSpan.FromMilliseconds(250);


            visual.StartAnimation("Offset.Y", offsetInAnimation);
            visual.StartAnimation("Opacity", fadeAnimation);

            await Task.Delay(250); //waits until the animation completes in order to continue
        }

    }

}