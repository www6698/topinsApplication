using Microsoft.Xaml.Behaviors;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Input;

namespace topinsApplication;

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public class tpEventToCommandBehavior : Behavior<UIElement>
{
    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(tpEventToCommandBehavior));

    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public string EventName { get; set; }

    protected override void OnAttached()
    {
        switch (EventName)
        {
            case "MouseLeftButtonDown":
                AssociatedObject.MouseLeftButtonDown += OnEventRaised;
                break;
            case "MouseMove":
                AssociatedObject.MouseMove += OnEventRaised;
                break;
            case "MouseLeftButtonUp":
                AssociatedObject.MouseLeftButtonUp += OnEventRaised;
                break;
        }
    }

    private void OnEventRaised(object sender, RoutedEventArgs e)
    {
        if (true == Command?.CanExecute(e))
        {
            Command.Execute(e);
        }
    }

    protected override void OnDetaching()
    {
        AssociatedObject.MouseLeftButtonDown -= OnEventRaised;
        AssociatedObject.MouseMove -= OnEventRaised;
        AssociatedObject.MouseLeftButtonUp -= OnEventRaised;
    }
}