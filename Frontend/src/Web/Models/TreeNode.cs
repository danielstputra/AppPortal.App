namespace Web.Models;

/// <summary>
/// Tree node model for AppTreeView.
/// Supports both hierarchical (Children) and flat (ParentId) data binding.
/// </summary>
public class TreeNode
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int? ParentId { get; set; }
    public bool HasChildren { get; set; }
    public List<TreeNode> Children { get; set; } = new();
}
