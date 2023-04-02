#if UNITY_EDITOR && ODIN_INSPECTOR
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;

internal sealed class SubclassSelectorAttributeProcessor<T> : OdinAttributeProcessor<T> where T : class
{
	#region Internal Members

	private SubclassSelectorAttribute subclassSelector;
	private ICollectionResolver collectionResolver;

	private bool isCollection = false;
	private bool parentIsCollection = false;

	#endregion

	#region Processor Overrides

	public override bool CanProcessSelfAttributes(InspectorProperty property)
	{
		if ((subclassSelector = property.Attributes.GetAttribute<SubclassSelectorAttribute>()) == null)
		{
			return false;
		}

		bool isUnityObject;
		collectionResolver = property.ChildResolver as ICollectionResolver;
		isCollection = collectionResolver != null;

		if (isCollection)
		{
			Type elementType = collectionResolver.ElementType;
			isUnityObject = typeof(UnityEngine.Object).IsAssignableFrom(elementType);
			return !isUnityObject;
		}
		else
		{
			collectionResolver = property.ParentValueProperty?.ChildResolver as ICollectionResolver ?? null;
			parentIsCollection = collectionResolver != null;
			isUnityObject = typeof(UnityEngine.Object).IsAssignableFrom(property.ValueEntry.BaseValueType);

			if (parentIsCollection)
			{
				return !isUnityObject && subclassSelector.DrawDropdownForListElements;
			}
			else
			{
				return !isUnityObject;
			}
		}
	}

	public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
	{
		var hideReferencePicker = property.Attributes.GetAttribute<HideReferenceObjectPickerAttribute>();
		if (subclassSelector.HideReferencePicker && hideReferencePicker == null)
		{
			hideReferencePicker = new HideReferenceObjectPickerAttribute();
			attributes.Add(hideReferencePicker);
		}

		var hideLabel = property.Attributes.GetAttribute<HideLabelAttribute>();

		if (subclassSelector.HideClassLabel && hideLabel == null)
		{
			hideLabel = new HideLabelAttribute();
			attributes.Add(hideLabel);
		}

		if (isCollection)
		{
			var labelText = new LabelTextAttribute(property.NiceName);
			attributes.Add(labelText);

			var lds = property.Attributes.GetAttribute<ListDrawerSettingsAttribute>();

			if (lds == null)
			{
				lds = new ListDrawerSettingsAttribute();
				attributes.Add(lds);
			}

			lds.CustomAddFunction = SubclassSelectorUtilities.OpenSubclassSelectorString;

			if (subclassSelector.DrawBoxForListElements)
			{
				lds.OnBeginListElementGUI = SubclassSelectorUtilities.OnBeginBoxSubclassElementString;
				lds.OnEndListElementGUI = SubclassSelectorUtilities.OnEndBoxSubclassElementString;
			}
		}
		else
		{
			var typeFilter = new TypeFilterAttribute(SubclassSelectorUtilities.TypeFilterResolverString);

			if (parentIsCollection)
			{
				typeFilter.DrawValueNormally = true;
			}
			else
			{
				bool valueIsNull = property.ValueEntry.WeakSmartValue == null;
				bool hidePicker = subclassSelector.HideReferencePicker;
				typeFilter.DrawValueNormally = !valueIsNull || !hidePicker;
			}

			attributes.Add(typeFilter);
		}
	}

	#endregion
}
#endif