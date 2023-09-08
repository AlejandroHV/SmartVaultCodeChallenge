namespace SmartVault.DataGeneration.Util
{
    public class Utilities
    {

        public static DateTime RandomDay(Random random)
        {
            DateTime start = new DateTime(1985, 1, 1);
            int range = (DateTime.Today - start).Days;
            return start.AddDays(random.Next(range));
        }
    }
}
