using java.io;
using java.lang;
using net.sf.jni4net;
using net.sf.jni4net.adaptors;

namespace Exercise.Misc
{
    public class Jni
    {
        public static void Init()
        {
            // create bridge, with default setup
            // it will lookup jni4net.j.jar next to jni4net.n.dll
            var bridgeSetup = new BridgeSetup();
            bridgeSetup.AddAllJarsClassPath(".");
            bridgeSetup.Verbose = true;
            Bridge.CreateJVM(bridgeSetup);

            // here you go!
            java.lang.System.@out.println("Hello Java world!");

            // OK, simple hello is boring, let's play with Java properties
            // they are Hashtable realy
            java.util.Properties javaSystemProperties = java.lang.System.getProperties();

            // let's enumerate all keys. 
            // We use Adapt helper to convert enumeration from java o .NET
            foreach (java.lang.String key in Adapt.Enumeration(javaSystemProperties.keys()))
            {
                java.lang.System.@out.print(key);

                // this is automatic conversion of CLR string to java.lang.String
                java.lang.System.@out.print(" : ");

                // we use the hashtable
                Object value = javaSystemProperties.get(key);

                // and this is CLR ToString() redirected to Java toString() method
                string valueToString = value.ToString();
                java.lang.System.@out.println(valueToString);
            }

            // Java output is really Stream
            PrintStream stream = java.lang.System.@out;

            // it implements java.io.Flushable interface
            Flushable flushable = stream;
            flushable.flush();
        }
    }
}
