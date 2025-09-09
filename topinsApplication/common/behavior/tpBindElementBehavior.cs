using Microsoft.Xaml.Behaviors;
using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace topinsApplication;

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public class tpBindElementBehavior : Behavior<FrameworkElement>
{
    protected override void OnAttached()
    {
        AssociatedObject.Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        AssociatedObject.Loaded -= OnLoaded;

        if (AssociatedObject.DataContext is tpMvvm mvvm)
        {
            mvvm.Element = AssociatedObject;
        }
    }
}
