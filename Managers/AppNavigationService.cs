using System.Windows.Controls;

namespace CrosshairY.Managers
{
    public static class AppNavigationService
    {
        public static Action<Type>? NavigateAction;

        public static void Navigate<T>() where T : Page, new()
        {
            NavigateAction?.Invoke(typeof(T));
        }

        public static void Navigate(Type pageType)
        {
            NavigateAction?.Invoke(pageType);
        }
    }
}