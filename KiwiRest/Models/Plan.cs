namespace KiwiRest.Models
{
	public static class Plans
	{
		public static readonly Plan Basic = new Plan
		{
			MaxEntries = 1000,
			Name = "plan.basic"
		};

		public static readonly Plan Premium = new Plan
		{
			MaxEntries = 50000,
			Name = "plan.premium"
		};

		public static Plan? GetPlanByName(string name)
		{
			switch (name.ToLower())
			{
				case "plan.basic":
					return Basic;
				case "plan.premium":
					return Premium;
				default:
					return null;
			}
		}
	}
	public struct Plan
	{
		public string Name;
		public int MaxEntries;
	}
}