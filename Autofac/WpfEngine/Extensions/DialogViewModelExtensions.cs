using WpfEngine.Data.Abstract;
using WpfEngine.Enums;
using WpfEngine.ViewModels;

namespace WpfEngine.Extensions;

public static class DialogViewModelExtensions
{

    public static TResult CreateDialogResult<TViewModel, TResult, TData>(this TViewModel viewModel, Exception? exception = null)
        where TResult : IDialogResult<TResult, TData>
        where TViewModel : IDialogViewModel
        where TData : notnull
    {
        if(viewModel is IResultDialogViewModel<object> rvm)
        {
            return viewModel.Status switch
            {
                DialogStatus.Pending => TResult.Error(exception?.Message ?? "Pending dialog"),
                DialogStatus.Error => TResult.Error(exception?.Message),
                DialogStatus.Cancel => TResult.Cancel(),
                DialogStatus.Success => rvm.ResultData is TData result ? TResult.Success(result) : TResult.Success(default),
                _ => TResult.Cancel()
            };
        }
        else
        {
            return viewModel.Status switch
            {
                DialogStatus.Pending => TResult.Error(exception?.Message ?? "Pending dialog"),
                DialogStatus.Error => TResult.Error(exception?.Message),
                DialogStatus.Cancel => TResult.Cancel(),
                DialogStatus.Success => TResult.Success(),
                _ => TResult.Cancel()
            };
        }
    }
}
