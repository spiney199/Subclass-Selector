public static class PlainClasses
{
	public interface IBaseInterface { }

	[System.Serializable]
	[SubclassPath(SubClassName = "Base Class")]
	public class ClassBase : IBaseInterface
	{
		public string TestString;
	}

	[System.Serializable]
	[SubclassPath("Derived", "Class A")]
	public class ClassA : ClassBase
	{
		public float ClassAFloat;
	}

	[System.Serializable]
	[SubclassPath("Derived", "Class B")]
	public class ClassB : ClassA
	{
		public GameObject ClassCGameObject;
	}

	[System.Serializable]
	[SubclassPath("Derived", "Class C")]
	public class ClassC : ClassBase
	{
		public ClassC(ClassBase classBase)
		{
			this.TestString = classBase.TestString;
			this._classCBool = true;
		}

		public bool ClassCBool;
	}
}