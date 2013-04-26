#region

using NUnit.Framework;

#endregion

namespace PartitionedMemoryCacher.Tests
{
    [TestFixture]
    public class PartitionedMemoryCacherTests
    {
        [Test]
        public void AddTest()
        {
            int radix = 5;
            int arraySize = 10;
            PartitionedMemoryCacher<string> mc = new PartitionedMemoryCacher<string>(radix, arraySize);

            mc.Add(0, "sıfır");
            mc.Add(1, "bir");
            mc.Add(2, "iki");
            mc.Add(3, "üç");
            mc.Add(4, "dört");
            mc.Add(5, "beş");
            mc.Add(6, "altı");

            Assert.AreEqual(7, mc.Count);

            Assert.AreEqual(2, mc[0].Count);

            Assert.AreEqual(2, mc[1].Count);
            Assert.AreEqual(1, mc[2].Count);
            Assert.AreEqual(1, mc[3].Count);
            Assert.AreEqual(1, mc[4].Count);
        }

        [Test]
        public void MultipleArrayTest()
        {
            int radix = 5;
            int arraySize = 10;
            PartitionedMemoryCacher<int> mc = new PartitionedMemoryCacher<int>(radix, arraySize);

            Assert.AreEqual(radix, mc.GetInternalArrayCount());
        }


        [Test]
        public void RemoveTest()
        {
            int radix = 5;
            int arraySize = 10;
            PartitionedMemoryCacher<string> mc = new PartitionedMemoryCacher<string>(radix, arraySize);

            mc.Add(0, "sıfır");
            mc.Add(1, "bir");

            //init contidions
            Assert.AreEqual(2, mc.Count);
            Assert.AreEqual(1, mc[0].Count);
            Assert.AreEqual(1, mc[1].Count);

            mc.Remove(0);

            //phase 1 conditions
            Assert.AreEqual(1, mc.Count);
            Assert.AreEqual(0, mc[0].Count);
            Assert.AreEqual(1, mc[1].Count);

            mc.Remove(1);

            //phase 2 conditions
            Assert.AreEqual(0, mc.Count);
            Assert.AreEqual(0, mc[0].Count);
            Assert.AreEqual(0, mc[1].Count);
        }

        [Test]
        public void SingleArrayTest()
        {
            int radix = 1;
            int arraySize = 10;
            PartitionedMemoryCacher<int> mc = new PartitionedMemoryCacher<int>(radix, arraySize);

            Assert.AreEqual(radix, mc.GetInternalArrayCount());
        }
    }
}