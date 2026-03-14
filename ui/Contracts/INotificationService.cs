using Ca.Jwsm.Railroader.Api.Ui.Models;

namespace Ca.Jwsm.Railroader.Api.Ui.Contracts
{
    public interface INotificationService
    {
        void Notify(string message, UiPriority priority = UiPriority.Normal);
    }
}
