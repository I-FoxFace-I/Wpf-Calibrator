using WpfEngine.Data.Abstract;
using WpfEngine.Data.Dialogs;
using WpfEngine.Enums;
using WpfEngine.ViewModels;

namespace WpfEngine.Services;

public interface IScopedWindowManager : IWindowManager
{
    Task<DialogResult> ShowDialogAsync<TViewModel>(
       Guid? ownerWindowId,
       DialogModality modality = DialogModality.AppModal)
           where TViewModel : class, IViewModel, IDialogViewModel;

    Task<DialogResult> ShowDialogAsync<TViewModel, TParameters>(
        Guid? ownerWindowId,
        TParameters parameters,
        DialogModality modality = DialogModality.AppModal)
            where TViewModel : class, IViewModel, IDialogViewModel<TParameters>
            where TParameters : IViewModelParameters;

    Task<DialogResult<TResult>> ShowDialogAsync<TViewModel, TResult>(
        Guid? ownerWindowId,
        DialogModality modality = DialogModality.AppModal)
            where TViewModel : class, IViewModel, IDialogViewModel, IResultDialogViewModel<TResult>
            where TResult : class;

    Task<DialogResult<TResult>> ShowDialogAsync<TViewModel, TParameters, TResult>(
        Guid? ownerWindowId,
        TParameters parameters,
        DialogModality modality = DialogModality.AppModal)
            where TViewModel : class, IViewModel, IDialogViewModel<TParameters>, IResultDialogViewModel<TResult>
            where TParameters : IViewModelParameters
            where TResult : class;
}

