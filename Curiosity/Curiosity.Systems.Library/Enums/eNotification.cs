namespace Curiosity.Systems.Library.Enums
{
    /// <summary>
    /// Notification Settings for NUI
    /// 0 Clear
    /// 1 Success
    /// 2 Error
    /// 3 Warning
    /// 4 Info
    /// 5 No Style
    /// </summary>
    public enum eNotification
    {
        NOTIFICATION_CLEAR, // 0 - Remove all notifications
        NOTIFICATION_SUCCESS, // 1 - Successful Notification
        NOTIFICATION_ERROR, // 2 - Error
        NOTIFICATION_WARNING, // 3 - Warning
        NOTIFICATION_INFO, // 4 - Info
        NOTIFICATION_SHOW, // 5 - Default with no colors
        NOTIFICATION_LOADER, // 6 - Loading
        NOTIFICATION_ANNOUNCEMENT
    }
}
