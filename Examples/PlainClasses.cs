namespace LBG.Demo
{
	using UnityEngine;
	using LizardBrainGames.Serialisation;

	public static class PlainClasses
	{
		public interface IBaseInterface { }

		[System.Serializable]
		[SubclassPath(SubClassName = "Base Class")]
		[AllowedSubtypes(typeof(ClassA), typeof(ClassB))]
		public class ClassBase : IBaseInterface
		{
			[field: SerializeField, HideInDerivedTypes]
			public string TestString { get; set; }
		}

		[System.Serializable]
		[SubclassPath("Derived", "Class A")]
		public class ClassA : ClassBase
		{
			[SerializeField]
			private float _ClassAFloat;
		}

		[System.Serializable]
		[SubclassPath("Derived", "Class B")]
		public class ClassB : ClassA
		{
			[SerializeField]
			private GameObject _classCGameObject;
		}

		[System.Serializable]
		[SubclassPath("Derived", "Class C")]
		public class ClassC : ClassBase
		{
			[SerializeField]
			private bool _classCBool;

			public bool ClassCBool => _classCBool;
		}
	}
}