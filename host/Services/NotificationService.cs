using Ca.Jwsm.Railroader.Api.Ui.Contracts;
using Ca.Jwsm.Railroader.Api.Ui.Models;
using UI.Common;

namespace Ca.Jwsm.Railroader.Api.Host.Services
{
    public sealed class NotificationService : INotificationService
    {
        public void Notify(string message, UiPriority priority = UiPriority.Normal)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            try
            {
                Toast.Present(message, ResolvePosition(priority));
            }
            catch
            {
            }
        }

        private static ToastPosition ResolvePosition(UiPriority priority)
        {
            switch (priority)
            {
                case UiPriority.High:
                    return ToastPosition.Middle;
                case UiPriority.Low:
                    return ToastPosition.Bottom;
                case UiPriority.Normal:
                default:
                    return ToastPosition.Middle;
            }
        }
    }
}
