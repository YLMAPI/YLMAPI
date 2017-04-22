using System;
using System.Reflection;
using MMILAccess;

namespace MonoMod {
    public class MMILAccessExample {

        public readonly static MethodBase[] Methods = new BatchAccess<MMILAccessExample>().AllMethods;
        public readonly static FieldInfo[] Fields = new BatchAccess<MMILAccessExample>().AllFields;

        public int FieldA = 42;
        public int FieldB = 21;
        public int FieldC;
        public int FieldD;
        public int FieldE;
        public string SomeStringProperty { get; set; } = "original";

        public void Run() {
            try {
                Console.WriteLine("Running MMILAccessExample!");

                for (int i = 0; i < Methods.Length; i++) {
                    Console.WriteLine($"Method {i}: {Methods[i]}");
                }
                for (int i = 0; i < Fields.Length; i++) {
                    Console.WriteLine($"Field {i}: {Fields[i]}");
                }

                new Access<MMILAccessExample>(this, "RunSomethingElse").Call();
                new StaticAccess<MMILAccessExample>("StaticTest").Call("World!", 42);
                new StaticAccess<MMILAccessExample>("StaticTest").Call("nested calls!", new StaticAccess<MMILAccessExample>("Add").Call<int>(8, 13));

                FieldA = 25;
                FieldB = 4;
                SomeStringProperty = "whatever";

                MMILAccessExample other = new MMILAccessExample();
                new BatchAccess<MMILAccessExample>(this).With("FieldA", "SomeStringProperty").CopyTo(other);

                Console.WriteLine($"Copied values: {other.FieldA} {other.FieldB} {other.SomeStringProperty}");

                Console.WriteLine("MMILAccessExample didn't burn down!");
            } catch (Exception e) {
                Console.WriteLine($"MMILAccessExample failed! {e}");
            }
        }

        public void RunSomethingElse() {
            Console.WriteLine("Something else running!");
        }

        public static int Add(int a, int b) => a + b;
        public static void StaticTest(string str, int num) => Console.WriteLine("Hello, " + str + " " + num);

    }
}
