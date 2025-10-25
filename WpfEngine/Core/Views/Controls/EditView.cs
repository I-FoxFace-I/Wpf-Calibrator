using WpfEngine.Core.ViewModels;

namespace WpfEngine.Core.Views.Controls;

// ========== EDIT VIEW ==========

/// <summary>
/// UserControl for editing entity
/// </summary>
public abstract class EditView : DetailView, IEditView
{
    protected EditView()
    {
        IsReadOnly = false; // Edit views are not read-only
    }

    /// <summary>
    /// Validates current input
    /// Override in derived classes for custom validation
    /// </summary>
    public virtual bool Validate()
    {
        return true;
    }
}

/// <summary>
/// Edit control view with strongly-typed ViewModel
/// </summary>
public abstract class EditView<TViewModel> : EditView, IEditView<TViewModel>
    where TViewModel : BaseViewModel
{
    protected EditView()
    {
    }

    public TViewModel? ContextModel
    {
        get => (TViewModel)DataContext;
        set => DataContext = value;
    }
}