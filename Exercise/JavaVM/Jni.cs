using Base.Misc;
using java.io;
using net.sf.jni4net;
using net.sf.jni4net.adaptors;
using System;

namespace Exercise.JavaVM
{
    public class Jni
    {
        private static readonly Logger Log = Logger.GetLogger<Jni>();

        public static void Init()
        {
            // create bridge, with default setup
            // it will lookup jni4net.j.jar next to jni4net.n.dll
            var bridgeSetup = new BridgeSetup();
            bridgeSetup.AddAllJarsClassPath(".");
            //bridgeSetup.Verbose = true;
            bridgeSetup.AddJVMOption("-Xmx512m");
            while (true)
            {
                try
                {
                    Bridge.CreateJVM(bridgeSetup);
                    break;
                }
                catch (Exception e)
                {
                    Log.w(e);
                    if (bridgeSetup.IgnoreJavaHome)
                        throw;
                    else
                        bridgeSetup.IgnoreJavaHome = true;
                }
            }

            // here you go!
            java.lang.System.err.println("Hello Java world!");

            // OK, simple hello is boring, let's play with Java properties
            // they are Hashtable realy
            java.util.Properties javaSystemProperties = java.lang.System.getProperties();

            // let's enumerate all keys. 
            // We use Adapt helper to convert enumeration from java o .NET
            foreach (java.lang.String key in Adapt.Enumeration(javaSystemProperties.keys()))
            {
                java.lang.System.err.print(key);

                // this is automatic conversion of CLR string to java.lang.String
                java.lang.System.err.print(" : ");

                // we use the hashtable
                java.lang.Object value = javaSystemProperties.get(key);

                // and this is CLR ToString() redirected to Java toString() method
                string valueToString = value.ToString();
                java.lang.System.err.println(valueToString);
            }

            // Java output is really Stream
            PrintStream stream = java.lang.System.err;

            // it implements java.io.Flushable interface
            Flushable flushable = stream;
            flushable.flush();

            java.lang.System.setOut(java.lang.System.err);
        }
    }
}
