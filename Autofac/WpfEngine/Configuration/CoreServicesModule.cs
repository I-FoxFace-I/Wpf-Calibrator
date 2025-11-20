using Autofac;
using WpfEngine.Services;
using WpfEngine.Services.Autofac;

namespace WpfEngine.Configuration;

/// <summary>
/// Autofac module for Core services registration - REFACTORED
/// 
/// NEW SIMPLIFIED ARCHITECTURE:
/// - IWindowManager (Singleton) - Application-level window management
/// - IWindowContext (InstancePerLifetimeScope) - Per-window child management
/// - ViewRegistry (Singleton) - VM->View mappings
/// - Session management will be added later via ISessionManager
/// </summary>
public class CoreServicesModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        // ========== VIEW REGISTRY (Singleton) ==========
        builder.RegisterType<ViewRegistry>()
               .AsSelf()
               .As<IViewRegistry>()
               .SingleInstance();

        // ========== WINDOW MANAGER (Singleton) ==========
        /// <summary>
        /// WINDOW MANAGER - Unified window and dialog management
        /// Uses new IScopeManager for session management
        /// Responsibilities:
        /// - Opens root windows from application scope
        /// - Opens child windows from parent window scope
        /// - Global window tracking and queries
        /// - Window lifecycle events
        /// - Dialog opening and closing
        /// - Session-aware window operations
        /// </summary>
        builder.RegisterType<ScopedWindowManager>()
               .As<IWindowManager>()
               .As<IScopedWindowManager>()
               .SingleInstance();

        builder.RegisterType<WindowTracker>()
                   .As<IWindowTracker>()
                   .SingleInstance();

        // ========== SCOPE MANAGER (Singleton) ==========
        /// <summary>
        /// SCOPE MANAGER - New session/scope management system
        /// Responsibilities:
        /// - Creates and tracks IScopeSession instances
        /// - Manages scope hierarchy with tags
        /// - Supports database, workflow, and custom scopes
        /// </summary>
        builder.RegisterType<ScopeManager>()
               .As<IScopeManager>()
               .SingleInstance();



        // ========== WINDOW CONTEXT (InstancePerLifetimeScope) ==========
        /// <summary>
        /// WINDOW CONTEXT - Per-window child management
        /// Each window scope has its own instance
        /// Responsibilities:
        /// - Opens child windows from THIS window
        /// - Closes this window
        /// - Tracks children opened from this window
        /// - Automatically disposes when window scope disposes
        /// 
        /// NOTE: WindowId is set during window creation via OnActivating
        /// </summary>
        builder.RegisterType<WindowContext>()
               .As<IWindowContext>()
               .InstancePerLifetimeScope();

        //// ========== CONTENT MANAGER (InstancePerLifetimeScope) ==========
        //// For Shell windows to manage content area
        builder.RegisterType<ContentManager>()
               .As<IContentManager>()
               .InstancePerLifetimeScope();

        // ========== NAVIGATOR (InstancePerLifetimeScope) ==========
        /// <summary>
        /// NAVIGATOR - Unified navigation and content management
        /// Replaces INavigationService and IContentManager
        /// Responsibilities:
        /// - Resolves and initializes ViewModels
        /// - Manages navigation stack and history
        /// - Provides window/shell close requests
        /// 
        /// USAGE:
        /// - Inject into Shell windows for ContentControl binding
        /// - Inject into ViewModels for programmatic navigation
        /// - Bind ContentControl.Content to Navigator.CurrentViewModel
        /// </summary>
        builder.RegisterType<Navigator>()
               .As<INavigator>()
               .InstancePerLifetimeScope();

        // ========== DIALOG SERVICE (InstancePerLifetimeScope) ==========
        /// <summary>
        /// DIALOG SERVICE - Modal dialogs with results
        /// Per-window instance for proper dialog ownership
        /// </summary>
        builder.RegisterType<DialogService>()
               .As<IDialogService>()
               .InstancePerLifetimeScope();

        builder.RegisterType<DialogHost>()
              .As<IDialogHost>()
              .InstancePerLifetimeScope();
    }
}

/// <summary>
/// Usage Notes:
/// 
/// 1. Opening Root Windows:
///    - Inject IWindowManager in root ViewModels (e.g., MainViewModel)
///    - Call windowManager.OpenWindow<TViewModel>()
/// 
/// 2. Opening Child Windows:
///    - Inject IWindowContext in window ViewModels
///    - Call windowContext.OpenChild<TViewModel>()
///    - Children automatically close when parent closes (scope cascade)
/// 
/// 3. Closing Windows:
///    - To close this window: windowContext.CloseWindow()
///    - To close children: windowContext.CloseChildren()
///    - To close specific window: windowManager.Close(windowId)
/// 
/// 4. Session Management (Future):
///    - Sessions will be handled by separate ISessionManager
///    - SessionManager will internally use IWindowManager
/// 
/// 5. Scope Hierarchy:
///    Root Container
///      ?? IWindowManager (Singleton)
///      ?? Window Scope (per window)
///           ?? IWindowContext (identifies THIS window)
///           ?? IContentManager (for shell windows)
///           ?? ViewModels
/// </summary>