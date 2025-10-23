using System.Windows;

namespace Calibrator.WpfApplication.Views.Base;

/// <summary>
/// Represents facade for views. (Supports loading indicator, initialization etc)
/// Template is defined in <see cref="BaseViewStyles.xaml"/>
/// </summary>
public class BaseView : Window
{
    static BaseView()
    {
        // Allows to use ContentPresenter (in BaseViewStyles.xaml) and override default styles
        DefaultStyleKeyProperty.OverrideMetadata(typeof(BaseView),
            new FrameworkPropertyMetadata(typeof(BaseView)));
    }
}

