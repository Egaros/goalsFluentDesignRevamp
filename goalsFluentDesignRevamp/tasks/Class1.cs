using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Notifications;
using Windows.ApplicationModel.Background;
using NotificationsExtensions.Toasts;
using NotificationsExtensions;

namespace tasks
{
    public sealed class Class1 : IBackgroundTask
    {
        BackgroundTaskDeferral _deferral; // Note: defined at class scope so we can mark it complete inside the OnCancel() callback if we choose to support cancellation
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();
            var toastContent = new ToastContent()
            {
                Launch = "comeBack",
                ActivationType = ToastActivationType.Foreground,
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
            {
                new AdaptiveText()
                {
                    Text = "Reminding you to achieve your goals!"
                },
                new AdaptiveText()
                {
                    Text = "Most people struggle in the beginning but YOU ARE different, YOU CAN achieve your goals!"
                }
            },

                    }
                },
                Actions = new ToastActionsCustom()
                {
                    Buttons =
        {
            new ToastButton("Complete My Goals!", "Yes")
            {
                ActivationType = ToastActivationType.Foreground,

            }
        }
                }
            };

            // Create the toast notification
            var toastNotif = new ToastNotification(toastContent.GetXml());

            // And send the notification
            ToastNotificationManager.CreateToastNotifier().Show(toastNotif);

            _deferral.Complete();
        }
    }
}
