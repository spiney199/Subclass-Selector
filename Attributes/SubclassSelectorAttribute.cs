using System;

namespace Spiney.SubclassSelector
{
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

		#endregion

		#region Properties

		/// <summary>
		/// If false, duplicate won't be selectable in the collection selector, or collection element dropdowns.
		/// </summary>
		/// <remarks>
		/// Only affects collections. True by default.
		/// </remarks>
		public bool AllowDuplicates { get; set; } = true;

		/// <summary>
		/// Will hide the Odin default object reference picker if true as it it has been
		/// decorated with <see cref="Sirenix.OdinInspector.HideReferenceObjectPickerAttribute"/>.
		/// </summary>
		/// <remarks>
		/// True by default.
		/// </remarks>
		public bool HideReferencePicker { get; set; } = true;

		/// <summary>
		/// If true, <see cref="Sirenix.OdinInspector.TypeFilterAttribute"/> dropdowns will be drawn at the top
		/// of each list element.
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
		/// Only affects non-collection fields. True by default.
		/// </remarks>
		public bool HideClassLabel { get; set; } = true;

		/// <summary>
		/// Provide a value resolver expression to filter types after the intial collection of subtypes has been generated.
		/// </summary>
		/// <remarks>
		/// Abstract types are already filtered.
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

		#endregion
	}
}