using System;

/// <summary>
/// Use this attribute on plain class types to determine their path when using <see cref="SubclassSelectorAttribute"/>.
/// </summary>
[System.Diagnostics.Conditional("UNITY_EDITOR")]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class SubclassPathAttribute : Attribute
{
	#region Constructors

	/// <summary>
	/// Empty constructor. The types names in <see cref="SubclassSelectorAttribute"/> will not be modified.
	/// </summary>
	public SubclassPathAttribute() { }

	/// <summary>
	/// Determines the path for types displayed in <see cref="SubclassPathAttribute"/>.
	/// The default type name will be used.
	/// </summary>
	/// <param name="path">The path for this type.</param>
	public SubclassPathAttribute(string path)
	{
		SubClassPath = path;
	}

	/// <summary>
	/// Determines both the path and the display name for types displayed in <see cref="SubclassSelectorAttribute"/>.
	/// </summary>
	/// <param name="path">The path for this type.</param>
	/// <param name="subclassName">The display name for this type.</param>
	public SubclassPathAttribute(string path, string subclassName)
	{
		SubClassPath = path;
		SubClassName = subclassName;
	}

	#endregion

	#region Internal Members

	private string _subClassPath = string.Empty;

	private string _subClassName = string.Empty;

	#endregion

	#region Properties

	/// <summary>
	/// The path for this type as used by <see cref="SubclassSelectorAttribute"/>.
	/// </summary>
	public string SubClassPath
	{
		get => _subClassPath;
		set
		{
			HasSubClassPath = !string.IsNullOrEmpty(value);
			_subClassPath = value;
		}
	}

	/// <summary>
	/// The display name for this type as used by <see cref="SubclassSelectorAttribute"/>.
	/// </summary>
	public string SubClassName
	{
		get => _subClassName;
		set
		{
			HasSubClassName = !string.IsNullOrEmpty(value);
			_subClassName = value;
		}
	}

	public bool HasSubClassPath { get; private set; } = false;

	public bool HasSubClassName { get; private set; } = false;

	#endregion
}