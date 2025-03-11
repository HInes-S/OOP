using Droweroid;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace Tests
{
    public class DrowerTests
    {
        [SetUp]
        public void Setup()
        {
            Drawer.Canvas = new Canvas(120, 20, ' ');
        }

        [Test]
        public void SaveTest()
        {
            Assert.That((Drawer.Canvas.Save("D:/savefile")), Is.EqualTo(""));
        }

        [Test]
        public void LoadTest()
        {
            Assert.That((Drawer.Canvas.Load("D:/loadfile")), Is.EqualTo("Error: Can not read file"));
        }
    }
}