namespace Web.Models;

/// <summary>
/// Hierarchical tree node model for AppTreeView.
/// </summary>
public class TreeNode
{
    public string Name { get; set; } = "";
    public bool HasChildren { get; set; }
    public List<TreeNode> Children { get; set; } = new();
}
