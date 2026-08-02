using Microsoft.UI.Xaml.Controls;

namespace HalcyonRecords.SeedDataGenerator.Navigation;

public sealed class NavigationService
{
    private Frame? _frame;

    public void Initialize(Frame frame) => _frame = frame;

    public void Navigate(Type pageType, object? parameter = null)
    {
        if (_frame is null)
        {
            throw new InvalidOperationException(
                "NavigationService was never initialized with a Frame."
            );
        }

        _frame.Navigate(pageType, parameter);
    }
}
