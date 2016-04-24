using System;
using System.IO;
using AutoPatterns.Runtime;
using NUnit.Framework;
using Shouldly;

namespace AutoPatterns.Tests.Runtime
{
    [TestFixture]
    public class TypeKeyTests
    {
        [Test]
        public void CanCreateTypeKeys()
        {
            //-- arrange & act

            TypeKey key1 = new TypeKey<Type>(typeof(IDisposable));
            TypeKey key2 = new TypeKey<Type, Type>(typeof(Stream), typeof(IDisposable));
            TypeKey key3 = new TypeKey<Type, Type, int>(typeof(Stream), typeof(IDisposable), 123);
            TypeKey key4 = new TypeKey<Type, Type, int, string>(typeof(Stream), typeof(IDisposable), 123, "ABC");

            //-- assert

            key1.Count.ShouldBe(1);
            key1[0].ShouldBe(typeof(IDisposable));
            key1.ToString().ShouldBe("System_IDisposable");

            key2.Count.ShouldBe(2);
            key2[0].ShouldBe(typeof(Stream));
            key2[1].ShouldBe(typeof(IDisposable));
            key2.ToString().ShouldBe("System_IO_Stream_System_IDisposable");

            key3.Count.ShouldBe(3);
            key3[0].ShouldBe(typeof(Stream));
            key3[1].ShouldBe(typeof(IDisposable));
            key3[2].ShouldBe(123);
            key3.ToString().ShouldBe("System_IO_Stream_System_IDisposable_123");

            key4.Count.ShouldBe(4);
            key4[0].ShouldBe(typeof(Stream));
            key4[1].ShouldBe(typeof(IDisposable));
            key4[2].ShouldBe(123);
            key4[3].ShouldBe("ABC");
            key4.ToString().ShouldBe("System_IO_Stream_System_IDisposable_123_ABC");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanCompareTypeKeysOfIdenticalStructure()
        {
            //-- arrange

            TypeKey key1 = new TypeKey<Type>(typeof(IDisposable));
            TypeKey key1B = new TypeKey<Type>(typeof(IDisposable));
            TypeKey key1C = new TypeKey<Type>(typeof(IFormattable));

            TypeKey key2 = new TypeKey<Type, Type>(typeof(Stream), typeof(IDisposable));
            TypeKey key2B = new TypeKey<Type, Type>(typeof(Stream), typeof(IDisposable));
            TypeKey key2C = new TypeKey<Type, Type>(typeof(MemoryStream), typeof(IDisposable));
            TypeKey key2D = new TypeKey<Type, Type>(typeof(Stream), typeof(IFormattable));

            TypeKey key3 = new TypeKey<Type, Type, int>(typeof(Stream), typeof(IDisposable), 123);
            TypeKey key3B = new TypeKey<Type, Type, int>(typeof(Stream), typeof(IDisposable), 123);
            TypeKey key3C = new TypeKey<Type, Type, int>(typeof(MemoryStream), typeof(IDisposable), 123);
            TypeKey key3D = new TypeKey<Type, Type, int>(typeof(Stream), typeof(IFormattable), 123);
            TypeKey key3E = new TypeKey<Type, Type, int>(typeof(Stream), typeof(IDisposable), 456);

            TypeKey key4 = new TypeKey<Type, Type, int, string>(typeof(Stream), typeof(IDisposable), 123, "ABC");
            TypeKey key4B = new TypeKey<Type, Type, int, string>(typeof(Stream), typeof(IDisposable), 123, "ABC");
            TypeKey key4C = new TypeKey<Type, Type, int, string>(typeof(MemoryStream), typeof(IDisposable), 123, "ABC");
            TypeKey key4D = new TypeKey<Type, Type, int, string>(typeof(Stream), typeof(IFormattable), 123, "ABC");
            TypeKey key4E = new TypeKey<Type, Type, int, string>(typeof(Stream), typeof(IDisposable), 456, "ABC");
            TypeKey key4F = new TypeKey<Type, Type, int, string>(typeof(Stream), typeof(IDisposable), 123, "DEF");

            //-- act

            var key1Compares = new[] { key1.Equals(key1B), key1.Equals(key1C) };
            var key2Compares = new[] { key2.Equals(key2B), key2.Equals(key2C), key2.Equals(key2D) };
            var key3Compares = new[] { key3.Equals(key3B), key3.Equals(key3C), key3.Equals(key3D), key3.Equals(key3E) };
            var key4Compares = new[] { key4.Equals(key4B), key4.Equals(key4C), key4.Equals(key4D), key4.Equals(key4E), key4.Equals(key4F) };

            //-- assert

            key1Compares.ShouldBe(new[] { true, false });
            key2Compares.ShouldBe(new[] { true, false, false });
            key3Compares.ShouldBe(new[] { true, false, false, false });
            key4Compares.ShouldBe(new[] { true, false, false, false, false });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void HashCodesOfIdenticalKeysAreEqual()
        {
            //-- arrange

            TypeKey key1 = new TypeKey<Type>(typeof(IDisposable));
            TypeKey key1B = new TypeKey<Type>(typeof(IDisposable));

            TypeKey key2 = new TypeKey<Type, Type>(typeof(Stream), typeof(IDisposable));
            TypeKey key2B = new TypeKey<Type, Type>(typeof(Stream), typeof(IDisposable));

            TypeKey key3 = new TypeKey<Type, Type, int>(typeof(Stream), typeof(IDisposable), 123);
            TypeKey key3B = new TypeKey<Type, Type, int>(typeof(Stream), typeof(IDisposable), 123);

            TypeKey key4 = new TypeKey<Type, Type, int, string>(typeof(Stream), typeof(IDisposable), 123, "ABC");
            TypeKey key4B = new TypeKey<Type, Type, int, string>(typeof(Stream), typeof(IDisposable), 123, "ABC");

            //-- act

            var hash1 = new[] { key1.GetHashCode(), key1B.GetHashCode() };
            var hash2 = new[] { key2.GetHashCode(), key2B.GetHashCode() };
            var hash3 = new[] { key3.GetHashCode(), key3B.GetHashCode() };
            var hash4 = new[] { key4.GetHashCode(), key4B.GetHashCode() };

            //-- assert

            hash1[1].ShouldBe(hash1[0]);
            hash2[1].ShouldBe(hash2[0]);
            hash3[1].ShouldBe(hash3[0]);
            hash4[1].ShouldBe(hash4[0]);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void HashCodesOfNonIdenticalKeysDiffer()
        {
            //-- arrange

            TypeKey key1 = new TypeKey<Type>(typeof(IDisposable));
            TypeKey key2 = new TypeKey<Type, Type>(typeof(Stream), typeof(IDisposable));
            TypeKey key3 = new TypeKey<Type, Type, int>(typeof(Stream), typeof(IDisposable), 123);
            TypeKey key4 = new TypeKey<Type, Type, int, string>(typeof(Stream), typeof(IDisposable), 123, "ABC");

            //-- act

            var hash1 = key1.GetHashCode();
            var hash2 = key2.GetHashCode();
            var hash3 = key3.GetHashCode();
            var hash4 = key4.GetHashCode();

            //-- assert

            hash1.ShouldNotBe(hash2);
            hash1.ShouldNotBe(hash3);
            hash1.ShouldNotBe(hash4);

            hash2.ShouldNotBe(hash3);
            hash2.ShouldNotBe(hash4);

            hash3.ShouldNotBe(hash4);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanCompareTypeKeysOfDifferentStructure()
        {
            //-- arrange

            TypeKey key1 = new TypeKey<Type>(typeof(Stream));
            TypeKey key2 = new TypeKey<Type, Type>(typeof(Stream), typeof(IDisposable));
            TypeKey key3 = new TypeKey<Type, Type, int>(typeof(Stream), typeof(IDisposable), 123);
            TypeKey key4 = new TypeKey<Type, Type, int, string>(typeof(Stream), typeof(IDisposable), 123, "ABC");

            //-- act && assert

            key1.Equals(key2).ShouldBe(false);
            key1.Equals(key3).ShouldBe(false);
            key1.Equals(key4).ShouldBe(false);
            key4.Equals(key1).ShouldBe(false);
            key3.Equals(key1).ShouldBe(false);
            key2.Equals(key1).ShouldBe(false);
        }
    }
}
