#if UNITY_EDITOR && ODIN_INSPECTOR
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;

/// <summary>
/// Attribute processor for <see cref="SubclassPathAttribute"/>. 
/// For collections it will assign a <see cref="ListDrawerSettingsAttribute.CustomAddFunction"/> to enable the type selector.
/// For fields it will apply <see cref="HideLabelAttribute"/> is desired.
/// Will apply <see cref="HideReferenceObjectPickerAttribute"/> in both instances if desired.
/// </summary>
internal sealed class SubclassSelectorAttributeProcessor<T> : OdinAttributeProcessor<T> where T : class
{
	#region Internal Members

	private SubclassSelectorAttribute subclassSelector;
	private ICollectionResolver collectionResolver;

	private bool isCollection = false;

	#endregion

	#region Processor Overrides

	public override bool CanProcessSelfAttributes(InspectorProperty property)
	{
		if ((subclassSelector = property.Attributes.GetAttribute<SubclassSelectorAttribute>()) == null)
		{
			return false;
		}

		collectionResolver = property.ChildResolver as ICollectionResolver;
		isCollection = collectionResolver != null;

		if (!isCollection)
		{
			Type type = property.ValueEntry.TypeOfValue;
			bool isUnityObject = typeof(UnityEngine.Object).IsAssignableFrom(type);
			return !isUnityObject;
		}
		else
		{
			Type elementType = collectionResolver.ElementType;
			bool isUnityObject = typeof(UnityEngine.Object).IsAssignableFrom(elementType);
			return !isUnityObject;
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

		if (!isCollection)
		{
			var hideLabel = property.Attributes.GetAttribute<HideLabelAttribute>();

			if (subclassSelector.HideClassLabel && hideLabel == null)
			{
				hideLabel = new HideLabelAttribute();
				attributes.Add(hideLabel);
			}
		}
		else
		{
			var lds = property.Attributes.GetAttribute<ListDrawerSettingsAttribute>();

			if (lds == null)
			{
				lds = new ListDrawerSettingsAttribute();
				attributes.Add(lds);
			}

			lds.CustomAddFunction = SubclassSelectorUtilities.OpenSubclassSelectorString;
		}
	}

	#endregion
}
#endif