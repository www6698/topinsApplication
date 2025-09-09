using Microsoft.Xaml.Behaviors;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace topinsApplication;

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public class tpTextBoxTextChangedBehavior : Behavior<TextBox>
{
    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(tpTextBoxTextChangedBehavior));

    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    private Key? __lastKey;

    protected override void OnAttached()
    {
        AssociatedObject.KeyDown += OnKeyDown;
        AssociatedObject.TextChanged += OnTextChanged;
    }

    protected override void OnDetaching()
    {
        AssociatedObject.KeyDown -= OnKeyDown;
        AssociatedObject.TextChanged -= OnTextChanged;
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        __lastKey = e.Key;
    }

    private void OnTextChanged(object sender, TextChangedEventArgs e)
    {
        string text = (AssociatedObject?.Text) ?? string.Empty;
        var key = __lastKey;

        __lastKey = null;

        if (Command?.CanExecute((key, text)) == true) Command.Execute((key, text));
    }
}
