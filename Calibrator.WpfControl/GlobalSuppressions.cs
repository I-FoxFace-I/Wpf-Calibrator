// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("StyleCop.CSharp.NamingRules", "SA1309:FieldNamesMustNotBeginWithUnderscore", Justification = "Reviewed.")]
[assembly: SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:ElementsMustAppearInTheCorrectOrder", Justification = "Reviewed.")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1649:FileNameMustMatchTypeName", Justification = "Reviewed.")]
[assembly: SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleType", Justification = "Reviewed.")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed.")]
[assembly: SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1028:Code should not contain trailing whitespace", Justification = " Reviewed.", Scope = "type")]
[assembly: SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1200:Using directives should be placed correctly", Justification = "Reviwed")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1642:ConstructorSummaryDocumentationMustBeginWithStandardText", Justification = "Reviewed.")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1629:DocumentationTextMustEndWithAPeriod", Justification = "Reviewed.")]
[assembly: SuppressMessage("Major Code Smell", "S125:Sections of code should not be commented out", Justification = "<Pending>", Scope = "type", Target = "~T:Calibrator.WpfControl.Controls.ScTextBox.ScTextBoxComponent")]
[assembly: SuppressMessage("Minor Code Smell", "S4136:Method overloads should be grouped together", Justification = "<Pending>", Scope = "member", Target = "~M:Calibrator.WpfControl.Controls.ScTextBox.ScTextBoxComponent.OnLostFocus(System.Object,System.Windows.RoutedEventArgs)")]
[assembly: SuppressMessage("Major Code Smell", "S4144:Methods should not have identical implementations", Justification = "<Pending>", Scope = "member", Target = "~M:Calibrator.WpfControl.Controls.UniForm.UniFormContainer.OnLayoutChanged(System.Windows.DependencyObject,System.Windows.DependencyPropertyChangedEventArgs)")]
[assembly: SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>", Scope = "member", Target = "~M:Calibrator.WpfControl.Controls.ScSmartTable.ScSmartTableComponent.#ctor")]

// Additional StyleCop suppressions for non-critical formatting issues
[assembly: SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1101:PrefixLocalCallsWithThis", Justification = "Modern C# style - this prefix not required for clarity")]
[assembly: SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1503:BracesShouldNotBeOmitted", Justification = "Modern C# style - single line statements acceptable")]
[assembly: SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1413:UseTrailingCommaInMultiLineInitializers", Justification = "Optional formatting preference")]
[assembly: SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1122:UseStringEmptyForEmptyStrings", Justification = "Both string.Empty and \"\" are acceptable")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1633:FileMustHaveHeader", Justification = "File headers not required for internal components")]
[assembly: SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1124:DoNotUseRegions", Justification = "Regions help organize large files")]
[assembly: SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1111:ClosingParenthesisShouldBeOnLineOfLastParameter", Justification = "Formatting preference")]

// Additional StyleCop suppressions for formatting and documentation preferences
[assembly: SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1117:ParametersShouldBeOnSameLineOrSeparateLines", Justification = "Dependency property registrations are readable as-is")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1623:PropertySummaryDocumentationMustMatchAccessors", Justification = "Custom documentation style preferred")]
[assembly: SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1518:UseLineEndingsCorrectlyAtEndOfFile", Justification = "File endings handled by git")]
[assembly: SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1513:ClosingBraceShouldBeFollowedByBlankLine", Justification = "Compact code style preferred")]
[assembly: SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1507:CodeShouldNotContainMultipleBlankLinesInARow", Justification = "Spacing for readability")]
[assembly: SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1516:ElementsShouldBeSeparatedByBlankLine", Justification = "Compact assembly attributes")]
[assembly: SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1206:DeclarationKeywordsShouldFollowOrder", Justification = "new static pattern acceptable")]
[assembly: SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1202:ElementsMustBeOrderedByAccess", Justification = "Logical grouping preferred over access order")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1615:ElementReturnValueShouldBeDocumented", Justification = "Simple factory methods don't need return documentation")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1404:CodeAnalysisSuppressionShouldHaveJustification", Justification = "Justifications provided where needed")]
[assembly: SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1025:CodeShouldNotContainMultipleWhitespaceInARow", Justification = "Alignment for readability")]
[assembly: SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1024:ColonShouldBeFollowedBySpace", Justification = "Interface implementation style")]
[assembly: SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1009:ClosingParenthesisShouldNotBePrecededBySpace", Justification = "Formatting preference")]
[assembly: SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1005:SingleLineCommentShouldBeginWithSpace", Justification = "Commented code blocks")]

// Additional StyleCop suppressions for remaining formatting issues
[assembly: SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1208:SystemUsingDirectivesMustBePlacedBeforeOtherUsingDirectives", Justification = "Global usings handle system directives")]
[assembly: SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1204:StaticElementsMustAppearBeforeInstanceElements", Justification = "Logical grouping preferred")]
[assembly: SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1116:SplitParametersMustStartOnLineAfterDeclaration", Justification = "Compact parameter style acceptable")]
[assembly: SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1508:ClosingBracesShouldNotBePrecededByBlankLine", Justification = "Spacing for readability")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1611:ElementParametersMustBeDocumented", Justification = "Self-explanatory parameters don't need documentation")]

// Final StyleCop suppressions for remaining minor formatting issues
[assembly: SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1134:AttributesShouldNotShareLine", Justification = "ObservableProperty attribute style acceptable")]