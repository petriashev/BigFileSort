using System.Text;
using BigFileSort.Domain;

namespace BigFileSort;

public static class Tests
{
    public static void Sample()
    {
        {
            Dictionary<VirtualString, List<VirtualString>> index = new Dictionary<VirtualString, List<VirtualString>>();
            string text = "5. Apple\r\n120. Apple";
            var bytes1 = Encoding.ASCII.GetBytes(text);
            var bytes2 = Encoding.UTF8.GetBytes(text);
            var charArray = bytes2.ToCharArray();
            var num1 = new VirtualString(bytes1, 0, 1);
            var str1 = new VirtualString(bytes1, 3, 5);
        
            var num2 = new VirtualString(bytes1, 10, 3);
            var str2 = new VirtualString(bytes1, 15, 5);

            var hashCode1 = str1.GetHashCode();
            var hashCode2 = str2.GetHashCode();
            var b = str1 == str2;

            index[str1] = new List<VirtualString> {num1};
            index[str2].Add(num2); 
        }
        
        {
            string text = "1658.166389.15476";
            var bytes1 = Encoding.ASCII.GetBytes(text);
            var num1 = new VirtualString(bytes1, 0, 4);
            var num2 = new VirtualString(bytes1, 5, 6);
            var num3 = new VirtualString(bytes1, 12, 5);

            var list = new [] { num1, num2, num3 };
            var sorted = list.OrderBy(s => s, VirtualStringComparer.ValueAsNumber).ToArray();

            int i = 0;

            //1658. String_141683
            //166389. String_141683
        }
        
    }

}