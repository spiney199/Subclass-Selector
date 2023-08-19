using System;

/// <summary>
/// Attribute that allows you to easily select from among the subclsses of a non-Unity Object, polymorphic reference value.
/// Supports both singular fields and collections of any type that Odin supports.
/// </summary>
[System.Diagnostics.Conditional("UNITY_EDITOR")]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class SubclassSelectorAttribute : Attribute
{
	#region Constructor

	public SubclassSelectorAttribute() { }

	#endregion

	#region Internal Members

	private string _customTypeFilter = string.Empty;
	private bool _hasCustomTypeFilter = false;

	private string _onTypesSelected = string.Empty;
	private bool _hasOnTypesSelected = false;

	#endregion

	#region Properties

	/// <summary>
	/// If false, duplicate types won't be available in the collection selector, or collection element dropdowns.
	/// </summary>
	/// <remarks>
	/// Only affects collections. True by default.
	/// </remarks>
	public bool AllowDuplicates { get; set; } = true;

	/// <summary>
	/// Will hide the Odin default object reference picker if true as if it had been 
	/// decorated with <see cref="Sirenix.OdinInspector.HideReferenceObjectPickerAttribute"/>.
	/// </summary>
	/// <remarks>
	/// True by default.
	/// </remarks>
	public bool HideReferencePicker { get; set; } = true;

	/// <summary>
	/// If true, the attribute drawer for <see cref="SubclassSelectorAttribute"/> fields will be used for each element,
	/// providing a dropdown to change the selected type of each list element.
	/// </summary>
	/// <remarks>
	/// Only affects collections. False by default.
	/// </remarks>
	public bool DrawDropdownForListElements { get; set; } = false;

	/// <summary>
	/// If true, will draw a BoxGroup-style box around each element of a collection.
	/// </summary>
	/// <remarks>
	/// False by default. Only affects collections.
	/// </remarks>
	public bool DrawBoxForListElements { get; set; } = false;

	/// <summary>
	/// If true, will hide the class' label as if it had been decorated with <see cref="Sirenix.OdinInspector.HideLabelAttribute"/>.
	/// </summary>
	/// <remarks>
	/// Only affects non-collection fields. False by default.
	/// </remarks>
	public bool HideClassLabel { get; set; } = false;

	/// <summary>
	/// Draws a dropdown similar to <see cref="Sirenix.OdinInspector.TypeFilterAttribute"/> if true.
	/// </summary>
	/// <remarks>
	/// Only affects non-collection fields.
	/// </remarks>
	public bool DrawClassFoldout { get; set; } = false;

	/// <summary>
	/// Provide a value resolver expression to filter types after the intial collection of subtypes has been generated.
	/// </summary>
	/// <remarks>
	/// Abstract types are already filtered. Provides a $type named value and expects a boolean return value.
	/// </remarks>
	public string CustomTypeFilter
	{
		get => _customTypeFilter;
		set
		{
			_hasCustomTypeFilter = !string.IsNullOrEmpty(value);
			_customTypeFilter = value;
		}
	}

	/// <summary>
	/// Is there a custom type filter expression?
	/// </summary>
	public bool HasCustomTypeFilter => _hasCustomTypeFilter;

	/// <summary>
	/// Provide a string expression to this to provide custom logic when selecting types, useful for providing specific 
	/// initialisation to instances. This acts similar to <see cref="Sirenix.OdinInspector.ListDrawerSettingsAttribute.CustomAddFunction"/> 
	/// in that you take complete control of adding instances to collection or field. 
	/// </summary>
	/// <remarks>
	/// Collections provide a $types named parameter and pass a collection of the selected types.
	/// Fields provide a $type named paramter passing the singular type that was selected.
	/// </remarks>
	public string OnTypesSelected
	{
		get => _onTypesSelected;
		set
		{
			_hasOnTypesSelected = !string.IsNullOrEmpty(value);
			_onTypesSelected = value;
		}
	}

	/// <summary>
	/// Is there a OnTypesSelected expression?
	/// </summary>
	public bool HasOnTypesSelected => _hasOnTypesSelected;

	#endregion
}