namespace Web.UI.Base;

/// <summary>
/// Event arguments for the <see cref="AppAccordion.BeforeContextMenuShown"/> event.
/// Provides information about the accordion item that was right-clicked
/// and allows the consumer to suppress or customize the context menu.
/// </summary>
public class AccordionContextMenuEventArgs
{
    /// <summary>The display text of the accordion item that was right-clicked.</summary>
    public string? ItemText { get; set; }

    /// <summary>Set to true to suppress the context menu from appearing.</summary>
    public bool Cancel { get; set; }
}
