using Microsoft.Xaml.Behaviors;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace topinsApplication;

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public class tpNumericInputBehavior : Behavior<TextBox>
{
    private static readonly Regex __regex = new(@"^\d+$");

    protected override void OnAttached()
    {
        base.OnAttached();

        AssociatedObject.PreviewTextInput += OnPreviewTextInput;

        DataObject.AddPastingHandler(AssociatedObject, OnPaste);
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        AssociatedObject.PreviewTextInput -= OnPreviewTextInput;

        DataObject.RemovePastingHandler(AssociatedObject, OnPaste);
    }

    private void OnPreviewTextInput(object sender, TextCompositionEventArgs e) => e.Handled = !__regex.IsMatch(e.Text);

    private void OnPaste(object sender, DataObjectPastingEventArgs e)
    {
        if (!e.SourceDataObject.GetDataPresent(DataFormats.Text, true)) return;

        if (!__regex.IsMatch(e.SourceDataObject.GetData(DataFormats.Text) as string)) e.CancelCommand();
    }
}