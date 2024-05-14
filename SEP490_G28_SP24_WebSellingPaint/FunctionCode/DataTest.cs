using System.Text;

namespace SEP490_G28_SP24_WebSellingPaint.FeatureCode
{
    public class DataTest
    {
        public static void DisplayQueryResults<T>(IEnumerable<T> query)
        {
            foreach (var item in query)
            {
                DisplayObjectProperties(item);
                Console.OutputEncoding = Encoding.UTF8;
                Console.WriteLine();
            }
        }

        public static void DisplayObjectProperties<T>(T obj)
        {
            if (obj == null)
            {
                Console.WriteLine("null");
                return;
            }
            var type = obj.GetType();
            var properties = type.GetProperties();

            foreach (var property in properties)
            {
                var value = property.GetValue(obj);
                Console.WriteLine($"{property.Name}: {value ?? "null"}");
            }
        }
    }
}
