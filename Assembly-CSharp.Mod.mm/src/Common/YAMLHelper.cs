using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using YamlDotNet.Serialization;

public static class YamlHelper {

    public static Deserializer Deserializer = new DeserializerBuilder().Build();
    public static Serializer Serializer = new SerializerBuilder().Build();

}
