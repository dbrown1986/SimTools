using System.Collections.ObjectModel;
using System.Windows.Media;

namespace SimTools;

/// <summary>
/// A single recursive node used at every level of the Buy tree.
/// Leaf nodes (no Children) have a non-empty Url that opens in the browser.
/// Category nodes that should show a pop-up on selection carry a Message.
/// </summary>
public class BuyTreeNode
{
    public string       Label    { get; set; } = string.Empty;
    public ImageSource? Icon     { get; set; }
    public string?      Url      { get; set; }     // non-empty → open browser on select
    public string?      Message  { get; set; }     // non-null  → show info popup on select
    public ObservableCollection<BuyTreeNode> Children { get; } = [];
}
