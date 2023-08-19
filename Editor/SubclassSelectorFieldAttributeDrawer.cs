#if UNITY_EDITOR && ODIN_INSPECTOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using Sirenix.OdinInspector.Editor.ActionResolvers;

/// <summary>
/// Attribute drawer for <see cref="SubclassSelectorAttribute"/> fields. Draws a selector dropdown that respects
/// <see cref="SubclassPathAttribute"/>, including field name by default, and a foldout if desired.
/// </summary>
[DrawerPriority(value: 3000)]
internal sealed class SubclassSelectorFieldAttributeDrawer<T> : OdinAttributeDrawer<SubclassSelectorAttribute, T> where T : class
{
	#region Internal Members

	private ActionResolver _onTypesSelected;

	#endregion

	#region Drawer Overrides

	protected override bool CanDrawAttributeValueProperty(InspectorProperty property)
	{
		bool isCollection = property.ChildResolver is ICollectionResolver;
		var attr = property.Attributes.GetAttribute<SubclassSelectorAttribute>();

		if (isCollection)
		{
			return false;
		}

		if (property.ParentValueProperty.ChildResolver is ICollectionResolver collectionResolver) // parent is collection
		{
			Type elementType = collectionResolver.ElementType;
			bool isUnityObject = typeof(UnityEngine.Object).IsAssignableFrom(elementType);
			return !isUnityObject && attr.DrawDropdownForListElements;
		}
		else
		{
			bool isUnityObject = typeof(UnityEngine.Object).IsAssignableFrom(property.ValueEntry.BaseValueType);
			return !isUnityObject;
		}
	}

	protected override void Initialize()
	{
		var attr = this.Attribute;
		var property = this.Property;

		if (attr.HasOnTypesSelected)
		{
			_onTypesSelected = ActionResolver.Get(property, attr.OnTypesSelected,
				new Sirenix.OdinInspector.Editor.ActionResolvers.NamedValue("type", typeof(Type)));
		}
	}

	protected override void DrawPropertyLayout(GUIContent label)
	{
		_onTypesSelected?.DrawError();

		this.DrawDropdown(label);
	}

	#endregion

	#region Internal Methods

	private void DrawDropdown(GUIContent label)
	{
		var attr = this.Attribute;
		var property = this.Property;

		EditorGUI.BeginChangeCheck();

		IEnumerable<Type> result = null;
		string valueName = this.GetCurrentValueName();

		if (!this.Attribute.HideReferencePicker)
		{
			result = GenericSelector<Type>.DrawSelectorDropdown(label, valueName, this.ShowSelector);
			this.CallNextDrawer(label);
		}
		else if (this.Property.Children.Count > 0)
		{
			if (attr.DrawClassFoldout)
			{
				this.Property.State.Expanded = SirenixEditorGUI.Foldout(this.Property.State.Expanded, label, out Rect valRect);
				result = GenericSelector<Type>.DrawSelectorDropdown(valRect, valueName, this.ShowSelector);

				if (SirenixEditorGUI.BeginFadeGroup(this, this.Property.State.Expanded))
				{
					DrawChildren(property);
				}
				SirenixEditorGUI.EndFadeGroup();
			}
			else
			{
				result = GenericSelector<Type>.DrawSelectorDropdown(label, valueName, this.ShowSelector);
				DrawChildren(property);
			}

			// local method
			static void DrawChildren(InspectorProperty property)
			{
				int count = property.Children.Count;
				for (int i = 0; i < count; i++)
				{
					var child = property.Children[i];
					child.Draw(child.Label);
				}
			}
		}
		else
		{
			result = GenericSelector<Type>.DrawSelectorDropdown(label, valueName, this.ShowSelector);
		}

		if (EditorGUI.EndChangeCheck() && result != null)
		{
			Type type = result.Count() > 0 ? result.First() : null;
			if (type != null)
			{
				this.UpdateSelectedType(type);
			}
		}
	}

	private OdinSelector<Type> ShowSelector(Rect rect)
	{
		var selector = this.CreateSelector();

		rect.x = (int)rect.x;
		rect.y = (int)rect.y;
		rect.width = (int)rect.width;
		rect.height = (int)rect.height;

		selector.ShowInPopup(rect, new Vector2(0, 0));
		return selector;
	}

	private GenericSelector<Type> CreateSelector()
	{
		IEnumerable<ValueDropdownItem<Type>> selections = SubclassSelectorUtilities.GetSubclassSelectorDropdownItems(this.Property);
		var selector = new GenericSelector<Type>("Select Types", false, selections.Select(x => new GenericSelectorItem<Type>(x.Text, x.Value)));

		selector.CheckboxToggle = false;
		selector.EnableSingleClickToSelect();

		foreach (var menuItem in selector.SelectionTree.EnumerateTree())
		{
			// assign folder items to dropdown menu items
			if (menuItem.ChildMenuItems.Count > 0 && menuItem.Value == null)
			{
				menuItem.Icon = EditorIcons.Folder.Raw;
				continue;
			}

			Type itemType = (Type)menuItem.Value;
			bool hasDefaultConstructor = itemType.GetConstructor(Type.EmptyTypes) != null;

			if (hasDefaultConstructor || this.Attribute.HasOnTypesSelected)
			{
				menuItem.Icon = EditorIcons.File.Raw;
			}
			else
			{
				menuItem.Icon = EditorIcons.AlertTriangle.Raw;
				menuItem.Name = $"{menuItem.Name} - No Default Constructor";
			}
		}

		return selector;
	}

	private string GetCurrentValueName()
	{
		if (!EditorGUI.showMixedValue)
		{
			if (this.ValueEntry.SmartValue != null)
			{
				Type currentType = this.ValueEntry.TypeOfValue;
				return SubclassSelectorUtilities.GetTypeSubclassPath(currentType);
			}
			else
			{
				return "Null";
			}
		}
		else
		{
			return SirenixEditorGUI.MixedValueDashChar;
		}
	}

	private void UpdateSelectedType(Type type)
	{
		if (_onTypesSelected != null && !_onTypesSelected.HasError)
		{
			_onTypesSelected.Context.NamedValues.Set("type", type);
			_onTypesSelected.DoActionForAllSelectionIndices();
		}
		else
		{
			var instance = SubclassSelectorUtilities.GetInstanceFromType(type);
			this.ValueEntry.WeakSmartValue = instance;
		}
	}

	#endregion
}
#endif