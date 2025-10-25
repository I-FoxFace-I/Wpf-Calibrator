# WPF Engine - Manual Testing Guide

This guide provides instructions for manually testing features that are difficult or impossible to automate in unit tests.

## ✅ What's Already Automated

**81 Unit Tests** covering:
- ✅ STA Thread Tests (4 tests) - `ViewLocatorServiceTests` with `[STAFact]`
- ✅ Scope Tag Definitions (14 tests)
- ✅ Content Management (11 tests)
- ✅ View Registry (8 tests)
- ✅ Navigation Service (13 tests)
- ✅ ViewModel Factory (7 tests)
- ✅ Window Service Basic (4 tests)
- ✅ Window Service Advanced (7 tests)
- ✅ Dialog Service (4 tests)
- ✅ Shell ViewModel (6 tests)
- ✅ Session Scope Concept Tests (3 tests)

---

## 🎯 Manual Testing Required

The following scenarios **cannot be easily automated** due to:
- Complex reflection requirements for internal WindowService state
- Real WPF window/UI interactions
- Multi-session parallel execution
- Actual Autofac scope tag matching at runtime

### 📋 Test Checklist

Use this checklist when performing manual testing:

- [ ] **Scenario 1:** Basic window management
- [ ] **Scenario 2:** Workflow session - shared state
- [ ] **Scenario 3:** Multiple workflows in parallel
- [ ] **Scenario 4:** Child window management
- [ ] **Scenario 5:** Dialog windows with results
- [ ] **Scenario 6:** Content navigation (shell)
- [ ] **Scenario 7:** Memory leak detection
- [ ] **Scenario 8:** Session scope sharing (CRITICAL!)

---

## Scenario 8: Session Scope Sharing ⭐ MOST IMPORTANT

**Purpose:** Verify that `InstancePerMatchingLifetimeScope` works correctly with session scopes.

**What to Test:**

### Test 8.1: Shared Service in Same Session

**Steps:**
1. Run `WpfEngine.Demo`
2. Open "Workflow Demo"
3. In Step 1: Add logging to `IOrderBuilderService` constructor
4. Click "Select Customer" → Select a customer
5. Return to Step 1 → Click "Select Customer" again
6. Check logs

**Expected Result:**
- `IOrderBuilderService` constructor called **ONCE** when session starts
- Same instance used for all windows in the session
- Customer selection persists

**Verifies:**
```csharp
builder.RegisterType<OrderBuilderService>()
       .As<IOrderBuilderService>()
       .InstancePerMatchingLifetimeScope((scope, reg) =>
       {
           var tag = scope.Tag?.ToString() ?? "";
           return tag.StartsWith("WorkflowSession:");
       });
```

### Test 8.2: Different Sessions Have Different Instances

**Steps:**
1. Open first workflow → Select Customer A
2. **Don't close it**
3. Open second workflow → Select Customer B
4. Switch to first workflow
5. Verify it still shows Customer A

**Expected Result:**
- Two different `IOrderBuilderService` instances
- Independent state between workflows
- No cross-contamination

**Verifies:**
- Each session gets its own instance
- Sessions are isolated

### Test 8.3: Child Windows Share Parent Session Service

**Steps:**
1. Open workflow
2. In Step 2: Click "Add Products" (opens child window)
3. Select products in child window
4. Close child window
5. Verify products appear in Step 2

**Expected Result:**
- Child window sees same `IOrderBuilderService` as parent
- Modifications in child visible in parent
- Shared state works across window hierarchy

**Verifies:**
```
Session Scope (WorkflowSession:xxx)
    ├─ Window Scope (Window:Step1)
    ├─ Window Scope (Window:Step2)
    │   └─ Window Scope (Window:ProductSelector) ← Inherits session service!
    └─ Window Scope (Window:Step3)
```

### Test 8.4: Window-Specific Services Are Unique

**Steps:**
1. Open workflow
2. Add logging to any `InstancePerMatchingLifetimeScope("Window:")` service
3. Open Step 1, Step 2, Step 3
4. Check logs

**Expected Result:**
- Window-specific service constructor called **3 times** (once per window)
- Each window has its own instance
- But all share the same session service

**Verifies:**
```csharp
builder.RegisterType<WindowSpecificService>()
       .As<IWindowSpecificService>()
       .InstancePerMatchingLifetimeScope((scope, reg) =>
       {
           var tag = scope.Tag?.ToString() ?? "";
           return tag.StartsWith("Window:");
       });
```

### Test 8.5: Session Disposal Disposes Shared Services

**Steps:**
1. Add `IDisposable` to `OrderBuilderService` with logging
2. Open workflow
3. Close the workflow host window
4. Check logs

**Expected Result:**
- `OrderBuilderService.Dispose()` called when session closes
- All session windows closed
- No memory leaks

**Verifies:**
- Session scope disposal works correctly
- Services are cleaned up

---

**Test Steps:**
1. Run `WpfEngine.Demo`
2. Click "Demo Menu" → "Customer List"
3. Select a customer → Click "View Details"
4. Modify customer data → Click "Save"
5. Verify the list refreshes automatically

**Expected Result:**
- Detail window opens
- Changes are saved
- List updates when detail window closes

**Verifies:**
- Window opening/closing
- ViewModel lifecycle
- Data refresh mechanism

---

### Scenario 2: Workflow Session - Shared State

**Test Steps:**
1. Run `WpfEngine.Demo`
2. Click "Demo Menu" → "Workflow Demo"
3. In Step 1: Click "Select Customer"
4. Choose a customer → Click "Next"
5. In Step 2: Click "Add Products"
6. Select products → Click "Add"
7. Click "Next" → Verify order summary in Step 3

**Expected Result:**
- Customer selection persists across steps
- Products added in Step 2 appear in Step 3
- All workflow windows share the same `IOrderBuilderService`

**Verifies:**
- Session-scoped shared services
- `InstancePerMatchingLifetimeScope` behavior
- State persistence across workflow steps

---

### Scenario 3: Multiple Workflows in Parallel

**Test Steps:**
1. Run `WpfEngine.Demo`
2. Open first workflow: "Demo Menu" → "Workflow Demo"
3. Select Customer A → Add Product X
4. Don't close the workflow
5. Open second workflow: "Demo Menu" → "Workflow Demo" again
6. Select Customer B → Add Product Y
7. Switch between workflows → Verify independent state

**Expected Result:**
- Two workflow sessions run independently
- Customer A and Product X in first workflow
- Customer B and Product Y in second workflow
- No shared state between the two workflows

**Verifies:**
- Multiple session instances
- Session isolation
- Independent `IOrderBuilderService` per session

---

### Scenario 4: Child Window Management

**Test Steps:**
1. Open "Customer List"
2. Open 3 customer detail windows (different customers)
3. Close the parent "Customer List" window

**Expected Result:**
- All child detail windows close automatically
- No orphaned windows remain

**Verifies:**
- Parent-child window tracking
- Cascade closing behavior
- Proper disposal

---

### Scenario 5: Dialog Windows with Results

**Test Steps:**
1. Navigate to any form with a "Select Product" button
2. Click "Select Product"
3. Choose a product → Click "OK"
4. Verify product appears in the form

**Expected Result:**
- Dialog opens modally
- Selection returns to caller
- Dialog closes properly

**Verifies:**
- `IDialogService` functionality
- `IDialogViewModel<TResult>` pattern
- Result passing

---

### Scenario 6: Content Navigation (Shell)

**Test Steps:**
1. Open "Workflow Demo" (uses ShellViewModel)
2. Navigate through: Step 1 → Step 2 → Step 3
3. Click "Back" to return to Step 2
4. Click "Back" again to return to Step 1

**Expected Result:**
- Content area updates with each navigation
- History is maintained
- Back navigation works correctly
- Previous ViewModels are disposed

**Verifies:**
- `IContentManager` navigation
- History management
- ViewModel disposal

---

### Scenario 7: Memory Leak Detection

**Test Steps:**
1. Run `WpfEngine.Demo` with a memory profiler (e.g., dotMemory)
2. Open/close 100 customer detail windows
3. Force garbage collection
4. Check for retained ViewModels or Windows

**Expected Result:**
- All closed windows are garbage collected
- No memory leaks from ViewModels
- Disposable services are properly disposed

**Verifies:**
- Proper disposal patterns
- No circular references
- Memory management

---

## 📊 Performance Testing

### Load Test: Many Windows

**Test Steps:**
1. Modify demo to open 50 customer detail windows
2. Measure:
   - Time to open all windows
   - Memory consumption
   - UI responsiveness

**Expected Result:**
- All windows open < 5 seconds
- Memory growth is linear
- UI remains responsive

---

## 🐛 Error Handling

### Test: Missing View Registration

**Test Steps:**
1. Create a ViewModel without registering its View
2. Try to open a window for that ViewModel

**Expected Result:**
- Clear exception message
- Points to ViewRegistry issue
- Application doesn't crash

---

### Test: Invalid Scope Access

**Test Steps:**
1. Try to resolve a session-scoped service from root scope
2. Observe error handling

**Expected Result:**
- Clear exception about scope mismatch
- Guidance on proper registration

---

## ✅ Test Completion Checklist

Use this checklist when performing manual testing:

- [ ] Basic window management works
- [ ] Workflow session state is shared correctly
- [ ] Multiple parallel workflows are isolated
- [ ] Child windows close with parent
- [ ] Dialogs return results properly
- [ ] Content navigation and history work
- [ ] No memory leaks detected
- [ ] Performance is acceptable
- [ ] Error messages are clear

---

## 🔧 Troubleshooting

### Issue: Windows don't close properly

**Check:**
- Is `IWindowService.Close()` being called?
- Is the correct window ID or VmKey used?
- Are event handlers unsubscribed in Dispose()?

### Issue: Shared service not working

**Check:**
- Is the service registered with `InstancePerMatchingLifetimeScope`?
- Is the scope tag correct (must start with "WorkflowSession:")?
- Is the window opened within a session scope?

### Issue: Memory leaks

**Check:**
- Are all ViewModels implementing `IDisposable`?
- Are event handlers unsubscribed?
- Are window references released?

---

## 📝 Reporting Issues

When reporting issues from manual testing, include:

1. **Steps to reproduce**
2. **Expected behavior**
3. **Actual behavior**
4. **Screenshots** (if applicable)
5. **Log output** (check console for `[WINDOW]`, `[SESSION]`, `[NAV]` tags)

---

**Last Updated:** 2025-10-25  
**Test Coverage:** Unit tests (79 automated) + Manual scenarios (7)

